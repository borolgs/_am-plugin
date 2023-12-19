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

        public bool Test {
            get { return Config.Debug; }
            set {
                Config.Debug = value;
                OnPropertyChanged();
            }
        }

        public string ConfigAppPath {
            get { return Config.AppPath; }
            set {
                Config.AppPath = value;
                OnPropertyChanged();
            }
        }

        private string appPath;
        public string AppPath {
            get { return appPath; }
            set {
                appPath = value;
                OnPropertyChanged();
            }
        }

        private string error;
        public string Error {
            get { return error; }
            set {
                error = value;
                OnPropertyChanged("HasError");
                OnPropertyChanged();
            }
        }

        private string errorMsg;
        public string ErrorMsg {
            get { return errorMsg; }
            set {
                errorMsg = value;
                OnPropertyChanged();
            }
        }

        public bool HasError => !string.IsNullOrEmpty(Error);

        public MainViewModel()
        {
            externalHandler = new RevitEventHandler();
            externalEvent = ExternalEvent.Create(externalHandler);

            ReloadApp();
        }

        public bool ReloadApp() {
            Error = null;
            ErrorMsg = null;
            try {
                string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                
                if (!string.IsNullOrEmpty(ConfigAppPath) && File.Exists(ConfigAppPath)) {
                    AppPath = ConfigAppPath;
                } else {
                    AppPath = Path.Combine(dir, "AlfaMapApp.dll");
                }

                if (!File.Exists(AppPath)) {
                    var dialog = new TaskDialog("Init App Error");
                    dialog.MainInstruction = "AlfaMapApp.dll doesn't exists";
                    dialog.MainContent = $"Invalid file path: {AppPath}";
                    dialog.Show();
                    Error = "Файл не найден!";
                    ErrorMsg = "Файл по указанному пути не найден";
                    return HasError;
                }

                app = new AppWrapper(appPath, Test);
                app.UpdateDoc(Doc);
                AppUI = app.UI;
            } catch (Exception e) {
                Console.WriteLine(e);
                Error = "Не удалось загрузить приложение!";
                ErrorMsg = e.ToString();
                //TaskDialog.Show("Init App Error", e.ToString());
            }

            return HasError;
            
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
