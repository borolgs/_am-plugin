using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AlfaMap.DataSync;
using AlfaMap.State;
using AlfaMap.Properties;
using RevitWrapper;

namespace AlfaMap.Converter2d
{
    class SVGConverter
    {
        //private readonly BuildingTree tree; // todo delete
        private readonly LevelTo2DConverter levelConverter = new LevelTo2DConverter();
        private readonly RoomTo2DConverter roomConverter = new RoomTo2DConverter();
        private readonly WorkplaceTo2DConverter workplaceConverter = new WorkplaceTo2DConverter();
        //private readonly UIApplication uiapp;
        private readonly Document doc;

        public SVGConverter(UIApplication uiapp = null) //, 
        {
            //this.uiapp = uiapp;
            // TODO: !!!!
            if (uiapp != null)
                this.doc = uiapp.ActiveUIDocument.Document;
            //this.tree = tree;
        }


        //public void ConvertAllAndSaveToFiles()
        //{
        //    var roomNodesWithWorkplaces = this.tree.NodesByType[NodeType.Room].FindAll(n => n.Children.Count > 0);

        //    foreach (Node roomNode in roomNodesWithWorkplaces)
        //    {
        //        RoomData roomData = roomConverter.Convert(roomNode);
        //        foreach (Node workplaceNode in roomNode.Children)
        //        {
        //            var wpData = workplaceConverter.ConvertWorkplace(workplaceNode);
        //            roomData.Workplaces.Add(wpData);
        //        }

        //        var roomSvg = CreateRoomSVG(roomData);
        //        roomSvg = System.Web.HttpUtility.HtmlDecode(roomSvg);
        //        File.WriteAllText($@"C:\Users\U_M12EE\Desktop\svg\{roomData.Name}.svg", roomSvg);
        //    }
        //}


        public string ConvertLevel(ModelNode levelNode) {
            LevelData levelData = levelConverter.Convert(levelNode);
            foreach (ModelNode roomNode in levelNode.Children) {
                RoomData roomData = roomConverter.Convert(roomNode);
                foreach (ModelNode workplaceNode in roomNode.Children) {
                    var wpData = workplaceConverter.ConvertWorkplace(workplaceNode);
                    roomData.Workplaces.Add(wpData);
                }

                levelData.Rooms.Add(roomData);
            }

            var doors = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_Doors)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementLevelFilter(levelNode.Element.Id))
                .Cast<FamilyInstance>();

            foreach (var door in doors) {
                var doorData = workplaceConverter.ConvertDoor(door);
                levelData.Doors.Add(doorData);
            }

            // TODO: add elements to building tree
            var furnitures = Collector.CollectFurniture(doc, levelNode.Element.Id).OrderBy(w => {
                string workplaceNumber = w.get_Parameter(OldParams.WorkplaceNumber.Guid)?.AsString() ?? "0";
                string digitsOnly = new string(workplaceNumber.Where(c => char.IsDigit(c)).ToArray());
                int number = int.TryParse(digitsOnly, out number) ? number : 0;
                return number;
            });
            foreach (FamilyInstance element in furnitures) {
                bool isWorkplace = element.Symbol.get_Parameter(OldParams.IsWorkplaces.Guid).AsInteger() > 0;
                if (isWorkplace) continue;

                var elementData = workplaceConverter.ConvertNonWorkplace(element);
                levelData.Elements.Add(elementData);
            }

