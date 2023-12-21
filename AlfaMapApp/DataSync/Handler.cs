using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlfaMap.Revit;
using AlfaMap.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using AlfaMap.State;
using AlfaMap.Shared;
using AlfaMap.Converter;
using AlfaMap.Converter2d;
using Newtonsoft.Json.Serialization;

namespace AlfaMap.DataSync {
    public class Handler {
        public BuildingTree Tree;
        private Document doc;

        private int buildingId;
        private int placeId;

        private Building building;
        public Building Building => building;
        private List<Node> nodes;

        public SyncResults SyncResults = null;

        private readonly Client client;

        public Handler(Client client) {
            this.client = client;
        }

        public bool PrepareDocument(Document doc) {
            bool success = ParamChecker.CheckAndCreateParams(doc, Parameters.GetAll());
            return success;
        }

        public void InitFromDocument(Document doc) {
            this.doc = doc;

            bool hasParams = ParamChecker.CheckParams(doc, Parameters.GetAll());
            if (!hasParams) {
                throw new DataSyncException("No Parameters", DataSyncExceptionCode.App);
            }

            Parameter buildingIdPar = doc.ProjectInformation.get_Parameter(Parameters.BuildingId.Guid);
            Parameter placeIdPar = doc.ProjectInformation.get_Parameter(Parameters.PlaceId.Guid);
            buildingId = buildingIdPar?.AsInteger() ?? -1;
            placeId = placeIdPar?.AsInteger() ?? -1;

            bool invalidBuildingPar = buildingIdPar != null && buildingIdPar.StorageType == StorageType.String;
            if (invalidBuildingPar) {
                placeId = int.TryParse(buildingIdPar.AsString(), out placeId) ? placeId : -1;
            }
            bool invalidPlacePar = placeIdPar != null && placeIdPar.StorageType == StorageType.String;
            if (invalidPlacePar) {
                buildingId = int.TryParse(buildingIdPar.AsString(), out buildingId) ? buildingId : -1;
            }

            Tree = new BuildingTree(doc);
        }

        public async Task<Result<bool, DataSyncException>> InitBuilding() {
            var building = await FindOrCreateBuilding();
            if (building.Err) {
                return new ErrResult<bool, DataSyncException>(building.Error);
            }

            this.building = building.Value;

            if (buildingId != this.building.id) {
                buildingId = this.building.id;
            }

            return new OkResult<bool, DataSyncException>(true);

        }

        public async Task<Result<bool, DataSyncException>> LoadData() {
            var buildingReady = await InitBuilding();
            if (buildingReady.Err) {
                return buildingReady;
            }

            var nodes = await FindNodes();
            if (nodes.Err) {
                return new ErrResult<bool, DataSyncException>(nodes.Error);
            }

            this.nodes = nodes.Value;
            return new OkResult<bool, DataSyncException>(true);
        }

        private async Task<Result<Building, DataSyncException>> FindOrCreateBuilding() {
            ErrResult<Building, DataSyncException> err(DataSyncException e) => new ErrResult<Building, DataSyncException>(e);
            ErrResult<Building, DataSyncException> appErr(string message) => new ErrResult<Building, DataSyncException>(new DataSyncException(message));
            OkResult<Building, DataSyncException> ok(Building b) => new OkResult<Building, DataSyncException>(b);

            if (buildingId < 1) {
                if (placeId < 1) {
                    return appErr("No placeId or buildingId");
                }
                Console.WriteLine($"Try find buildings with placeId {placeId}");
                var otherExistingBuildings = await client.FindBuildingsByPlaceId(placeId);
                if (otherExistingBuildings.Err) {
                    return err(otherExistingBuildings.Error);
                }
                if (otherExistingBuildings.Value.Length > 0) {
                    Building building = otherExistingBuildings.Value[0];
                    Console.WriteLine($"Building found {building.id}");
                    return ok(building);
                }
            }

            // TODO: fix. Если нет здания с таким place, а buildingId=0, то будет лишний GET /building/0
            var existingBuilding = await client.FindBuildingById(buildingId);
            if (existingBuilding.Ok) {
                Building building = existingBuilding.Value;
                Console.WriteLine($"Building found {building.id}");
                return ok(building);
            }

            if (existingBuilding.Error.Code == DataSyncExceptionCode.NotFound) {
                if (placeId < 1) {
                    return appErr("placeId parameter is empty");
                }
                Console.WriteLine($"Try find buildings with placeId {placeId}");
                var otherExistingBuildings = await client.FindBuildingsByPlaceId(placeId);
                if (otherExistingBuildings.Err) {
                    return err(otherExistingBuildings.Error);
                }
                if (otherExistingBuildings.Value.Length > 0) {

                    Building building = otherExistingBuildings.Value[0];
                    Console.WriteLine($"Building found {building.id}");
                    return ok(building);
                }

                var newBuilding = await client.CreateBuilding(new BuildingCreate { name = doc.Title, placeId = placeId });
                Console.WriteLine($"Building created placeId={placeId}");
                return newBuilding;

            }
            return err(existingBuilding.Error);
        }

