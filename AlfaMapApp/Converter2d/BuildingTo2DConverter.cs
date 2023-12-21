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

namespace AlfaMap.Converter2d
{
    class BuildingTo2DConverter
    {
        private readonly SpatialElementBoundaryOptions spatialOptions = new SpatialElementBoundaryOptions();
        private readonly Options options = new Options();

        public BuildingTo2DConverter() 
        {
            options.DetailLevel = ViewDetailLevel.Coarse;
            options.IncludeNonVisibleObjects = true;
            spatialOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreCenter;
        }

        public Building2D Convert(ModelNode rootNode) {
            var buildingMass = new FilteredElementCollector(rootNode.Element.Document)
                .OfCategory(BuiltInCategory.OST_Mass)
                .WhereElementIsNotElementType()
                .Where(a => a.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString() == "MAIN")
                .FirstOrDefault();

            if (buildingMass == null) {
                throw new Exception("Для здания должна быть сделана формообразующая с комментарием МAIN");
            }

            var buildingBbox = buildingMass.get_BoundingBox(null);

            var buildingData = new Building2D {
                Id = "building",
                Levels = new List<Level2D>(),
                BBox = buildingBbox,
                FormatVersion = "0.0.1"
            };

            foreach (ModelNode levelNode in rootNode.Children) {
                if (levelNode.Children.Count == 0) {
                    continue;
                }
                var area = new FilteredElementCollector(levelNode.Element.Document)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementLevelFilter(levelNode.Element.Id))
                .Where(a => a.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString() == "TA")
                .Cast<Area>()
                .FirstOrDefault();

                var levelData = new Level2D {
                    Id = levelNode.Uuid,
                    Name = "Level",
                    Elevation = (levelNode.Element as Level).Elevation,
                    Rooms = new List<Room2D>(),
                    Doors = new List<Door2D>(),
                    Elements = new List<Element2D>(),
                    Curves = new List<Curve>(),
                    Boundaries = area != null ? ExtractSpatialBoundaires(area): new List<List<(XYZ, XYZ)>>(),
                    BBox = buildingBbox
                };

                foreach (ModelNode roomNode in levelNode.Children) {
                    var room = roomNode.Element as Room;
                    var roomBbox = roomNode.Element.get_BoundingBox(null);
                    var roomData =new Room2D {
                        Id = roomNode.Uuid,
                        Name = roomNode.Node?.name,
                        Workplaces = new List<Workplace2D>(),
                        Boundaries = ExtractSpatialBoundaires(room, false),
                        CenterBoundaries = ExtractSpatialBoundaires(room, true),
                        BBox = roomBbox,
                        Origin = (room.Location as LocationPoint).Point
                    };
                    foreach (ModelNode workplaceNode in roomNode.Children) {
                        var workplace = workplaceNode.Element as FamilyInstance;

                        var workplaceBbox = workplace.get_BoundingBox(null);
                        var workplaceData = new Workplace2D {
                            Id = workplaceNode.Uuid,
                            Name = workplaceNode.Node?.name,
                            BBox = workplaceBbox,
                            Boundaries = ExtractWorkplaceBoundaries(workplace),
                            Origin = (workplace.Location as LocationPoint).Point,
                        };

                        roomData.Workplaces.Add(workplaceData);
                    }
                    levelData.Rooms.Add(roomData);
                }

                var doors = new FilteredElementCollector(levelNode.Element.Document)
                    .OfClass(typeof(FamilyInstance))
                    .OfCategory(BuiltInCategory.OST_Doors)
                    .WhereElementIsNotElementType()
                    .WherePasses(new ElementLevelFilter(levelNode.Element.Id))
                    .Cast<FamilyInstance>();

                foreach (var door in doors) {
                    var location = door.Location as LocationPoint;
                    if (location == null) {
                        Console.WriteLine($"Door without location #{door.Id}");
                        continue;
                    }
                    var bbox = door.get_BoundingBox(null);
                    var pt = location.Point;

                    var wall = door.Host as Wall;
                    double wallWidth = wall.Width;
                    double doorWidth = door.get_Parameter(BuiltInParameter.FAMILY_WIDTH_PARAM)?.AsDouble() ?? 2;

                    // A--------B
                    // |   pt   |
                    // D--------C
                    var ptA = pt.Add(door.FacingOrientation.Multiply(wallWidth / 2)).Add(-door.HandOrientation.Multiply(doorWidth / 2));
                    var ptB = ptA.Add(door.HandOrientation.Multiply(doorWidth));
                    var ptC = ptB.Add(-door.FacingOrientation.Multiply(wallWidth));
                    var ptD = ptC.Add(door.HandOrientation.Multiply(doorWidth));

                    var boundaries = new List<List<(XYZ, XYZ)>> { new List<(XYZ, XYZ)>() };

                    //boundaries[0].Add((ptA, null));
                    //boundaries[0].Add((ptB, null));
                    //boundaries[0].Add((ptC, null));
                    //boundaries[0].Add((ptD, null));

                    var doorData = new Door2D {
                        Id = door.UniqueId,
                        Name = "Door",
                        //Boundaries = boundaries,
                        BBox = bbox,
                        Origin = location.Point,
                        Hand = door.HandOrientation,
                        Face = door.FacingOrientation,
                        Width = doorWidth,
                        Depth = wallWidth
                    };

                    levelData.Doors.Add(doorData);
                }


                var furnitures = new FilteredElementCollector(levelNode.Element.Document)
                    .OfCategory(BuiltInCategory.OST_Furniture)
                    .WhereElementIsNotElementType()
                    .WherePasses(new ElementLevelFilter(levelNode.Element.Id))
                    .OrderBy(w => {
                        string workplaceNumber = w.get_Parameter(OldParams.WorkplaceNumber.Guid)?.AsString() ?? "0";
                        string digitsOnly = new string(workplaceNumber.Where(c => char.IsDigit(c)).ToArray());
                        int number = int.TryParse(digitsOnly, out number) ? number : 0;
                        return number;
                    })
                    .Cast<FamilyInstance>();

                foreach (FamilyInstance element in furnitures) {
                    bool isWorkplace = element.Symbol.get_Parameter(OldParams.IsWorkplaces.Guid).AsInteger() > 0;
                    if (isWorkplace) continue;


                    var elementBbox = element.get_BoundingBox(null);
                    var elementData = new Element2D {
                        Id = element.UniqueId,
                        Name = element.Symbol?.Name ?? "Element",
                        BBox = elementBbox,
                        Boundaries = ExtractFurnitureBoundaries(element),
                        Origin = (element.Location as LocationPoint).Point,
                    };

                    levelData.Elements.Add(elementData);
                }

                buildingData.Levels.Add(levelData);

                //var walls = new FilteredElementCollector(levelNode.Element.Document)
                //    .OfClass(typeof(Wall))
                //    .WhereElementIsNotElementType()
                //    .WherePasses(new ElementLevelFilter(levelNode.Element.Id))
                //    .Cast<Wall>();

                //foreach (Wall wall in walls) {
                //    var wallLocation = (LocationCurve)wall.Location;
                //    levelData.Curves.Add(wallLocation.Curve);
                //}
            }

