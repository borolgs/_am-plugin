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
    class LevelTo2DConverter
    {

        private readonly SpatialElementBoundaryOptions spatialOptions = new SpatialElementBoundaryOptions();
        private readonly IEnumerable<Area> levelAreas;

        public LevelTo2DConverter()
        {
        }

        public LevelData Convert(ModelNode levelNode)
        {
            var level = levelNode.Element as Level;

            // TODO
            var mass = new FilteredElementCollector(level.Document)
                                    .OfCategory(BuiltInCategory.OST_Mass)
                                    .WhereElementIsNotElementType()
                                    .Where(a => a.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString() == "MAIN")
                                    .First();
            var buildingBbox = mass.get_BoundingBox(null);

            var name = levelNode.Node?.name;

            var area = new FilteredElementCollector(level.Document)
                .OfCategory(BuiltInCategory.OST_Areas)
                .WhereElementIsNotElementType()
                .WherePasses(new ElementLevelFilter(level.Id))
                .Where(a => a.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)?.AsString() == "TA")
                .Cast<Area>()
                .FirstOrDefault();

            var data = new LevelData
            {
                Id = levelNode.Uuid,
                Name = name,
                Rooms = new List<RoomData>(),
                Doors = new List<DoorData>(),
                Elements = new List<ElementData>(),
                LegacyBoundaires = area != null ? ExtractBoundaires(area): new List<List<(double, double)>>(),
                LegacyBBox = new BBoxData(buildingBbox)
            };
            return data;
        }

        List<List<(double, double)>> ExtractBoundaires(SpatialElement spatial)
        {
            var boundaries = new List<List<(double X, double Y)>>();
            if (spatial == null) {
                return boundaries;
            }
            foreach (var segments in spatial.GetBoundarySegments(spatialOptions))
            {
                var boundary = new List<(double, double)>();
                foreach (var segment in segments)
                {
                    Curve crv = segment.GetCurve();
                    if (crv is Arc)
                    {
                        var pts = crv.Tessellate();
                        foreach (var pt in pts)
                        {
                            var pt2d = ConvertUtils.ConvertPoint(pt);
                            boundary.Add(pt2d);
                        }
                    }
                    else
                    {
                        var pt = crv.GetEndPoint(0);
                        var pt2d = ConvertUtils.ConvertPoint(pt);
                        boundary.Add(pt2d);
                    }
                }
                boundaries.Add(boundary);
            }
            return boundaries;
        }
    }
}
