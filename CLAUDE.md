# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

Revit 2020 add-in ("AlfaMap" / "AMAP") for Alfa-Bank's office-real-estate workflow. The plugin runs inside Autodesk Revit, exposes a dockable WPF pane, and synchronises building/room/furniture data with a remote AMAP HTTP backend. It also produces 3D (THREE.js JSON) and 2D (SVG) exports of model geometry.

Target framework: **.NET Framework 4.7.2** (Revit 2020 host). Build only on Windows with Revit 2020 installed.

## Solution layout

There are two C# class-library projects in `AlfaMap.sln`:

- **`AlfaMap/`** — thin *loader* add-in. Registered with Revit via `AlfaMap.addin` (FullClassName `AlfaMap.App`). At startup it creates the ribbon tab + dockable pane, then reflectively loads `AlfaMapApp.dll` via `AppWrapper.cs` (using `Assembly.Load(bytes)` so the file isn't locked) and instantiates `AlfaMap.MainViewModel` / `AlfaMap.UI` from it. The user setting `AppPath` (in `app.config`) points the loader at a specific `AlfaMapApp.dll` — typically a network share — so the app can be updated without restarting Revit. Falls back to `AlfaMapApp.dll` next to the loader DLL.
- **`AlfaMapApp/`** — all real functionality lives here. AssemblyName `AlfaMapApp`, RootNamespace `Workplace`, but most types are under the `AlfaMap.*` namespace (the namespace the loader reflects against).

Since commit `e63b843` ("make alfamapapp standalone"), `AlfaMapApp` can *also* be registered with Revit directly via `Workplace.addin` (FullClassName `Workplace.App`, i.e. `AlfaMapApp/App.cs`), bypassing the loader. In that mode `HostViewModel` + `HostPage` (HostUI.xaml) replace the loader's `MainViewModel`/`AddInPage` and host `AppViewModel`/`UI` directly. **Both add-ins register the same `DockablePaneId` GUID `FAEF1F03-3DFC-49C4-B9FB-38562254A04B` and the same tab name `AlfaMap`** — do not install both into the same Revit at once.

`Workplace.addin` currently declares `<Assembly>Workplace/Workplace.dll</Assembly>` but the actual output is `AlfaMapApp.dll`; if you touch the standalone install path, expect that to need fixing.

## Build & install

Build is MSBuild / Visual Studio 2019+. There is no CLI test or lint target — the only quality gate is a successful build.

```
# Restore NuGet packages (packages.config style) and build the solution
nuget restore AlfaMap.sln
msbuild AlfaMap.sln /p:Configuration=Debug
```

Solution configurations: `Debug`, `Release`, plus `DebugApp` / `DebugHost` on the loader (`AlfaMap`) only — these select which `app.config` / output dir the loader uses while iterating on the standalone vs. host setup; for AlfaMapApp they map to plain `Debug`.

Each project's post-build step `xcopy`s its `.addin` manifest next to the built DLL. To install in Revit, copy the `.addin` file and the DLL(s) into `%APPDATA%\Autodesk\Revit\Addins\2020\`.

Native Revit references (`RevitAPI.dll`, `RevitAPIUI.dll`) are picked up from `C:\Program Files\Autodesk\Revit 2020\` via relative `HintPath`s — the repo lives several levels deep on the original developer's machine; on a different layout you'll need to fix those paths or set a `RevitInstallDir` indirection.

`AlfaMapApp` uses **Fody/Costura** (see `FodyWeavers.xml`) to embed `Newtonsoft.Json` and `Handlebars` into the output DLL so the standalone build is a single file. New embedded dependencies must be added there too.

## Runtime architecture

Inside Revit, control flows roughly:

1. Revit calls `IExternalApplication.OnStartup` (in `AlfaMap/App.cs` *or* `AlfaMapApp/App.cs` depending on which `.addin` is installed). The app creates a ribbon button → `ShowPaneCommand` → toggles the dockable pane.
2. The pane hosts a WPF `Page` (`AddInPage` or `HostPage`) bound to a view-model. When loaded through the *loader*, `MainViewModel.ReloadApp()` reads bytes for `AlfaMapApp.dll` (+ its `.pdb` if present), `Assembly.Load`s it, then `Activator.CreateInstance`s `AlfaMap.MainViewModel` (which is the original name of what's now `AppViewModel` inside `AlfaMapApp`) and `AlfaMap.UI` reflectively. The resulting `UIElement` is shown via `ContentPresenter`. When loaded standalone, `HostViewModel` just `new`s `AppViewModel` / `UI` directly. Either way the rest of the app sees the same `AppViewModel`.
3. `AppViewModel` (`AlfaMapApp/AppViewModel.cs`) is the orchestrator: it owns commands invoked from `UI.xaml`, the `RevitEventHandler` (Revit `ExternalEvent` pattern — *required* for any code that mutates the document, since WPF callbacks are off the Revit API thread), and the `DataSync.Handler`/`Client` that talk to the backend.
4. Revit `Application_ViewActivated` events propagate the active `Document` down through the view-model chain (loader → app, or host → app). Family documents and invalid docs are filtered out at the boundary.

### Key subsystems inside `AlfaMapApp/`

- **`DataSync/`** — HTTP client (`Client.cs`) and orchestrating `Handler.cs` for syncing the in-Revit `BuildingTree` (`State/BuildingTree.cs`) with the AMAP backend's `places`/`buildings`/`nodes` endpoints. Errors flow through `DataSyncException` + the `Result<T, E>` type in `Common/Result.cs`.
- **`Revit/`** — Revit-API wrappers: `Collector` (filtered element collectors), `ParamChecker` / `ParamCreator` (shared-parameter provisioning), `Storage`, `FailureHandler`, `Utils`. Anything that reads from / writes to a `Document` should go through here or be wrapped in an `ExternalEvent`.
- **`Shared/Parameters.cs`** — Authoritative list of shared parameter GUIDs the plugin uses (`AM_BuildingId`, `AM_PlaceId`, `AM_NodeId`, `AM_OfficeId`, …). These GUIDs are persisted in real Revit models in production — **never change a GUID** once it's been used; only add new ones.
- **`Converter/`** — Revit geometry → THREE.js JSON (`THREEConverter` produces `THREERoot` containing `THREEGeometry`/`THREEMaterial`/`THREEMesh`/`THREEGroup`/`THREEObject`/`THREEUserData`). Used by the upload-model flow.
- **`Converter2d/`** — Revit → SVG conversion at the building / level / room / family scope, plus the `UnitConverter` (Revit feet → metric).
- **`Batch/`** — `BatchConverter` + dialog for running conversion across many models at once.
- **`Snapshots/CreateSnapshothandler.cs`** — note: namespace `Workplace.Snapshots`, not `AlfaMap.Snapshots`.
- **`MVVM/`**, **`Common/`** — lightweight `RelayCommand` / `ViewModelBase` + utilities (`Result<T,E>`, matrix math, image helpers).

## Things to know before editing

- **Reflection contract loader ↔ app.** `AlfaMap/AppWrapper.cs` hard-codes the type names `AlfaMap.MainViewModel` and `AlfaMap.UI` *with property `Doc` and optional `Test`*. If you rename, move, or change the constructor signature of these, the loader-based install path will silently break (you'll see "Не удалось загрузить приложение!" in the pane). The standalone install path goes through `HostViewModel` and is not affected. Touch both when changing this contract.
- **Document mutations must go through `ExternalEvent`.** See the `externalHandler.Method = uiapp => { … }; externalEvent.Raise();` pattern in `AppViewModel.RunWorkplaceCommand`. Anything else will throw "Attempt to modify the document outside of a valid Revit API context".
- **`AppPath` user setting.** The loader's `app.config` ships with a hard-coded UNC path (`T:\IT\Офисная недвижимость\alfamap\AlfaMapApp.dll`). On dev machines without that share, set `AppPath` to empty via the settings UI or it will fail to find the DLL before falling back.
- **Resources are localized (ru/en).** Most `.resx` files have a `.ru.resx` sibling — keep both in sync when adding user-facing strings. UI defaults to Russian.
- **No tests.** There is no test project and no CI configuration. Verification is manual inside Revit.
