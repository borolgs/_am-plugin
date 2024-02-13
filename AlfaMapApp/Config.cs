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

        public static readonly Guid PaneId = new Guid("FAEF1F03-3DFC-49C4-B9FB-38562254A04B");
        public const string TabName = "AlfaMap";
        public const string PaneName = "AlfaMap";
    }
}