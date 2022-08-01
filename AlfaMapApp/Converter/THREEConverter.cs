#region Namespaces
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System.Resources;
using System.Reflection;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using WPF = System.Windows;
using System.Linq;
using Autodesk.Revit.DB.Architecture;
using Newtonsoft.Json;
using RevitWrapper;
using Common;
//using LanguageExt;
//using static LanguageExt.Prelude;
using AlfaMap.Connector;
using Newtonsoft.Json.Converters;
using AlfaMap.State;
#endregion

namespace AlfaMap.Converter
{

    public sealed partial class THREEConverter
    {
        private readonly Options options = new Options();
        private readonly Document doc;

        private readonly BuildingTree tree;

        public THREERoot Root;
        private readonly Dictionary<string, THREEGeometry> geometries = new Dictionary<string, THREEGeometry>();
        private readonly Dictionary<string, THREEObject> objects = new Dictionary<string, THREEObject>();

        private List<ElementId> levelsIds = new List<ElementId>();
        private List<ElementId> roomIds = new List<ElementId>();
        private List<ElementId> workplaceIds = new List<ElementId>();
        private Dictionary<ElementId, string> symbolGeometries = new Dictionary<ElementId, string>();
        private List<ElementId> flippedInstances = new List<ElementId>();

        public THREEConverter(Document doc, BuildingTree tree)
        {
            this.doc = doc;
            options.DetailLevel = ViewDetailLevel.Medium;
            this.tree = tree;
        }

        private void AddObject(string key, THREEObject obj)
        {
            objects[key] = obj;
        }

        public void Convert()
        {
            THREERoot root = new THREERoot();
            foreach (ModelNode levelNode in tree.Root.Children)
            {
                THREEGroup levelGroup = ConvertLevel(levelNode);

                ConvertRooms(levelNode, levelGroup);
                ConvertWorkplaces(levelNode, levelGroup);
                ConvertNonInteractiveEntities(levelNode, levelGroup);

                var level = levelNode.Element as Level;
                THREEGroup elementsGroup = ConvertNonInteractiveElements(level);
                if (elementsGroup != null)
                    levelGroup.children.Add(elementsGroup);

                if (levelGroup.children.Count > 0)
                    root.@object.children.Add(levelGroup);
            }

            THREEGroup multiLevelGroup = ConvertNonInteractiveElements();
            if (multiLevelGroup != null && multiLevelGroup.children.Count > 0)
                root.@object.children.Add(multiLevelGroup);


            root.geometries = geometries.Values.ToList();

            this.Root = root;
        }

        private THREEGroup ConvertLevel(ModelNode level)
        {
            levelsIds.Add(level.Id);
            THREEGroup group = THREEGroup.FromLevel(level.Element as Level);
            AddObject(level.Uuid, group);
            return group;
        }

        public void ConvertRooms(ModelNode level, THREEGroup levelGroup)
        {
            List<ModelNode> rooms = level.Children;
            if (rooms.Count < 1) return;


            THREEGroup roomGroup = THREEGroup.Create($"{level.Uuid}__Rooms", ElementType.RoomGroup);
            AddObject(roomGroup.name, roomGroup);

            foreach (ModelNode roomNode in rooms)
            {
                Room room = roomNode.Element as Room;
                var solids = GetElementSolids(room);
                if (solids.Count < 1) throw new Exception("Invalid room!");
                roomIds.Add(roomNode.Id);

                THREEGeometry geometry = ConvertSolidsToThreeGeometry(solids);
                geometries[geometry.uuid] = geometry;

                THREEMesh mesh = THREEMesh.FromRoom(room, geometry.uuid);
                AddObject(roomNode.Uuid, mesh);

                roomGroup.children.Add(mesh);

                //ConvertWorkplaces(roomNode, levelGroup);
            }

            
            levelGroup.children.Add(roomGroup);
        }

