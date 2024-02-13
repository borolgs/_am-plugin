using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap
{
    [Transaction(TransactionMode.ReadOnly)]
    public class ShowPaneCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            DockablePaneId dpid = new DockablePaneId(Config.PaneId);
            DockablePane dp = commandData.Application.GetDockablePane(dpid);
            dp.Show();
            return Result.Succeeded;
        }
    }
}
