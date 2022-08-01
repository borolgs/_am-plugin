using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlfaMap;

namespace RevitWrapper
{
    public static class Collector
    {
        public static FilteredElementCollector CollectLevels(Document doc)
        {
            return new FilteredElementCollector(doc).OfClass(typeof(Level));
        }

        public static FilteredElementCollector CollectRooms(Document doc, ElementId levelId)
        {
            var collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);


            var activeOption = DesignOption.GetActiveDesignOptionId(doc);
            var edo_filter = new ElementDesignOptionFilter(activeOption);
            collector.WherePasses(edo_filter);

            var areaValProvider = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_AREA));
            var filter_greater = new FilterNumericGreater();
            var filter_rule = new FilterDoubleRule(areaValProvider, filter_greater, 0, 0);
            var area_param_filter = new ElementParameterFilter(filter_rule);
            collector.WherePasses(area_param_filter);

            collector.WherePasses(new ElementLevelFilter(levelId));

            return collector;
        }

        public static FilteredElementCollector CollectRooms(Document doc)
        {
            var collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Rooms);


            var activeOption = DesignOption.GetActiveDesignOptionId(doc);
            var edo_filter = new ElementDesignOptionFilter(activeOption);
            collector.WherePasses(edo_filter);

            var areaValProvider = new ParameterValueProvider(new ElementId(BuiltInParameter.ROOM_AREA));
            var filter_greater = new FilterNumericGreater();
            var filter_rule = new FilterDoubleRule(areaValProvider, filter_greater, 0, 0);
            var area_param_filter = new ElementParameterFilter(filter_rule);
            collector.WherePasses(area_param_filter);

            return collector;
        }

        public static IEnumerable<FamilyInstance> CollectWorkplaces(Document doc)
        {
            var collector = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Furniture).WhereElementIsNotElementType();
            var workplaces = collector.Cast<FamilyInstance>().Where(e =>
            {
                var isW = e.Symbol.get_Parameter(OldParams.IsWorkplaces.Guid).AsInteger();
                return isW > 0;
            });
            return workplaces;
        }

        public static FilteredElementCollector CollectFurniture(Document doc, ElementId levelId)
        {
            var collector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Furniture)
                .WhereElementIsNotElementType();
            collector.WherePasses(new ElementLevelFilter(levelId));
            return collector;
        }

        public static FilteredElementCollector CollectFurnitureFamilyInstances(Document doc, ElementId levelId)
        {
            var collector = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Furniture)
                .WhereElementIsNotElementType();
            collector.WherePasses(new ElementLevelFilter(levelId));
            return collector;
        }

        public static FilteredElementCollector CollectFamilyInstances(Document doc)
        {
            var collector = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Furniture)
                .WhereElementIsNotElementType();
            return collector;
        }

        public static IEnumerable<Element> CollectNonInteractiveElements(Document doc, ElementId levelId)
        {
            ElementMulticategoryFilter filter = new ElementMulticategoryFilter(
                new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_Walls,
                    BuiltInCategory.OST_Roofs,
                    BuiltInCategory.OST_Floors,
                    BuiltInCategory.OST_Columns,
                    BuiltInCategory.OST_StructuralColumns,
                    BuiltInCategory.OST_CurtainWallMullions,
                    BuiltInCategory.OST_CurtainWallPanels,
                    BuiltInCategory.OST_Stairs,
                }
            );
            var collector = new FilteredElementCollector(doc).WherePasses(filter).WhereElementIsNotElementType();

            collector.WherePasses(new ElementLevelFilter(levelId));

            var elements = collector.Where(e =>
            {
                var cat = (BuiltInCategory)e.Category.Id.IntegerValue;
                if (cat == BuiltInCategory.OST_Walls)
                {
                    var wall = e as Wall;
                    var typeName = wall.WallType.Name;
                    bool skip = typeName == "INT-C-CON.12.5_перегородка"
                      || typeName == "INT-C-CON.12.5_перегородка"
                      || typeName == "INT-C-CON.20_перегородка с/у"
                      || typeName == "INT-P-WPB.20";

                    if (skip) return false;
                }

                return true;
            });

            var inPlaceFamilyInstances = CollectModelInPlaceFamilyInstances(doc, levelId);
            elements = elements.Concat(inPlaceFamilyInstances);

            return elements;
        }

        public static IEnumerable<Element> CollectModelInPlaceFamilyInstances(Document doc, ElementId levelId)
        {
            var levels = new FilteredElementCollector(doc).OfClass(typeof(Level)).WhereElementIsNotElementType();

            var collector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).WhereElementIsNotElementType();

            var elements = collector.Where(e =>
            {
                if ((e as FamilyInstance).Symbol.Family.IsInPlace)
                {
                    var cat = (BuiltInCategory)e.Category.Id.IntegerValue;
                    if (cat == BuiltInCategory.OST_Furniture)
                    {
                        if (FindClosestLevel(e).Id == levelId)
                        {
                            return true;
                        }
                    }
                }
                return false;
            });
            return elements;

            Level FindClosestLevel(Element element)
            {
                double zLoc = element.get_BoundingBox(null).Min.Z;

                Level closestLevel = null;
                double minDist = Double.MaxValue;
                foreach (Level level in levels)
                {
                    double dist = Math.Abs(zLoc - level.Elevation);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestLevel = level;
                    }
                }
                return closestLevel;
            }
        }

        public static FilteredElementCollector CollectMultiLevelElements(Document doc)
        {
            ElementMulticategoryFilter filter = new ElementMulticategoryFilter(
                new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_Stairs,
                }
            );
            var collector = new FilteredElementCollector(doc).WherePasses(filter).WhereElementIsNotElementType();

            return collector;
        }
    }
}