        public void ConvertWorkplaces(ModelNode level, THREEGroup levelGroup)
        {
            if (level.Children.Count < 1) return;

            THREEGroup workplaceGroup = THREEGroup.Create($"{levelGroup.name}__Workplaces", ElementType.WorkplaceGroup);
            AddObject(workplaceGroup.name, workplaceGroup);

            foreach (ModelNode room in level.Children)
            {
                foreach (FamilyInstance workplace in room.Children.Select(n => n.Element as FamilyInstance))
                {
                    var n = workplace.get_Parameter(OldParams.WorkplaceNumber.Guid).AsString();
                    workplaceIds.Add(workplace.Id);

                    THREEGeometry geometry;
                    if (symbolGeometries.ContainsKey(workplace.Symbol.Id))
                    {
                        string uuid = symbolGeometries[workplace.Symbol.Id];
                        geometry = geometries[uuid];
                    }
                    else
                    {
                        if (workplace.Mirrored) flippedInstances.Add(workplace.Id);
                        var solids = GetFamilyInstanceOriginalSolids(workplace);
                        geometry = ConvertSolidsToThreeGeometry(solids);
                        geometries[geometry.uuid] = geometry;
                        symbolGeometries[workplace.Symbol.Id] = geometry.uuid;
                    }

                    THREEMesh mesh = THREEMesh.FromWorkplace(workplace, geometry.uuid);
                    AddObject(workplace.UniqueId, mesh);

                    mesh.UpdateMatrix(workplace);

                    workplaceGroup.children.Add(mesh);
                }
            }

            if (workplaceGroup.children.Count < 1) return;

            levelGroup.children.Add(workplaceGroup);

        }

        public void ConvertNonInteractiveEntities(ModelNode level, THREEGroup levelGroup)
        {
            if (level.Children.Count < 1) return;

            THREEGroup entityGroup = THREEGroup.Create($"{levelGroup.name}__Entities", ElementType.EntityGroup);
            AddObject(entityGroup.name, entityGroup);

            var collector = Collector.CollectFurnitureFamilyInstances(this.doc, level.Element.Id).Cast<FamilyInstance>().Where(e =>
            {
                var isW = e.Symbol.get_Parameter(OldParams.IsWorkplaces.Guid).AsInteger();
                return isW < 1;
            });
            foreach (FamilyInstance instance in collector)
            {
                THREEGeometry geometry;
                if (symbolGeometries.ContainsKey(instance.Symbol.Id))
                {
                    string uuid = symbolGeometries[instance.Symbol.Id];
                    geometry = geometries[uuid];
                }
                else
                {
                    var solids = GetFamilyInstanceOriginalSolids(instance);
                    geometry = ConvertSolidsToThreeGeometry(solids);
                    geometries[geometry.uuid] = geometry;
                    symbolGeometries[instance.Symbol.Id] = geometry.uuid;
                }

                THREEMesh mesh = THREEMesh.FromFamilyInstance(instance, geometry.uuid);
                AddObject(instance.UniqueId, mesh);

                mesh.UpdateMatrix(instance);

                entityGroup.children.Add(mesh);
            }
            

            if (entityGroup.children.Count < 1) return;

            levelGroup.children.Add(entityGroup);

        }

        public THREEGroup ConvertNonInteractiveElements(Level level)
        {
            IEnumerable<Element> collector = Collector.CollectNonInteractiveElements(doc, level.Id);
            if (collector.Count() < 1) return null;

            // TODO: remame?
            // TODO: Add group types: Rooms, Workplaces, Elements
            THREEGroup group = THREEGroup.Create($"{level.UniqueId}__Elements", ElementType.ElementGroup);
            AddObject(group.name, group);

            var solids = new List<Solid>();
            var structuralSolids = new List<Solid>();
            var transparentSolids = new List<Solid>();
            foreach (var element in collector)
            {
                // TODO add GetElementSymbolSolids to GetElementSolids
                var cat = (BuiltInCategory)element.Category.Id.IntegerValue;
                if (cat == BuiltInCategory.OST_CurtainWallPanels)
                {
                    var transparentElementSolids = GetElementSymbolSolids(element);
                    if (transparentElementSolids.Count > 0)
                        transparentSolids.AddRange(transparentElementSolids);

                    continue;
                }

                if (cat == BuiltInCategory.OST_CurtainWallMullions)
                {
                    var mulonSolids = GetElementSymbolSolids(element);
                    if (mulonSolids.Count > 0)
                        solids.AddRange(mulonSolids);

                    continue;
                }

                //if (cat == BuiltInCategory.OST_Walls || cat == BuiltInCategory.OST_Columns || cat == BuiltInCategory.OST_StructuralColumns || cat == BuiltInCategory.OST_Floors)
                //{
                //    var elStrSolids = GetElementSolids(element);
                //    if (elStrSolids.Count > 0)
                //        structuralSolids.AddRange(elStrSolids);

                //    continue;
                //}

                var elSolids = GetElementSolids(element);
                if (elSolids.Count > 0)
                    solids.AddRange(elSolids);
            }

            THREEGeometry geometry = ConvertSolidsToThreeGeometry(solids);
            geometries[geometry.uuid] = geometry;

            THREEMesh mesh = THREEMesh.FromLevel(level, geometry.uuid);
            AddObject(mesh.name, mesh);

            group.children.Add(mesh);

            // Transaprent elements
            THREEGeometry trGeometry = ConvertSolidsToThreeGeometry(transparentSolids);
            geometries[trGeometry.uuid] = trGeometry;

            THREEMesh trMesh = THREEMesh.FromLevel(level, trGeometry.uuid, THREEMaterial.DefaultTransparentMaterialUuid, "tr");
            AddObject(trMesh.name, trMesh);

            group.children.Add(trMesh);

            // Structural elements
            //THREEGeometry structuralGeometry = ConvertSolidsToThreeGeometry(structuralSolids);
            //geometries[structuralGeometry.uuid] = structuralGeometry;

            //THREEMesh structuralMesh = THREEMesh.FromLevel(level, structuralGeometry.uuid, "Structural", THREEMaterial.DefaultMaterialUuid, ElementType.StructuralElement);
            //AddObject(structuralMesh.name, structuralMesh);

            //group.children.Add(structuralMesh);


            return group;
        }

