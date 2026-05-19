using Autodesk.Revit.DB;
using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace AlfaMap {
    public class AppWrapper {
        private readonly object vmInstance = null;
        private readonly PropertyInfo docProperty = null;
        public UIElement UI { get; private set; } = null;
        public AppWrapper(string dllPath, bool test) {
            var pdbPath = dllPath.Replace(".dll", ".pdb");
            byte[] bytes = File.ReadAllBytes(dllPath);
            Assembly assembly;
            if (File.Exists(pdbPath)) {
                byte[] pdbbytes = File.ReadAllBytes(pdbPath);
                assembly = Assembly.Load(bytes, pdbbytes);
            } else {
                assembly = Assembly.Load(bytes);
            }
            Type vmType = assembly.GetType("AlfaMap.AppViewModel");
            Type uiType = assembly.GetType("AlfaMap.UI");
            vmInstance = Activator.CreateInstance(vmType);
            docProperty = vmType.GetProperty("Doc");

            vmType.GetProperty("Test")?.SetValue(vmInstance, test, null);

            object uiInstance = Activator.CreateInstance(uiType, new object[] { vmInstance });
            UI = uiInstance as UIElement;
            //runMethod = type.GetMethod("Run", new Type[] { typeof(UIApplication) });
        }

        public void UpdateDoc(Document doc) {
            docProperty.SetValue(vmInstance, doc, null);
        }
    }
}