            return buildingData;
        }

        List<List<(XYZ,XYZ)>> ExtractSpatialBoundaires(SpatialElement spatial, bool center = true) {
            var boundaries = new List<List<(XYZ, XYZ)>>();
            if (spatial == null) {
                return boundaries;
            }

            if (center) {
                spatialOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
            } else {
                spatialOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish;

            }

            foreach (var segments in spatial.GetBoundarySegments(spatialOptions)) {
                var boundary = new List<(XYZ, XYZ)>();
               
                foreach (var segment in segments) {
                    Curve crv = segment.GetCurve();
                    var pt = crv.GetEndPoint(0);
                    var arc = crv as Arc;
                    if (arc != null) {
                        boundary.Add((pt, crv.Evaluate(0.5, true)));
                    } else {
                        boundary.Add((pt, null));
                    }
                }
                boundaries.Add(boundary);
            }
            return boundaries;
        }

        List<List<(XYZ, XYZ)>> ExtractWorkplaceBoundaries(Element element) {
            var boundaries = new List<List<(XYZ, XYZ)>>();
            var solids = GetElementSymbolSolids(element);
            foreach (Solid solid in solids) {
                var boundary = new List<(XYZ, XYZ)>();
                Face face = GetSolidBottomFace(solid);
                var loop = face.GetEdgesAsCurveLoops().FirstOrDefault();
                foreach (Curve edge in loop) {
                    var pt = edge.GetEndPoint(0);
                    if (edge is Arc) {
                        boundary.Add((pt, edge.Evaluate(0.5, true)));
                    } else {
                        boundary.Add((pt, null));
                    }
                }
                boundaries.Add(boundary);

            }
            return boundaries;
        }

        List<List<(XYZ, XYZ)>> ExtractFurnitureBoundaries(Element element) {
            var boundaries = new List<List<(XYZ, XYZ)>>();
            var solids = GetElementSymbolSolids(element);
            if (solids.Count > 2) {
                solids = new List<Solid> { solids.Last() };
            }
            foreach (Solid solid in solids) {
                var boundary = new List<(XYZ, XYZ)>();
                Face face = GetSolidBottomFace(solid);
                var loop = face.GetEdgesAsCurveLoops().FirstOrDefault();
                foreach (Curve edge in loop) {
                    var pt = edge.GetEndPoint(0);
                    if (edge is Arc) {
                        boundary.Add((pt, edge.Evaluate(0.5, true)));
                    } else {
                        boundary.Add((pt, null));
                    }
                }
                boundaries.Add(boundary);

            }
            return boundaries;
        }

        private Face GetSolidBottomFace(Solid solid) {
            foreach (Face face in solid.Faces) {
                if (face == null)
                    continue;

                var planarFace = face; // as PlanarFace;

                //if (planarFace == null)
                //    continue;

                bool isBottomDir = (planarFace.ComputeNormal(new UV(0.5, 0.5)).Z + 1) < 0.01;
                if (isBottomDir) {
                    return planarFace;
                }
            }

            return null;
        }

        private List<Solid> GetElementSymbolSolids(Element element) {
            var solids = new List<Solid>();

            GeometryElement geo = element.get_Geometry(options);
            foreach (GeometryObject geoObj in geo) {
                var instance = geoObj as GeometryInstance;
                if (instance == null)
                    continue;

                GeometryElement symbolGeo = instance.SymbolGeometry.GetTransformed(instance.Transform);
                foreach (GeometryObject symbolGeoObj in symbolGeo) {
                    var solid = symbolGeoObj as Solid;

                    if (solid == null)
                        continue;
                    if (solid.Volume <= 0 || solid.Faces.Size < 1)
                        continue;

                    solids.Add(solid);
                }
            }

            solids = solids.OrderBy(s => s.Volume).ToList();
            return solids;
        }
    }

}