        public THREEGroup ConvertNonInteractiveElements()
        {
            IEnumerable<Element> collector = Collector.CollectMultiLevelElements(doc);
            if (collector.Count() < 1) return null;

            THREEGroup group = THREEGroup.Create($"MultiLevelGroup", ElementType.MultiLevelGroup);
            AddObject(group.name, group);

            var solids = new List<Solid>();
            foreach (var element in collector)
            {
                var elSolids = GetElementSolids(element);
                if (elSolids.Count > 0)
                    solids.AddRange(elSolids);
            }

            THREEGeometry geometry = ConvertSolidsToThreeGeometry(solids);
            geometries[geometry.uuid] = geometry;

            THREEMesh mesh = THREEMesh.Create(
                "MultiLevelElementsMesh", Guid.NewGuid().ToString(), geometry.uuid, THREEMaterial.DefaultMaterialUuid, ElementType.Element
            );
            AddObject(mesh.name, mesh);

            group.children.Add(mesh);

            return group;
        }

        private List<Solid> GetElementSolids(Element element)
        {
            GeometryElement geo = element.get_Geometry(options);
            var solids = new List<Solid>();
            foreach (GeometryObject obj in geo)
            {
                var solid = obj as Solid;
                if (solid == null || solid.Volume < 0) continue;
                solids.Add(solid);
            }

            if (solids.Count == 0)
            {
                solids = GetElementSymbolSolids(element);
            }

            return solids;
        }

        private List<Solid> GetElementSymbolSolids(Element element)
        {
            GeometryElement geo = element.get_Geometry(options);
            var solids = new List<Solid>();
            foreach (GeometryObject g in geo)
            {
                var gi = g as GeometryInstance;
                if (gi == null) continue;
                var symbolGeo = gi.SymbolGeometry;
                var giTransformed = symbolGeo.GetTransformed(gi.Transform);

                foreach (var subg in giTransformed)
                {
                    var solid = subg as Solid;
                    if (solid == null || solid.Volume < 0) continue;
                    solids.Add(solid);
                }
            }

            return solids;
        }

        public List<Solid> GetFamilyInstanceOriginalSolids(FamilyInstance element)
        {

            GeometryElement geo = element.Symbol.get_Geometry(options);


            //GeometryElement geo = element.GetOriginalGeometry(options);
            //if (element.Mirrored)
            //{
              //  var p = Plane.CreateByNormalAndOrigin(element.FacingOrientation, new XYZ(0, 0, 0));
                //geo = geo.GetTransformed(Transform.CreateReflection(p));
            //}

            
            var solids = new List<Solid>();

            foreach (GeometryObject obj in geo)
            {
                var solid = obj as Solid;
                if (solid == null || solid.Volume < 0) continue;
                solids.Add(solid);
            }
            return solids;
        }

