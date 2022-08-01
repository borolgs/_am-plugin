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
    class RoomTo2DConverter
    {

        private readonly SpatialElementBoundaryOptions spatialOptions;

        public RoomTo2DConverter()
        {
            spatialOptions = new SpatialElementBoundaryOptions();
            spatialOptions.SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center;
        }

        public RoomData Convert(ModelNode roomNode)
        {
            // TODO check NodeType.Room

            var room = roomNode.Element as Room;

            var name = roomNode.Node?.name;

            var bbox = room.get_BoundingBox(null);
            var data = new RoomData
            {
                Id = roomNode.Uuid,
                Name = name,
                Workplaces = new List<WorkplaceData>(),
                Elements = new List<ElementData>(),
                LegacyBoundaires = ExtractBoundaires(room),
                LegacyBBox = new BBoxData(bbox)
            };
            return data;
        }

        List<List<(double, double)>> ExtractBoundaires(SpatialElement spatial)
        {
            var boundaries = new List<List<(double X, double Y)>>();
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
