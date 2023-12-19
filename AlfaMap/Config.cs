using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap {
    public static class Config {
        public static bool Debug {
            get {
                return Properties.Settings.Default.Debug;
            }
            set {
                Properties.Settings.Default.Debug = value;
                Properties.Settings.Default.Save();
            }
        }

        public static string AppPath {
            get {
                return Properties.Settings.Default.AppPath;
            }
            set {
                Properties.Settings.Default.AppPath = value;
                Properties.Settings.Default.Save();
            }
        }
    }
}