            var levelSvg = CreateLevelSVG(levelData);
            levelSvg = System.Web.HttpUtility.HtmlDecode(levelSvg);
            return levelSvg;
        }

        public string Convert(ModelNode roomNode)
        {

            RoomData roomData = roomConverter.Convert(roomNode);
            foreach (ModelNode workplaceNode in roomNode.Children)
            {
                var wpData = workplaceConverter.ConvertWorkplace(workplaceNode);
                roomData.Workplaces.Add(wpData);
            }

            var roomSvg = CreateRoomSVG(roomData);
            roomSvg = System.Web.HttpUtility.HtmlDecode(roomSvg);
            return roomSvg;
        }

        public string ConvertForce(ModelNode roomNode, List<ModelNode> workplaceNodes, string name)
        {

            RoomData roomData = roomConverter.Convert(roomNode);
            foreach (ModelNode workplaceNode in workplaceNodes)
            {
                var wpData = workplaceConverter.ConvertWorkplace(workplaceNode);
                roomData.Workplaces.Add(wpData);
            }

            var roomSvg = CreateRoomSVG(roomData, name);
            roomSvg = System.Web.HttpUtility.HtmlDecode(roomSvg);
            return roomSvg;
        }

        string CreateLevelSVG(LevelData level) {
            //TODO move to file
            string source = Resources.level_svg_template;

            var template = Handlebars.Compile(source);

            var svgData = new {
                id = level.Name,
                name = level.Name,
                path = ConvertBoundariesToPath(level.LegacyBBox, level.LegacyBoundaires),
                rooms = new List<object>(),
                doors = new List<object>(),
                elements = new List<object>(),
            };

            // TODO: add doors to building tree
            foreach (DoorData door in level.Doors) {
                var doorSvgData = new {
                    id = door.Name,
                    name = door.Name,
                    path = ConvertBoundariesToPath(level.LegacyBBox, door.LegacyBoundaires),
                    origin = CreateSvgPoint(level.LegacyBBox, door.Origin)
                };

                svgData.doors.Add(doorSvgData);
            }

            foreach (ElementData elData in level.Elements) {
                svgData.elements.Add(new {
                    id = elData.Name,
                    name = elData.Name,
                    path = ConvertBoundariesToPath(level.LegacyBBox, elData.LegacyBoundaires)
                });
            }

            foreach (RoomData room in level.Rooms) {
                var roomSvgData = new {
                    id = room.Name,
                    name = room.Name,
                    path = ConvertBoundariesToPath(level.LegacyBBox, room.LegacyBoundaires),
                    workplaces = new List<object>(),
                };

                foreach (WorkplaceData wpData in room.Workplaces) {
                    roomSvgData.workplaces.Add(new {
                        id = wpData.Name,
                        name = wpData.Name,
                        path = ConvertBoundariesToPath(level.LegacyBBox, wpData.LegacyBoundaires)
                    });
                }

                svgData.rooms.Add(roomSvgData);
            }



            var result = template(svgData);

            return result;
        }

        string CreateRoomSVG(RoomData room, string forceName = null)
        {
            //TODO move to file
            string source = Resources.room_svg_template;

            var template = Handlebars.Compile(source);

            var svgData = new
            {
                id = forceName ?? room.Name,
                width = DoubleToSVGString(room.LegacyBBox.Max.X - room.LegacyBBox.Min.X),
                height = DoubleToSVGString(room.LegacyBBox.Max.Y - room.LegacyBBox.Min.Y),
                name = forceName ?? room.Name,
                path = ConvertBoundariesToPath(room.LegacyBBox, room.LegacyBoundaires),
                workplaces = new List<object>()
            };

            foreach (WorkplaceData wpData in room.Workplaces)
            {
                svgData.workplaces.Add(new
                {
                    id = forceName != null ? forceName + "-" + wpData.Name : wpData.Name,
                    name = forceName != null ? forceName + "-" + wpData.Name : wpData.Name,
                    path = ConvertBoundariesToPath(room.LegacyBBox, wpData.LegacyBoundaires)
                }); ;
            }

            var result = template(svgData);

            return result;
        }

        string ConvertBoundariesToPath(BBoxData bbox, List<List<(double X, double Y)>> boundaries)
        {
            var paths = new List<string>();
            foreach (var boundary in boundaries)
            {
                if (boundary.Count < 3)
                {
                    Debug.Print("Invalid boundary");
                    continue;
                }

                string firstPt = CreateSvgPathPoint(bbox, boundary.First(), true);
                string otherPts = String.Join(" ", boundary.Skip(1).Select(pt => CreateSvgPathPoint(bbox, pt)));
                paths.Add($"{firstPt} {otherPts} Z");
                
            }
            return String.Join("\n", paths);
        }

        string CreateSvgPathPoint(BBoxData bbox, (double X, double Y) point, bool first = false)
        {
            var newX = point.X - bbox.Min.X;
            var newY = point.Y - bbox.Min.Y;
            var height = bbox.Max.Y - bbox.Min.Y;
            var ptStr = $"{(first ? 'M' : 'L')} {DoubleToSVGString(newX)} {DoubleToSVGString(-newY + height)}";
            return ptStr;
        }

        string[] CreateSvgPoint(BBoxData bbox, (double X, double Y) point) {
            var newX = point.X - bbox.Min.X;
            var newY = point.Y - bbox.Min.Y;
            var height = bbox.Max.Y - bbox.Min.Y;
            return new string[] { DoubleToSVGString(newX), DoubleToSVGString(-newY + height) };
        }



        private string DoubleToSVGString(double value)
        {
            return value.ToString("0.000", new CultureInfo("en-US", false));
        }
    }

}
