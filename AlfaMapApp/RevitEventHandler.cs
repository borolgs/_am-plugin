using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.IO;
using System;

namespace AlfaMap
{
    [Transaction(TransactionMode.Manual)]
    public sealed partial class RevitEventHandler : IExternalEventHandler
    {
        public delegate void Runner(UIApplication uiapp);
        //public delegate void OnOutputUpdate(string uiapp);

        public Runner Method { get; set; }
        //public OnOutputUpdate Output { get; set; }

        public void Execute(UIApplication app)
        {
            Method?.Invoke(app);
            //string commandConsoleOutString;
            //TextWriter originalConsoleOut = Console.Out;
            //using (var writer = new StringWriter()) {
            //    Console.SetOut(writer);

            //    Method?.Invoke(app);

            //    writer.Flush();
            //    commandConsoleOutString = writer.GetStringBuilder().ToString();
            //}
            //Output?.Invoke(commandConsoleOutString);
            //Console.SetOut(originalConsoleOut);
        }

        public string GetName()
        {
            return GetType().Name;
        }
    }
}
