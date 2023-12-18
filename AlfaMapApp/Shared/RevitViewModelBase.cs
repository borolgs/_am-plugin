using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Shared {
    abstract public class RevitViewModelBase : ViewModelBase {
        protected ExternalEvent ExternalEvent;
        protected RevitEventHandler ExternalHandler;

        private string consoleOutput;
        public string ConsoleOutput {
            get { return consoleOutput; }
            set {
                consoleOutput = value;
                OnPropertyChanged();
            }
        }

        public RevitViewModelBase() {
            ExternalHandler = new RevitEventHandler();
            ExternalEvent = ExternalEvent.Create(ExternalHandler);
        }

        private RelayCommand runCommand;
        public RelayCommand RunCommand {
            get {
                return runCommand ?? (runCommand = new RelayCommand(command => {
                    //ExternalHandler.Output = output => {
                    //    ConsoleOutput = output;
                    //};
                    ExternalHandler.Method = uiapp => {
                        ConsoleOutput = "";
                        Run(command, uiapp);
                    };
                    ExternalEvent.Raise();
                }));
            }
        }

        abstract public void Run(object command, UIApplication uiapp);
    }
}
