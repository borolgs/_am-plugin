using System;
using System.Collections.Generic;
using System.Reflection;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AlfaMap.Common;
using AlfaMap.Properties;
using System.IO;
using Autodesk.Revit.UI.Events;
using Common;

namespace AlfaMap {
    class App : IExternalApplication {
        public App app = null;
        private HostViewModel viewModel = null;
        public HostPage panePage = null;

        private EventHandler<EventArgs> AppStartup;
        protected virtual void OnAppStartup(EventArgs e) {
            EventHandler<EventArgs> handler = AppStartup;
            handler?.Invoke(this, e);
        }


        public Result OnStartup(UIControlledApplication a) {
            OnAppStartup(new EventArgs());

            app = this;
            viewModel = new HostViewModel();
            panePage = new HostPage(viewModel);
            a.ViewActivated += new EventHandler<ViewActivatedEventArgs>(Application_ViewActivated);
            RegisterPane(a);
            AddTab(a);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a) {
            app = null;
            panePage = null;
            viewModel = null;
            a.ViewActivated -= new EventHandler<ViewActivatedEventArgs>(Application_ViewActivated);
            return Result.Succeeded;
        }

        public void Application_ViewActivated(object sender, ViewActivatedEventArgs args) {
            if (viewModel == null) return;

            Document newDoc = args.Document;
            try {
                viewModel.Doc = newDoc;
            } catch (Exception e) {
                Console.WriteLine($"Application_ViewActivated Error!\n{e.ToString()}");
            }
        }

        private void AddTab(UIControlledApplication application) {
            var tabName = Config.TabName;
            application.CreateRibbonTab(tabName);
            RibbonPanel panel = application.CreateRibbonPanel(tabName, tabName);

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Show Button
            var text = $"Show {Config.PaneName}";
            PushButtonData showBtn = new PushButtonData("Show", text, assemblyPath, "AlfaMap.ShowPaneCommand");
            showBtn.LargeImage = ImageUtils.GetImage(Resources.open_panel.GetHbitmap());
            panel.AddItem(showBtn);
        }

        private void RegisterPane(UIControlledApplication application) {
            DockablePaneId paneId = new DockablePaneId(Config.PaneId);
            application.RegisterDockablePane(paneId, Config.PaneName, panePage);
        }
    }
}