        private THREEGeometry ConvertSolidsToThreeGeometry(
            List<Solid> solids, List<int> materialIndexes = null, double? lod = null, XYZ origin = null)
        {
            bool many = false;
            if (solids.Count > 1 && materialIndexes != null)
            {
                many = true;
            }

            var faceIndices = new List<double>();
            var faceVertices = new List<double>();
            var faceNormals = new List<double>();
            var triangleIndices = new int[3];
            var triangleCorners = new XYZ[3];

            var groups = new List<THREEGeometryGroup>();

            int totalCount = 0;

            // todo rename to geo
            string solidUuid = Guid.NewGuid().ToString();

            for (int si = 0; si < solids.Count; si++)
            {
                //faceIndices.Clear();
                //faceVertices.Clear();
                //faceNormals.Clear();

                Solid solid = solids[si];

                int groupCount = 0;
                foreach (Face face in solid.Faces)
                {

                    Mesh mesh;
                    if (lod != null)
                    {
                        mesh = face.Triangulate((double)lod);
                    } else
                    {
                        mesh = face.Triangulate();
                    }

                    // Handle: AttributeError: 'NoneType' object has no attribute 'NumTriangles'
                    if (mesh == null) continue;


                    int nTriangles = mesh.NumTriangles;
                    var vertices = mesh.Vertices;
                    int nVertices = vertices.Count;

                    var vertexCoordsMm = new List<double>(3 * nVertices);

                    foreach (XYZ v in vertices)
                    {
                        // # Move pt to new origin
                        XYZ p = v;
                        if (origin != null)
                        {
                            p = v - origin;
                        }

                        vertexCoordsMm.Add(Utils.FootToMeter(p.X));
                        vertexCoordsMm.Add(Utils.FootToMeter(p.Y));
                        vertexCoordsMm.Add(Utils.FootToMeter(p.Z));
                    }

                    for (int i = 0; i < nTriangles; i++)
                    {


                        MeshTriangle triangle = mesh.get_Triangle(i);

                        for (int j = 0; j < 3; j++)
                        {
                            int k = (int)triangle.get_Index(j);
                            triangleIndices[j] = k;
                            triangleCorners[j] = vertices[k];
                        }

                        // Calculate constant triangle facet normal
                        XYZ v = triangleCorners[1] - triangleCorners[0];
                        XYZ w = triangleCorners[2] - triangleCorners[0];

                        XYZ triangleNormal = v.CrossProduct(w).Normalize();
                        groupCount += 3;

                        for (int j = 0; j < 3; j++)
                        {
                            int nFaceVertices = faceVertices.Count;

                            faceIndices.Add(nFaceVertices / 3);

                            int i3 = triangleIndices[j] * 3;

                            //Rotate the X, Y and Z directions
                            //since the Z direction points upward
                            //in Revit as opposed to sideways or
                            //outwards or forwards in WebGL.

                            faceVertices.Add(vertexCoordsMm[i3 + 1]);
                            faceVertices.Add(vertexCoordsMm[i3 + 2]);
                            faceVertices.Add(vertexCoordsMm[i3]);

                            UV uv = ProjectPoint(face, triangleCorners[j]);

                            XYZ normal = face.ComputeNormal(uv);
                            faceNormals.Add(normal.Y);
                            faceNormals.Add(normal.Z);
                            faceNormals.Add(normal.X);
                        }
                    }
                }

                if (many) {
                    groups.Add(new THREEGeometryGroup { start = totalCount, materialIndex = materialIndexes[si], count = groupCount });
                    totalCount += groupCount;
                }
            }

            var geometry = new THREEGeometry
            {
                uuid = solidUuid,
                data = new Dictionary<string, object>()
                {
                    {
                        "attributes", new Dictionary<string, THREEGeometryVectorArray>()
                        {
                            { "position", new THREEGeometryVectorArray { array = faceVertices } },
                            { "normal", new THREEGeometryVectorArray { array = faceNormals } }
                        }
                    }
                }
            };

            if(many)
            {
                geometry.data["groups"] = groups;
            }

            return geometry;
        }

        private UV ProjectPoint(Face face, XYZ point)
        {
            IntersectionResult projectResult = face.Project(point);

            if (projectResult != null)
            {
                return projectResult.UVPoint;
            }

            // Because Project() fails on ConicalFace:

            var minDist = Double.PositiveInfinity;
            Curve closestEdgeCrv = null;
            foreach (EdgeArray edgeLoop in face.EdgeLoops)
            {
                foreach (Edge edge in edgeLoop)
                {
                    Curve crv = edge.AsCurve();
                    double dist = crv.Distance(point);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestEdgeCrv = crv;
                    }
                }
            }

            XYZ edgeProjectionPoint = closestEdgeCrv.Project(point).XYZPoint;
            IntersectionResult result = face.Project(edgeProjectionPoint);

            // I do not know why
            if (result == null) return new UV(0, 0);

            return result.UVPoint;
        }

        public void SaveToFile(string filePath = @"C:\Users\U_M12EE\Desktop\model.json")
        {
            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new StringEnumConverter());
                serializer.Serialize(file, Root);
            }
        }
    }

    public class BWElement
    {
        public string id { get; set; }
        public string rvtId { get; set; }
        public string officeId { get; set; }
        public string type { get; set; }
        public List<string> children { get; set; } = new List<string>();
        public List<string> geometry { get; set; } = new List<string>();
        public double[] matrix { get; set; }
    }
}