        private async Task<Result<List<Node>, DataSyncException>> FindNodes() {
            var nodes = await client.FindNodesByPlaceId(placeId);
            return nodes;
        }

        public void UpdateDocumentBuildingData() {
            if (building == null && nodes == null) {
                throw new DataSyncException("Can't update document");
            }
            doc.ProjectInformation.get_Parameter(Parameters.BuildingId.Guid)?.Set(building.id);
            doc.ProjectInformation.get_Parameter(Parameters.PlaceId.Guid)?.Set(building.placeId);
            doc.ProjectInformation.get_Parameter(Parameters.BuildingData.Guid)?.Set(JsonConvert.SerializeObject(building));
        }

        public void UpdateDocumentElements() {
            if (building == null && nodes == null) {
                throw new DataSyncException("Can't update document");
            }

            MapNodesToElements(nodes);
        }

        private void MapNodesToElements(List<Node> nodes) {
            void setParamValues(Node node, ModelNode modelNode) {
                modelNode.Element.get_Parameter(Parameters.NodeId.Guid)?.Set(node.id);
                modelNode.Element.get_Parameter(Parameters.OfficeId.Guid)?.Set(node.officeId);
                modelNode.Element.get_Parameter(Parameters.NodeData.Guid)?.Set(JsonConvert.SerializeObject(node));
            }
            void resetParamValues(ModelNode modelNode) {
                modelNode.Element.get_Parameter(Parameters.NodeId.Guid)?.Set(0);
                modelNode.Element.get_Parameter(Parameters.OfficeId.Guid)?.Set(0);
                modelNode.Element.get_Parameter(Parameters.NodeData.Guid)?.Set("");
            }

            var officeNodePrefixes = new Dictionary<int, string>();
            int officeCount = building.place.offices.Length;
            foreach (Office office in building.place.offices) {
                if (office.node == null) {
                    continue;
                }
                officeNodePrefixes[office.id] = office.node.childrenPrefix;
            }
            string officeIdToNodeName(int officeId) {
                return officeNodePrefixes.ContainsKey(officeId) ? officeNodePrefixes[officeId].ToLower() : null;
            }

            if (officeCount < 1) {
                // TODO: throw error
            }

            var syncResults = new SyncResults();
            
            var nodesMap = new Dictionary<int, Node>();
            var syncedNodes = new HashSet<int>();
            var fullNameToNode = new Dictionary<string, Node>();
            var partNameToNode = new Dictionary<string, Node>();
            bool canUsePartName = true;

            foreach (Node node in nodes) {
                bool isNotRoomOrWorkplace = !new[] { 4, 5 }.Contains(node.type.id);
                if (isNotRoomOrWorkplace) {
                    continue;
                }
                nodesMap[node.id] = node;

                string name = node.name.ToLower();

                if (fullNameToNode.ContainsKey(node.name)) {
                    Console.WriteLine($"Node #{node.id}({node.name}): duplicated node name");
                }
                fullNameToNode[name] = node;

                // Remove prefix
                // TODO: add node field "partName": 200-1, 432 
                string partName = null;
                if (canUsePartName) {
                    string officePrefix = officeIdToNodeName(node.officeId);
                    if (!string.IsNullOrEmpty(officePrefix)) {
                        partName = name.Replace(officePrefix + "-", "");
                    }

                    if (partName != null && partName != name) {
                        if (partNameToNode.ContainsKey(partName) && node.type.id != 6) {
                            canUsePartName = false;
                        } else {
                            partNameToNode[partName] = node;
                        }
                    } else {
                        Console.WriteLine($"Node #{node.id}({node.name}): Can't remove prefix");
                    }
                }
            }

            foreach (ModelNode modelNode in Tree.Nodes.Values) {
                if (modelNode.Id.IntegerValue == 1349238) {
                    Console.WriteLine("!");
                }
                if (modelNode.Type == ModelNodeType.Level || modelNode.Type == ModelNodeType.Building) {
                    continue;
                }
                syncResults.Add(modelNode);

                if (string.IsNullOrEmpty(modelNode.NodePartialName)) {
                    syncResults.Update(modelNode, UpdateNodeStatus.NoName);
                    continue;
                }

                Node node = null;
                if (modelNode.NodeId > 0) {
                    int nodeId = modelNode.NodeId.GetValueOrDefault();
                    if(nodeId < 1) {
                        syncResults.Update(modelNode, UpdateNodeStatus.NoNodeId);
                    }
                    if (nodesMap.ContainsKey(nodeId)) {
                        node = nodesMap[nodeId];
                    } else {
                        syncResults.Update(modelNode, UpdateNodeStatus.InvalidNodeIdOrDeleted, $"узла с таким Id {nodeId} не существует");
                        resetParamValues(modelNode);
                    }
                }

                if (canUsePartName) {
                    string partName = modelNode.NodePartialName.ToLower();
                    if (!partNameToNode.ContainsKey(partName)) {
                        syncResults.Update(modelNode, UpdateNodeStatus.InvalidNameOrDeleted);
                        resetParamValues(modelNode);
                        continue;
                    }

                    Node nodeByPartName = partNameToNode[partName];

                    // TODO: show diff
                    if (node != null && nodeByPartName.id != node.id) {
                        syncResults.Update(modelNode, UpdateNodeStatus.Replaced);
                    }

                    setParamValues(nodeByPartName, modelNode);
                    syncResults.Update(modelNode, UpdateNodeStatus.Updated, $"{nodeByPartName.name}");
                    syncedNodes.Add(nodeByPartName.id);
                    continue;
                }

                if (modelNode.OfficeId.GetValueOrDefault() < 1) {
                    syncResults.Update(modelNode, UpdateNodeStatus.NoOfficeId);
                    resetParamValues(modelNode);
                    continue;
                }

                string officePrefix = officeIdToNodeName(modelNode.OfficeId ?? 0);
                if (string.IsNullOrEmpty(officePrefix)) {
                    syncResults.Update(modelNode, UpdateNodeStatus.InvalidOffice, $"Неправильный officeId: {modelNode.OfficeId}");
                    resetParamValues(modelNode);
                    continue;
                }

                string fullName = officePrefix + '-' + modelNode.NodePartialName.ToLower();
                if (!fullNameToNode.ContainsKey(fullName)) {
                    syncResults.Update(modelNode, UpdateNodeStatus.InvalidNameOrDeleted, $"неправильно имя '{fullName}' или узел был удален");
                    resetParamValues(modelNode);
                    continue;
                }

                Node nodeByFullName = fullNameToNode[fullName];

                // TODO: show diff
                if (node != null && nodeByFullName.id != node.id) {
                    syncResults.Update(modelNode, UpdateNodeStatus.Replaced);
                }

                syncResults.Update(modelNode, UpdateNodeStatus.Updated, $"{nodeByFullName.name}");
                setParamValues(nodeByFullName, modelNode);
                syncedNodes.Add(nodeByFullName.id);
            }

            foreach (Node node in nodesMap.Keys
                .Except(syncedNodes)
                .Where(nodesMap.ContainsKey)
                .Select(id => nodesMap[id])
                .Where(node => node.type.id != 6)) {

                syncResults.Add(node, UpdateNodeStatus.NoElement);
            }                

            SyncResults = syncResults;
            
        }

