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
        public AppWrapper(string dllPath) {
            byte[] bytes = File.ReadAllBytes(dllPath);
            Assembly assembly = Assembly.Load(bytes);
            Type vmType = assembly.GetType("AlfaMap.MainViewModel");
            Type uiType = assembly.GetType("AlfaMap.UI");
            vmInstance = Activator.CreateInstance(vmType);
            docProperty = vmType.GetProperty("Doc");

            object uiInstance = Activator.CreateInstance(uiType, new object[] { vmInstance });
            UI = uiInstance as UIElement;
            //runMethod = type.GetMethod("Run", new Type[] { typeof(UIApplication) });
        }

        public void UpdateDoc(Document doc) {
            docProperty.SetValue(vmInstance, doc, null);
        }
    }
}
