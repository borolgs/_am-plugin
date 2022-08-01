using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AlfaMap
{
    [Transaction(TransactionMode.Manual)]
    public sealed partial class RevitEventHandler : IExternalEventHandler
    {
        public delegate void Runner(UIApplication uiapp);

        public Runner Method { get; set; }

        public void Execute(UIApplication app)
        {
            Method?.Invoke(app);
        }

        public string GetName()
        {
            return GetType().Name;
        }
    }
}
