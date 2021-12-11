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

namespace AlfaMap
{
    class App : IExternalApplication
    {
        public App app = null;
        private MainViewModel viewModel = null;
        public AddInPage panePage = null;

        //private AppWrapper dynamicMainApp = null;

        private EventHandler<EventArgs> AppStartup;
        protected virtual void OnAppStartup(EventArgs e) {
            EventHandler<EventArgs> handler = AppStartup;
            handler?.Invoke(this, e);
        }


        public Result OnStartup(UIControlledApplication a)
        {
            OnAppStartup(new EventArgs());

            //dynamicMainApp = new AppWrapper();
            //dynamicMainApp.OnStartup(a);

            app = this;
            viewModel = new MainViewModel();
            panePage = new AddInPage(viewModel);
            a.ViewActivated += new EventHandler<ViewActivatedEventArgs>(Application_ViewActivated);
            RegisterPane(a);
            AddTab(a);
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
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

        private void AddTab(UIControlledApplication application)
        {
            var tabName = Constants.TabName;
            application.CreateRibbonTab(tabName);
            RibbonPanel panel = application.CreateRibbonPanel(tabName, tabName);

            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Show Button
            var text = $"Show {Constants.PaneName}";
            PushButtonData showBtn = new PushButtonData("Show", text, assemblyPath, "AlfaMap.ShowPaneCommand");
            showBtn.LargeImage = ImageUtils.GetImage(Resources.open_panel.GetHbitmap());
            panel.AddItem(showBtn);
        }

        private void RegisterPane(UIControlledApplication application)
        {
            DockablePaneId paneId = new DockablePaneId(Constants.PaneId);
            application.RegisterDockablePane(paneId, Constants.PaneName, panePage);
        }


/*        public void Application_StartUp(object sender, StartUp args) {
            MainViewModel viewModel = panePage.ViewModel;
            if (viewModel == null) return;

            Document newDoc = args.Document;
            try {
                viewModel.Doc = newDoc;
            } catch (Exception e) {
                Debug.WriteLine($"Application_ViewActivated Error!\n{e.ToString()}");
            }
        }*/
    }
}
