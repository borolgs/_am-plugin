using AlfaMap.Converter2d;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Visual;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Workplace.Snapshots
{
    public class CreateSnapshothandler
    {
        private UIApplication uiapp;
        private Document doc;

        public CreateSnapshothandler(UIApplication uiapp)
        {
            this.uiapp = uiapp;
            this.doc = uiapp.ActiveUIDocument.Document;
        }


        public void Run()
        {
            var elements = new FilteredElementCollector(doc).WhereElementIsNotElementType().ToList();

            var parIds = new HashSet<int>();
            var typeIds = new HashSet<int>();


            var typeObjects = new List<object>();
            var paramObjects = new List<object>();

            var elementObjects = new List<object>();
            var valuesObjects = new List<object>();

            foreach (var element in elements)
            {
                var symbol = element.Document.GetElement(element.GetTypeId());

                var symbolId = symbol?.Id.IntegerValue ?? -1;
                if (symbolId >= 0 && !typeIds.Contains(symbolId))
                {
                    var typeObject = new object[] {
                        "type", symbol.Id.IntegerValue, symbol.UniqueId, "TODO_ADD_TYPE_DATA",
                    };
                    typeObjects.Add(typeObject);

                    typeIds.Add(symbolId);
                }

                var elementObj = new object[] {
                    "element", element.Id, element.UniqueId, symbol?.Id.IntegerValue ?? null,
                };
                elementObjects.Add(elementObj);

                var parameters = element.Parameters;

                foreach (Parameter parameter in parameters)
                {
                    try
                    {
                        var def = parameter.Definition;
                        var name = def?.Name ?? "-";

                        string guid = null;
                        if (parameter.IsShared)
                        {
                            guid = parameter.GUID.ToString();
                        }

                        string builtIn = null;
                        if (def is InternalDefinition iternalDef)
                        {
                            builtIn = iternalDef.BuiltInParameter.ToString();
                        }

                        string type = "None";
                        string value = "";
                        switch (parameter.StorageType)
                        {
                            case StorageType.None:
                                type = "None";
                                value = parameter.AsString();
                                break;
                            case StorageType.Integer:
                                type = "Integer";
                                value = parameter.AsInteger().ToString();
                                break;
                            case StorageType.Double:
                                type = "Double";
                                value = parameter.AsDouble().ToString();
                                break;
                            case StorageType.String:
                                type = "String";
                                value = parameter.AsString();
                                break;
                            case StorageType.ElementId:
                                type = "ElementId";
                                ElementId id = parameter.AsElementId();
                                value = id.IntegerValue.ToString();
                                break;
                        }

                        int parId = parameter.Element.Id.IntegerValue;
                        if (!parIds.Contains(parId)) {
                            parIds.Add(parId);
                            var paramObject = new object[] {
                                "parameter", name, guid, builtIn, parIds, type
                            };

                            paramObjects.Add(paramObject);
                        }

                        

                        var valueObject = new object[] {
                           element.UniqueId, parameter.Id.IntegerValue, value
                        };
                        valuesObjects.Add(valueObject);
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            var jsonSettings = new JsonSerializerSettings
            {
                //ContractResolver = new CamelCasePropertyNamesContractResolver(),
               
                //NullValueHandling = NullValueHandling.Ignore,
            };

            var all = new List<List<object>> { 
                typeObjects, paramObjects, elementObjects, valuesObjects
            };
            var aaa = all.SelectMany(a => a).Distinct().ToList();

            var json = JsonConvert.SerializeObject(aaa, jsonSettings);

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{doc.Title}.json");
            File.WriteAllText(path, json, Encoding.UTF8);

        }

    }

}
