using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlfaMap.Revit {
    public static class ParamChecker {
        public static bool CheckAndCreateParams(Document doc, List<ParamData> parameters, bool ownTransaction = true) {

            var parsToAdd = new List<ParamData>();
            foreach (ParamData parData in parameters) {
                try {
                    if (SharedParameterElement.Lookup(doc, parData.Guid) == null) {
                        parsToAdd.Add(parData);
                    }
                } catch (Exception e) {
                    Console.WriteLine(e);
                }
            }
            if (parsToAdd.Count > 0) {
                var dialog = new TaskDialog("Недостающие параметры");
                dialog.MainInstruction = $"Добавить в проект?\n{String.Join(", ", parsToAdd.Select(p => p.Name))}.";
                dialog.CommonButtons = TaskDialogCommonButtons.Ok;
                TaskDialogResult result = dialog.Show();
                if (result != TaskDialogResult.Ok) return false;

                if (ownTransaction) {
                    using (Transaction tx = new Transaction(doc)) {
                        try {
                            tx.Start("Добавление параметров в проект");
                            Create();
                            tx.Commit();
                        } catch (Exception e) {
                            tx.RollBack();
                            throw e;
                        }
                    }
                } else {
                    Create();
                }
            }
            return true;

            void Create() {
                var creator = new ParamCreator(doc);
                foreach (ParamData paramData in parsToAdd) {
                    creator.Create(paramData);
                }
                doc.Regenerate();
            }
        }

        public static bool CheckParams(Document doc, List<ParamData> parameters) {

            var parsToAdd = new List<ParamData>();
            foreach (ParamData parData in parameters) {
                if (SharedParameterElement.Lookup(doc, parData.Guid) == null) {
                    parsToAdd.Add(parData);
                }
            }
            if (parsToAdd.Count > 0) {
                return false;
            }

            return true;
        }
    }
}