        public async Task<Result<BuildingModel, DataSyncException>> UploadData() {
            var buildingReady = await InitBuilding();
            if (buildingReady.Err) {
                return new ErrResult<BuildingModel, DataSyncException>(buildingReady.Error);
            }
            var model = await UpdateOrCreateBuildingModel();
            return model;
        }

        private async Task<Result<BuildingModel, DataSyncException>> UpdateOrCreateBuildingModel() {
            ErrResult<BuildingModel, DataSyncException> err(DataSyncException e) => new ErrResult<BuildingModel, DataSyncException>(e);
            ErrResult<BuildingModel, DataSyncException> appErr(string message) => new ErrResult<BuildingModel, DataSyncException>(new DataSyncException(message));
            OkResult<BuildingModel, DataSyncException> ok(BuildingModel m) => new OkResult<BuildingModel, DataSyncException>(m);

            if (building == null) {
                return appErr("No builidng");
            }
            if (Tree == null) {
                return appErr("No Tree");
            }

            var elements = CreateElements();
            if (elements.Err) {
                return err(elements.Error);
            }
            var geometry = await CreateGeometryData();
            if (geometry.Err) {
                return err(geometry.Error);
            }

            bool hasCurrentModel = building.currentModelId.HasValue && building.currentModelId.Value > 0;
            if (hasCurrentModel) {
                var currentModel = await client.FindBuildingModelById(building.currentModelId.Value);
                if (currentModel.Err) {
                    return err(currentModel.Error);
                }

                var updatedModel = await client.UpdateBuildingModel(
                    currentModel.Value.id,
                    new BuildingModelUpdate {
                        description = $"Test Update {DateTime.Now.ToLocalTime()}",
                        elements = elements.Value,
                        geometry = new BuildingModelGeometry {
                            d2 = geometry.Value.Item2,
                            d3 = geometry.Value.Item1,
                        }
                    }
                );

                return updatedModel;
            }

            var newModel = await client.CreateBuildingModel(
                new BuildingModelCreate {
                    buildingId = buildingId,
                    description = $"Test Create {DateTime.Now.ToLocalTime()}",
                    asCurrent = true,
                    elements = elements.Value,
                    geometry = new BuildingModelGeometry {
                        d2 = geometry.Value.Item2,
                        d3 = geometry.Value.Item1,
                    }
                }
            );
            return newModel;
        }

