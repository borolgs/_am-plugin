using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using RevitWrapper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AlfaMap.Shared;
using Newtonsoft.Json;

namespace AlfaMap.State {
    public class BuildingTree {
        private readonly UIApplication uiapp;
        private readonly Document doc;

        public Dictionary<string, ModelNode> Nodes = new Dictionary<string, ModelNode>();
        public List<string> Offices = new List<string>();
        public Dictionary<ModelNodeType, List<ModelNode>> NodesByType = new Dictionary<ModelNodeType, List<ModelNode>>();

        public ModelNode Root;

        public BuildingTree(Document doc) {
            this.doc = doc;

            string buildingUuid = doc.ProjectInformation.get_Parameter(OldParams.BuildingId.Guid)?.AsString();
            Root = ModelNode.CreateRootNode(doc.ProjectInformation, buildingUuid);

            foreach (Level level in Collector.CollectLevels(doc)) {
                ModelNode levelNode = ModelNode.CreateNode(level, ModelNodeType.Level);

                Root.AddChild(levelNode);
                AddNode(levelNode);

                var rooms = Collector.CollectRooms(doc, level.Id); // TODO: orderby

                foreach (Room room in rooms) {
                    ModelNode roomNode = ModelNode.CreateNode(room, ModelNodeType.Room);

                    levelNode.AddChild(roomNode);
                    AddNode(roomNode);
                }
                var workplaces = Collector.CollectFurniture(doc, level.Id).OrderBy(w => {
                    string workplaceNumber = w.get_Parameter(OldParams.WorkplaceNumber.Guid)?.AsString() ?? "0";
                    string digitsOnly = new string(workplaceNumber.Where(c => char.IsDigit(c)).ToArray());
                    int number = int.TryParse(digitsOnly, out number) ? number : 0;
                    return number;
                });
                foreach (FamilyInstance workplace in workplaces) {
                    bool isWorkplace = workplace.Symbol.get_Parameter(OldParams.IsWorkplaces.Guid).AsInteger() > 0;
                    if (!isWorkplace) continue;

                    Phase phase = doc.GetElement(workplace.CreatedPhaseId) as Phase;
                    Room room = workplace.Room;
                    if (room == null) {
                        foreach (Room r in Collector.CollectRooms(doc)) {
                            var rmPt = (r.Location as LocationPoint).Point;
                            var pt = (workplace.Location as LocationPoint).Point;
                            if (r.IsPointInRoom(pt)) {
                                room = r;
                                break;
                            }
                        }
                    }

                    if (room == null) {
                        Debug.Print($"Workplace #{workplace.Id} without room!");
                        continue;
                    }

                    if (!Nodes.ContainsKey(room.UniqueId)) {
                        Debug.Print($"Workplace #{workplace.Id} room problem!");
                        continue;
                    }


                    ModelNode workplaceNode = ModelNode.CreateNode(workplace, ModelNodeType.Workplace);


                    ModelNode roomNode = Nodes[room.UniqueId];
                    roomNode.AddChild(workplaceNode);
                    AddNode(workplaceNode);
                }

                levelNode.Children = levelNode.Children.OrderBy(n => (n.Element as Room).Number).ToList();
            }
            Root.Children = Root.Children.OrderBy(n => (n.Element as Level).Elevation).ToList();
            Offices.Sort();

            SortLevels();
        }

        private void AddNode(ModelNode node) {
            Nodes[node.Uuid] = node;
            if (NodesByType.ContainsKey(node.Type)) {
                NodesByType[node.Type].Add(node);
            } else {
                NodesByType[node.Type] = new List<ModelNode> { node };
            }
        }

        private void SortLevels() {
            var sortedLevels = NodesByType[ModelNodeType.Level]
                .OrderBy(lvl => {
                    string nameWithoutDigits = Regex.Replace((lvl.Element as Level).Name, @"[\s\d-.]", string.Empty);
                    return nameWithoutDigits;
                })
                .ThenBy(lvl => (lvl.Element as Level).Elevation)
                .ToList();

            Root.Children = sortedLevels;
        }

        public bool HasOffices() {
            return Offices.Where(of => !string.IsNullOrEmpty(of)).Count() > 0;
        }

        public ModelNode GetNode(string uniqueId) {
            return Nodes.ContainsKey(uniqueId) ? Nodes[uniqueId] : null;
        }
    }

    public enum ModelNodeType {
        Building,
        Level,
        Room,
        Workplace,
    }

    public class ModelNode {
        public ElementId Id { get; private set; } = null;
        private int? officeId = null;
        public int? OfficeId {
            get {
                if (officeId.HasValue && officeId.Value > 0) {
                    return officeId;
                }
                ModelNode parent = this.Parent;
                while (parent != null) {
                    if (parent.officeId.HasValue && parent.officeId.Value > 0) {
                        return parent.officeId.Value;
                    }
                    parent = parent.Parent;
                }
                return null;
            }
            private set {
                officeId = value;
            }
        }
        public int? NodeId { get; private set; } = null;
        // TODO
        public DataSync.Node Node => GetNode();
        public string NodePartialName { get { return GetNodePartialName(); } }
        public Element Element { get; private set; }
        public string Uuid { get; private set; }
        public ModelNodeType Type { get; private set; }
        public ModelNode Parent { get; set; } = null;
        public List<string> Path { get; set; } = new List<string>();
        public List<ModelNode> Children { get; set; } = new List<ModelNode>();
        public List<ModelNode> Neighbours { get; set; } = new List<ModelNode>();

        private ModelNode(string uuid, ModelNodeType type) {
            Uuid = uuid;
            Type = type;
        }

        public static ModelNode CreateNode(Element element, ModelNodeType type) {
            var node = new ModelNode(element.UniqueId, type);
            node.Id = element.Id;
            node.Element = element;
            node.OfficeId = element.get_Parameter(Parameters.OfficeId.Guid)?.AsInteger();
            node.NodeId = element.get_Parameter(Parameters.NodeId.Guid)?.AsInteger();
            return node;
        }

        public static ModelNode CreateRootNode(ProjectInfo info, string uuid) {
            var node = new ModelNode(uuid, ModelNodeType.Building);
            node.Element = info;
            node.OfficeId = info.get_Parameter(Parameters.OfficeId.Guid)?.AsInteger();
            return node;
        }

        public void AddChild(ModelNode child) {
            Children.Add(child);
            child.Parent = this;
            child.Path.Add(Uuid);
        }

        public List<ModelNode> GetParents() {
            var parents = new Stack<ModelNode>();
            var accessor = Parent;
            while (accessor != null) {
                parents.Push(accessor);
                accessor = accessor.Parent;
            }

            return parents.ToList();
        }

        private string GetNodePartialName() {
            switch (Type) {
                case ModelNodeType.Building:
                    return null;
                case ModelNodeType.Level:
                    return null;
                case ModelNodeType.Room:
                    return $"{(Element as Room).Number}";
                case ModelNodeType.Workplace:
                   return $"{(Parent.Element as Room).Number}-{Element.get_Parameter(OldParams.WorkplaceNumber.Guid)?.AsString() ?? "?"}";
                default:
                    return null;
            }
        }

        private DataSync.Node GetNode() {
            string nodestr = Element.get_Parameter(Parameters.NodeData.Guid)?.AsString();
            if (string.IsNullOrEmpty(nodestr)) {
                return null;
            }
            try {
                var node = JsonConvert.DeserializeObject<DataSync.Node>(nodestr);
                return node;
            } catch (Exception) {
                return null;
            }
        }

    }
}
