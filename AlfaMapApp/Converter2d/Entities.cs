using Autodesk.Revit.DB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AlfaMap.Converter2d {


    public class XYZConverter : JsonConverter<XYZ> {
        public override XYZ ReadJson(JsonReader reader, Type objectType, XYZ existingValue, bool hasExistingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, XYZ value, JsonSerializer serializer) {
            writer.WriteRawValue(JsonConvert.SerializeObject(new[] {
                value.X, value.Y, value.Z
            }.Select(v => ConvertUtils.Convert(v)).ToArray()));

        }
    }

    public class BBoxConverter : JsonConverter<BoundingBoxXYZ> {
        public override BoundingBoxXYZ ReadJson(JsonReader reader, Type objectType, BoundingBoxXYZ existingValue, bool hasExistingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, BoundingBoxXYZ value, JsonSerializer serializer) {
            writer.WriteRawValue(JsonConvert.SerializeObject(new[] { 
                new []{ value.Min.X, value.Min.Y, value.Min.Z }.Select(v => ConvertUtils.Convert(v)).ToArray(),
                new []{ value.Max.X, value.Max.Y, value.Max.Z }.Select(v => ConvertUtils.Convert(v)).ToArray()
            }));

        }
    }

    public class BoundariesConverter : JsonConverter<List<List<(XYZ firstPt, XYZ arcMiddlePt)>>> {
        public override List<List<(XYZ firstPt, XYZ arcMiddlePt)>> ReadJson(JsonReader reader, Type objectType, List<List<(XYZ firstPt, XYZ arcMiddlePt)>> existingValue, bool hasExistingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, List<List<(XYZ firstPt, XYZ arcMiddlePt)>> value, JsonSerializer serializer) {
            writer.WriteRawValue(JsonConvert.SerializeObject(
                value.Select(segment => segment.Select((v) => {
                     var points = new List<double> {
                        ConvertUtils.Convert(v.firstPt.X),
                        ConvertUtils.Convert(v.firstPt.Y),
                        ConvertUtils.Convert(v.firstPt.Z),
                    };
                    if (v.arcMiddlePt != null) {
                        points.Add(ConvertUtils.Convert(v.arcMiddlePt.X));
                        points.Add(ConvertUtils.Convert(v.arcMiddlePt.Y));
                        points.Add(ConvertUtils.Convert(v.arcMiddlePt.Z));
                    }
                    return points;
                })).ToArray()
            ));
        }
    }

    public class CurveConverter : JsonConverter<List<Curve>> {
        public override List<Curve> ReadJson(JsonReader reader, Type objectType, List<Curve> existingValue, bool hasExistingValue, JsonSerializer serializer) {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, List<Curve> value, JsonSerializer serializer) {
            writer.WriteRawValue(JsonConvert.SerializeObject(
                value.Select((curve) => {
                    XYZ firstPt = curve.GetEndPoint(0);
                    XYZ lastPt = curve.GetEndPoint(1);
                    var points = new List<double> {
                        ConvertUtils.Convert(firstPt.X),
                        ConvertUtils.Convert(firstPt.Y),
                        ConvertUtils.Convert(firstPt.Z),
                        ConvertUtils.Convert(lastPt.X),
                        ConvertUtils.Convert(lastPt.Y),
                        ConvertUtils.Convert(lastPt.Z),
                    };
                    if (curve is Arc) {
                        XYZ arcMiddlePt = curve.Evaluate(0.5, true);
                        points.Add(ConvertUtils.Convert(arcMiddlePt.X));
                        points.Add(ConvertUtils.Convert(arcMiddlePt.Y));
                        points.Add(ConvertUtils.Convert(arcMiddlePt.Z));
                    }
                    return points;
                }).ToArray()
            ));
        }
    }

    abstract class Entity {
        abstract public string Type { get; }
        public string Id { get; set; }
        public string Name { get; set; }
        public XYZ Origin { get; set; }
        [JsonProperty("bbox")]
        public BoundingBoxXYZ BBox { get; set; }
        public List<List<(XYZ, XYZ)>> Boundaries { get; set; }
    }

    class Building2D : Entity {
        public override string Type => "building";
        public List<Level2D> Levels { get; set; }
        public string FormatVersion { get; set; }
    }

    class Level2D : Entity {
        public override string Type => "level";
        public double Elevation { get; set; }
        public List<Room2D> Rooms { get; set; }
        public List<Door2D> Doors { get; set; }
        public List<Element2D> Elements { get; set; }
        public List<Curve> Curves { get; set; }
    }

    class Room2D : Entity {
        public override string Type => "room";
        public List<Workplace2D> Workplaces { get; set; }
        public List<List<(XYZ, XYZ)>> CenterBoundaries { get; set; }
    }

    class Workplace2D : Entity {
        public override string Type => "workplace";
    }

    class Door2D : Entity {
        public override string Type => "door";
        public double Depth { get; set; }
        public double Width { get; set; }
        public XYZ Hand { get; set; }
        public XYZ Face { get; set; }

    }

    class Element2D : Entity {
        public override string Type => "element";
    }

    class BuildingData {
        public const string Type = "building";
        public List<LevelData> Levels { get; set; }
        [JsonIgnore]
        public BBoxData LegacyBBox { get; set; }
        public (double X, double Y) Origin { get; set; }
    }

    class LevelData {
        public const string Type = "level";
        public string Id { get; set; }
        public string Name { get; set; }
        public double Elevation { get; set; }
        public List<RoomData> Rooms { get; set; }
        public List<DoorData> Doors { get; set; }
        public List<ElementData> Elements { get; set; }
        public List<List<(double, double)>> LegacyBoundaires { get; set; }
        [JsonIgnore]
        public BBoxData LegacyBBox { get; set; }
        public (double X, double Y) Origin { get; set; }
    }

    class RoomData {
        public const string Type = "room";
        public string Id { get; set; }
        public string Name { get; set; }
        public List<WorkplaceData> Workplaces { get; set; }
        public List<ElementData> Elements { get; set; }
        [JsonIgnore]
        public List<List<(double, double)>> LegacyBoundaires { get; set; }
        [JsonIgnore]
        public BBoxData LegacyBBox { get; set; }
        public (double X, double Y) Origin { get; set; }

    }

    class WorkplaceData {
        public const string Type = "workplace";
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsWorkplace { get; set; } = false;
        [JsonIgnore]
        public List<List<(double X, double Y)>> LegacyBoundaires { get; set; }
        [JsonIgnore]
        public BBoxData LegacyBBox { get; set; }
        public (double X, double Y) Origin { get; set; }
    }

    class DoorData {
        public const string Type = "door";
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public BBoxData LegacyBBox { get; set; }
        public (double X, double Y) Origin { get; set; }
        [JsonIgnore]
        public List<List<(double X, double Y)>> LegacyBoundaires { get; set; }
        public List<(double X, double Y)> Lines { get; set; }
    }

    class ElementData {
        public const string Type = "element";
        public string Id { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public BBoxData LegacyBBox { get; set; }
        public (double X, double Y) Origin { get; set; }
        [JsonIgnore]
        public List<List<(double X, double Y)>> LegacyBoundaires { get; set; }
        public List<(double X, double Y)> Lines { get; set; }
    }


    class BBoxData {
        public BBoxData(BoundingBoxXYZ bbox) {
            Min = (ConvertUtils.Convert(bbox.Min.X), ConvertUtils.Convert(bbox.Min.Y));
            Max = (ConvertUtils.Convert(bbox.Max.X), ConvertUtils.Convert(bbox.Max.Y));
        }
        public (double X, double Y) Min { get; set; }
        public (double X, double Y) Max { get; set; }
    }

}