        private async Task<Result<(string,string), DataSyncException>> CreateGeometryData() {
            try {
                var converter = new THREEConverter(doc, Tree);
                converter.Convert();
                var data3dString = JsonConvert.SerializeObject(converter.Root, new StringEnumConverter());
                var encodedData3dString = await StringUtils.ConvertoToZipBase64(data3dString);

                //var converter2d = new BuildingTo2DConverter();
                //var data2d = converter2d.Convert(Tree.Root);
                //var jsonSettings = new JsonSerializerSettings { 
                //    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                //    Converters = new JsonConverter[] {
                //        new BBoxConverter(),
                //        new XYZConverter(),
                //        new BoundariesConverter(),
                //        new CurveConverter(),
                //    },
                //    NullValueHandling = NullValueHandling.Ignore,
                //};
                //var data2dString = JsonConvert.SerializeObject(data2d, jsonSettings);
                //var encodedData2dString = await StringUtils.ConvertoToZipBase64(data2dString);

                return new OkResult<(string, string), DataSyncException>((encodedData3dString, null));
            } catch (Exception e) {
                return new ErrResult<(string,string), DataSyncException>(new DataSyncException("Convert geometry error", e));
            }
        }

        private Result<List<ModelElement>, DataSyncException> CreateElements() {
            try {
                var elements = new ModelDataConverter(Tree).Convert();
                return new OkResult<List<ModelElement>, DataSyncException>(elements);
            } catch (Exception e) {
                return new ErrResult<List<ModelElement>, DataSyncException>(new DataSyncException("Convert elements error", e));
            }
        }

        public ModelNode GetNode(string modelUniqueId) {
            var modelNode = Tree.GetNode(modelUniqueId);
            return modelNode;
        }
    }
}
