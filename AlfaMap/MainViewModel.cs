using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace AlfaMap {
    public class MainViewModel : ViewModelBase
    {
        private ExternalEvent externalEvent;
        private RevitEventHandler externalHandler;

        public AppWrapper app = null;

        private UIElement appUI = null;
        public UIElement AppUI {
            get { return appUI; }
            set {
                appUI = value;
                OnPropertyChanged();
            }
        }

        public bool Debug {
            get { return Config.Debug; }
            set {
                Config.Debug = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            externalHandler = new RevitEventHandler();
            externalEvent = ExternalEvent.Create(externalHandler);

            ReloadApp();
        }

        private void ReloadApp() {
            try {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string devAppPath = Path.Combine(dir, @"..\..\..\AlfaMapApp\bin\Debug\AlfaMapApp.dll");
                string appPath;
                if (Debug && File.Exists(devAppPath)) {
                    appPath = devAppPath;
                } else {
                    appPath = Path.Combine(dir, "AlfaMapApp.dll");
                }

                if (!File.Exists(appPath)) {
                    var dialog = new TaskDialog("Init App Error");
                    dialog.MainInstruction = "AlfaMapApp.dll doesn't exists";
                    dialog.MainContent = $"Invalid file path: {appPath}";
                    dialog.Show();
                    return;
                }

                app = new AppWrapper(appPath);
                app.UpdateDoc(Doc);
                AppUI = app.UI;
            } catch (Exception e) {
                Console.WriteLine(e);
                TaskDialog.Show("Init App Error", e.ToString());
            }
            
        }

        private Document doc;
        public Document Doc {
            get { return doc; }
            set {
                OnDocumentSwitch(value);
            }
        }

        private void OnDocumentSwitch(Document newDoc) {
            // Invalid or FamilyDocument
            if (newDoc == null || !newDoc.IsValidObject || newDoc.IsFamilyDocument) {
                doc = null;
                app?.UpdateDoc(doc);
                return;
            }

            // Same Document
            if (doc != null && doc.IsValidObject && doc.Equals(newDoc)) {
                return;
            }

            // Udapte Document
            if (doc == null || !doc.IsValidObject || !doc.Equals(newDoc)) {
                doc = newDoc;
            }

            app?.UpdateDoc(doc);
        }

        private RelayCommand runRevitCommand;
        public RelayCommand RunRevitCommand {
            get {
                return runRevitCommand ?? (runRevitCommand = new RelayCommand(obj => {
                    
                }));
            }
        }

        private RelayCommand runReloadAppCommand;
        public RelayCommand RunReloadAppCommand {
            get {
                return runReloadAppCommand ?? (runReloadAppCommand = new RelayCommand(obj => {
                    externalHandler.Method = uiapp => {
                        ReloadApp();
                    };
                    externalEvent.Raise();
                }));
            }
        }
    }
}
