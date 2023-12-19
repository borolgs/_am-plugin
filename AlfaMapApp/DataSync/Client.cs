using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AlfaMap.Common;

namespace AlfaMap.DataSync {
    public class Client {
        private readonly HttpClient httpClient;
        private readonly string urlBase;

        public Client(HttpClient httpClient, string baseUrl) {
            this.urlBase = baseUrl;
            this.httpClient = httpClient;
        }

        public async Task<Result<List<Node>, DataSyncException>> FindNodesByPlaceId(int placeId) {
            try {
                HttpResponseMessage response = await httpClient.GetAsync($"{urlBase}/nodes?placeId={placeId}&limit=-1");
                
                if (response.StatusCode != HttpStatusCode.OK) {
                    return ErrorResponse<List<Node>>($"Can't get nodes. Status {response.StatusCode}");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var building = JsonConvert.DeserializeObject<ListResponse<Node>>(responseBody);
                return OkResponse(building.results);

            } catch (Exception e) {
                return ErrorResponse<List<Node>>(e);
            }
        }

        public async Task<Result<Place, DataSyncException>> FindPlaceById(int placeId) {
            try {
                HttpResponseMessage response = await httpClient.GetAsync($"{urlBase}/places/{placeId}");

                if (response.StatusCode != HttpStatusCode.OK) {
                    if (response.StatusCode == HttpStatusCode.NotFound) {
                        return NotFoundResponse<Place>($"Place {placeId} Not Found");
                    }
                    return ErrorResponse<Place>($"Can't get place. Status {response.StatusCode}");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var place = JsonConvert.DeserializeObject<Place>(responseBody);
                return OkResponse(place);

            } catch (Exception e) {
                return ErrorResponse<Place>(e);
            }
        }

        public async Task<Result<Building, DataSyncException>> FindBuildingById(int buildingId) {
            try {
                HttpResponseMessage response = await httpClient.GetAsync($"{urlBase}/buildings/{buildingId}");

                if (response.StatusCode != HttpStatusCode.OK) {
                    if (response.StatusCode == HttpStatusCode.NotFound) {
                        return NotFoundResponse<Building>($"Building {buildingId} Not Found");
                    }
                    return ErrorResponse<Building>($"Can't get building. Status {response.StatusCode}");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var building = JsonConvert.DeserializeObject<Building>(responseBody);
                return OkResponse(building);

            } catch (Exception e) {
                return ErrorResponse<Building>(e);
            }
        }

        public async Task<Result<Building[], DataSyncException>> FindBuildingsByPlaceId(int placeId) {
            try {
                HttpResponseMessage response = await httpClient.GetAsync($"{urlBase}/buildings?placeId={placeId}");

                if (response.StatusCode != HttpStatusCode.OK) {
                    return ErrorResponse<Building[]>($"Can't get building. Status {response.StatusCode}");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var buildings = JsonConvert.DeserializeObject<ArrayResponse<Building>>(responseBody);
                return OkResponse(buildings.results);

            } catch (Exception e) {
                return ErrorResponse<Building[]>(e);
            }
        }

        public async Task<Result<Building, DataSyncException>> CreateBuilding(BuildingCreate data) {
            try {
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync($"{urlBase}/buildings", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.Created) {
                    return ErrorResponse<Building>($"Can't create building. Status {response.StatusCode}");
                }

                Building created = JsonConvert.DeserializeObject<Building>(responseBody);
                return OkResponse(created);

            } catch (Exception e) {
                return ErrorResponse<Building>(e);
            }
        }

        public async Task<Result<Building, DataSyncException>> UpdateBuilding(int buildingId, BuildingUpdate data) {
            try {
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json-patch+json");
                HttpResponseMessage response = await httpClient.PostAsync($"{urlBase}/buildings/{buildingId}", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK) {
                    if (response.StatusCode == HttpStatusCode.NotFound) {
                        return ErrorResponse<Building>($"Building {buildingId} Not Found");
                    }
                    return ErrorResponse<Building>($"Can't udpate building {buildingId}");
                }

                Building updated = JsonConvert.DeserializeObject<Building>(responseBody);

                return OkResponse(updated);
            } catch (Exception e) {
                return ErrorResponse<Building>(e);
            }
        }

        public async Task<Result<BuildingModel, DataSyncException>> FindBuildingModelById(int modelId) {
            try {
                HttpResponseMessage response = await httpClient.GetAsync($"{urlBase}/models/{modelId}");

                if (response.StatusCode != HttpStatusCode.OK) {
                    if (response.StatusCode == HttpStatusCode.NotFound) {
                        return NotFoundResponse<BuildingModel>($"Model {modelId} Not Found");
                    }
                    return ErrorResponse<BuildingModel>($"Can't get model. Status {response.StatusCode}");
                }

                string responseBody = await response.Content.ReadAsStringAsync();
                var model = JsonConvert.DeserializeObject<BuildingModel>(responseBody);
                return OkResponse(model);

            } catch (Exception e) {
                return ErrorResponse<BuildingModel>(e);
            }
        }

        public async Task<Result<BuildingModel, DataSyncException>> CreateBuildingModel(BuildingModelCreate data) {
            try {
                string dataStr = JsonConvert.SerializeObject(data);
                var content = new StringContent(dataStr, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync($"{urlBase}/models", content);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.Created) {
                    return ErrorResponse<BuildingModel>($"Can't create model. Status {response.StatusCode}. Message: {responseBody}");
                }

                BuildingModel created = JsonConvert.DeserializeObject<BuildingModel>(responseBody);
                return OkResponse(created);

            } catch (Exception e) {
                return ErrorResponse<BuildingModel>(e);
            }
        }

        public async Task<Result<BuildingModel, DataSyncException>> UpdateBuildingModel(int modelId, BuildingModelUpdate data) {
            try {
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{urlBase}/models/{modelId}") {
                    Content = content
                };
                HttpResponseMessage response = await httpClient.SendAsync(request);
                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.StatusCode != HttpStatusCode.OK) {
                    return ErrorResponse<BuildingModel>($"Can't update model. Status {response.StatusCode}. Message: {responseBody}");
                }

                BuildingModel updated = JsonConvert.DeserializeObject<BuildingModel>(responseBody);
                return OkResponse(updated);

            } catch (Exception e) {
                return ErrorResponse<BuildingModel>(e);
            }
        }

        private Result<T, DataSyncException> OkResponse<T>(T data) {
            return new OkResult<T, DataSyncException>(data);
        }

        private Result<T, DataSyncException> NotFoundResponse<T>(string message) {
            return new ErrResult<T, DataSyncException>(new DataSyncException(message, DataSyncExceptionCode.NotFound));
        }

        private Result<T, DataSyncException> ErrorResponse<T>(string message) {
            return new ErrResult<T, DataSyncException>(new DataSyncException(message));
        }

        private Result<T, DataSyncException> ErrorResponse<T>(Exception inner) {
            return new ErrResult<T, DataSyncException>(new DataSyncException(inner));
        }

        private Result<T, DataSyncException> ErrorResponse<T>(string message, Exception inner, DataSyncExceptionCode code = DataSyncExceptionCode.Unexpected) {
            return new ErrResult<T, DataSyncException>(new DataSyncException(message, inner, code));
        }
    }

    class ArrayResponse<T> {
        public T[] results { get; set; }
    }

    class ListResponse<T> {
        public List<T> results { get; set; }
    }
}
