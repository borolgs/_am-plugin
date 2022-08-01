using AlfaMap.DataSync;
using AlfaMap.State;
using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace AlfaMap.DataSync {
    public class SyncResults {
        public Dictionary<string, SyncNodeInfo> Elements = new Dictionary<string, SyncNodeInfo>();
        public Dictionary<int, SyncNodeInfo> Nodes = new Dictionary<int, SyncNodeInfo>();

        public void Add(ModelNode modelNode) {
            Elements[modelNode.Uuid] = new SyncNodeInfo(modelNode);
        }

        public void Add(Node node, UpdateNodeStatus status) {
            var info = new SyncNodeInfo(node);
            info.AddResult(SyncNodeResult.Create(status));
            Nodes[node.id] = info;
        }

        public void Update(ModelNode modelNode, UpdateNodeStatus status, string message = null) {
            Elements[modelNode.Uuid].AddResult(SyncNodeResult.Create(status, message));
        }
    }

    public class SyncNodeInfo {
        public int? NodeId { get; private set; } = null;
        public ElementId ElementId { get; private set; } = null;
        public string Name { get; private set; } = null;
        public List<SyncNodeResult> SyncResults { get; private set; } = new List<SyncNodeResult>();

        public bool Success { get; private set; } = false;
        public string Message => string.Join("\n", SyncResults.Select(r => r.Message));

        public SyncNodeInfo(ModelNode modelNode) {
            NodeId = modelNode.NodeId;
            ElementId = modelNode.Element.Id;
            Name = modelNode.NodePartialName;
        }
        public SyncNodeInfo(Node node) {
            NodeId = node.id;
            Name = node.name;
        }

        public void AddResult(SyncNodeResult result) {
            SyncResults.Add(result);
            if (result.Status == UpdateNodeStatus.Updated) {
                Success = true;
            }
        }
    }

    public enum UpdateNodeStatus {
        NoElement,
        NoName,
        NoNodeId,
        NoOfficeId,
        Deleted,
        InvalidNameOrDeleted,
        InvalidOffice,
        InvalidNodeIdOrDeleted,
        Replaced,
        Changed,
        Updated
    }

    public class SyncNodeResult {
        public UpdateNodeStatus Status { get; set; }
        public string Message { get; set; }

        public static SyncNodeResult Create(UpdateNodeStatus status, string message = null) {
            string createMessage() {
                switch (status) {
                    case UpdateNodeStatus.NoElement:
                        return "нет элемента в модели";
                    case UpdateNodeStatus.NoName:
                        return "нет такого имени";
                    case UpdateNodeStatus.NoNodeId:
                        return "нет такого Id";
                    case UpdateNodeStatus.NoOfficeId:
                        return "нет такого OfficeId";
                    case UpdateNodeStatus.Deleted:
                        return "узел был удален";
                    case UpdateNodeStatus.InvalidNameOrDeleted:
                        return "неправильно имя или узел был удален";
                    case UpdateNodeStatus.InvalidOffice:
                        return "неправильный OfficeId";
                    case UpdateNodeStatus.Replaced:
                        return "Выбран другой узел по новому имени";
                    case UpdateNodeStatus.Changed:
                        return "Данные изменены";
                    case UpdateNodeStatus.Updated:
                        return "Обновлен успешно";
                    default:
                        break;
                }
                return "";
            }
            string msg = message ?? createMessage();

            return new SyncNodeResult {
                Message = msg,
                Status = status
            };
        }
    }
}
