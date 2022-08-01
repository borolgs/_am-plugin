using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RevitWrapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlfaMap.Shared;
using AlfaMap.State;

namespace AlfaMap.DataSync
{
    public class ModelDataConverter
    {
        private BuildingTree tree;

        public List<ModelElement> elements = new List<ModelElement>();

        public ModelDataConverter( BuildingTree tree)
        {
            this.tree = tree;
        }

        public List<ModelElement> Convert()
        {
            // TODO : replace loop to tree.Nodes
            foreach (ModelNode levelNode in tree.Root.Children)
            {
                var level = new ModelElement {
                    id = levelNode.Uuid,
                    type = levelNode.Type.ToString(),
                    name = (levelNode.Element as Level).Name ?? "-",
                    parentId = levelNode.Parent.Uuid,
                    path = levelNode.GetParents().Select(n => n.Uuid).ToList(),
                    childrenIds = levelNode.Children.Select(n => n.Uuid).ToList(),
                    data = LevelData.Create(levelNode),
                };

                foreach (ModelNode roomNode in levelNode.Children)
                {
                    var room = new ModelElement
                    {
                        id = roomNode.Uuid,
                        type = roomNode.Type.ToString(),
                        name = (roomNode.Element as Room).Number ?? "-",
                        parentId = roomNode.Parent.Uuid,
                        levelId = levelNode.Uuid,
                        path = roomNode.GetParents().Select(n => n.Uuid).ToList(),
                        childrenIds = roomNode.Children.Select(n => n.Uuid).ToList(),
                        data = RoomData.Create(roomNode),
                        nodeId = roomNode.Element.get_Parameter(Parameters.NodeId.Guid)?.AsInteger(),
                        node = GetNodeData(roomNode)
                    };

                    foreach (ModelNode workplaceNode in roomNode.Children)
                    {
                        var workplace = new ModelElement
                        {
                            id = workplaceNode.Uuid,
                            type = workplaceNode.Type.ToString(),
                            name = workplaceNode.Element.get_Parameter(Parameters.WorkplaceNumber.Guid)?.AsString() ?? "-",
                            parentId = workplaceNode.Parent.Uuid,
                            levelId = levelNode.Uuid,
                            path = workplaceNode.GetParents().Select(n => n.Uuid).ToList(),
                            data = WorkplaceData.Create(workplaceNode),
                            nodeId = workplaceNode.Element.get_Parameter(Parameters.NodeId.Guid)?.AsInteger(),
                            node = GetNodeData(workplaceNode)
                        };

                        elements.Add(workplace);
                    }

                    elements.Add(room);
                }

                elements.Add(level);

            }

            return elements;
        }

        public Node GetNodeData(ModelNode modelNode) {
            string nodeData = modelNode.Element.get_Parameter(Parameters.NodeData.Guid).AsString();

            if (nodeData != null) {
                Node node = JsonConvert.DeserializeObject<Node>(nodeData);
                if (node != null) {
                    return node;
                }
            }

            return null;
        }

        public void SaveToFile(string filePath, string versionUuid, string modelUuid)
        {
            //using (streamwriter file = file.createtext(filepath)) {
            //    jsonserializer serializer = jsonserializer.create();

            //    var version = new dictionary<string, object>
            //    {
            //        {"id", versionuuid },
            //        {"building", tree.root.uuid },
            //        {"description", "test" },
            //        {"createdat", datetime.utcnow },
            //        {"elements", elements },
            //        {"models", new string[] { modeluuid } },
            //    };

            //    serializer.serialize(file, version);
            //}
        }

        public BuildingVersion CreateBuildingVersionData(string versionUuid, string modelUuid)
        {
            var version = new BuildingVersion
            {
                id = versionUuid,
                building = tree.Root.Uuid,
                description = "Test",
                createdAt = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds,
                elements = elements,
                models = new string[] { modelUuid }
            };
            return version;
        }
    }
    
    public class LevelData : ElementData {
        public double elevation { get; private set; }

        public static LevelData Create(ModelNode levelNode)
        {
            return new LevelData
            {
                elevation = Utils.FootToMeter((levelNode.Element as Level).Elevation)
            };
        }
    }

    public class RoomData : ElementData {
        public string department { get; set; }
        public string functionType { get; set; }
        public static RoomData Create(ModelNode roomNode)
        {
            return new RoomData
            {
                department = roomNode.Element.get_Parameter(BuiltInParameter.ROOM_NAME)?.AsString(),
                functionType = roomNode.Element.LookupParameter("AB_R_Type")?.AsString() // TODO
            };
        }
    }

    public class WorkplaceData : ElementData {

        public static WorkplaceData Create(ModelNode workplaceNode)
        {
            var data = new WorkplaceData();
            return data;
        }
    }

    
    public class BuildingVersion
    {
        public string id { get; set; }
        public string building { get; set; }
        public string description { get; set; } = null;
        public int createdAt { get; set; }
        public List<ModelElement> elements { get; set; }
        public string[] models { get; set; }
    }
}
