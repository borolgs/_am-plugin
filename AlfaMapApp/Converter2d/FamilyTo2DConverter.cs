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

namespace AlfaMap.Converter2d
{
    class WorkplaceTo2DConverter
    {
        private readonly Options options = new Options();

        public WorkplaceTo2DConverter()
        {
            options.DetailLevel = ViewDetailLevel.Coarse;
            options.IncludeNonVisibleObjects = true;
        }

        public WorkplaceData ConvertWorkplace(ModelNode workplaceNode)
        {
            var workplace = workplaceNode.Element as FamilyInstance;

            var name = workplaceNode.Node?.name ?? workplaceNode.Element.LookupParameter("AB_W_Number")?.AsString();
            var bbox = workplace.get_BoundingBox(null);
            var data = new WorkplaceData
            {
                Id = workplaceNode.Uuid,
                Name = name,
                LegacyBoundaires = ExtractBoundaries(workplace),
                IsWorkplace = true,
                LegacyBBox = new BBoxData(bbox),
                Origin = ConvertUtils.ConvertPoint((workplace.Location as LocationPoint).Point)
            };
            return data;
        }

        public ElementData ConvertNonWorkplace(FamilyInstance element)
        {
            var bbox = element.get_BoundingBox(null);
            var data = new ElementData {
                Id = element.UniqueId,
                Name = element.Id.ToString(),
                LegacyBoundaires = ExtractBoundaries(element),
                LegacyBBox = new BBoxData(bbox),
                Origin = ConvertUtils.ConvertPoint((element.Location as LocationPoint).Point)
            };
            return data;
        }

        public DoorData ConvertDoor(FamilyInstance door) {
            var location = door.Location as LocationPoint;
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

            var boundaries = new List<List<(double X, double Y)>> { new List<(double X, double Y)>() };
            boundaries[0].Add(ConvertUtils.ConvertPoint(ptA));
            boundaries[0].Add(ConvertUtils.ConvertPoint(ptB));
            boundaries[0].Add(ConvertUtils.ConvertPoint(ptC));
            boundaries[0].Add(ConvertUtils.ConvertPoint(ptD));

            var data = new DoorData {
                Id = door.UniqueId,
                Name = "Door",
                LegacyBoundaires = boundaries,
                LegacyBBox = new BBoxData(bbox),
                Origin = ConvertUtils.ConvertPoint(location.Point)
            };
            return data;
        }

        List<List<(double X, double Y)>> ExtractBoundaries(Element element)
        {
            var boundaries = new List<List<(double X, double Y)>>();
            var solids = GetElementSymbolSolids(element);
            foreach (Solid solid in solids)
            {
                var boundary = new List<(double X, double Y)>();
                Face face = GetSolidBottomFace(solid);
                var loop = face.GetEdgesAsCurveLoops().FirstOrDefault();
                foreach (Curve edge in loop)
                {
                    if (edge is Arc) {
                        var points = edge.Tessellate().Select(p => edge.Project(p)).OrderBy(pr => pr.Parameter).Select(pr => pr.XYZPoint);
                        foreach (XYZ arcPt in points) {
                            boundary.Add(ConvertUtils.ConvertPoint(arcPt));
                        }
                    }
                    XYZ pt = edge.GetEndPoint(0);
                    boundary.Add(ConvertUtils.ConvertPoint(pt));
                }
                boundaries.Add(boundary);

            }
            return boundaries;
        }

        private Face GetSolidBottomFace(Solid solid)
        {
            foreach (Face face in solid.Faces)
            {
                if (face == null)
                    continue;

                var planarFace = face; // as PlanarFace;

                //if (planarFace == null)
                //    continue;

                bool isBottomDir = (planarFace.ComputeNormal(new UV(0.5, 0.5)).Z + 1) < 0.01;
                if (isBottomDir)
                {
                    return planarFace;
                }
            }

            return null;
        }

        private List<Solid> GetElementSymbolSolids(Element element)
        {
            var solids = new List<Solid>();

            GeometryElement geo = element.get_Geometry(options);
            foreach (GeometryObject geoObj in geo)
            {
                var instance = geoObj as GeometryInstance;
                if (instance == null)
                    continue;

                GeometryElement symbolGeo = instance.SymbolGeometry.GetTransformed(instance.Transform);
                foreach (GeometryObject symbolGeoObj in symbolGeo)
                {
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
