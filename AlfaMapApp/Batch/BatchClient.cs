#region Namespaces
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
//using Simple.OData.Client;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
#endregion

namespace AlfaMap.Batch
{

    public sealed partial class BatchClient
    {
        private readonly string uri = "http://dahapp.moscow.alfaintra.net:8000/sap/opu/odata/sap/ZAM_SRV";
        private string token;
        private readonly HttpClient httpClient = new HttpClient();
        //private ODataClientSettings odataSettings;
        //private readonly ODataClient odataClient;

        public BatchClient()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "Y293b3JraW5nOkNvd29yayEyIzQ=");
            //odataSettings = new ODataClientSettings(httpClient);
            //odataSettings.BaseUri = uri;
            //odataSettings.AfterResponse = resMessage => {};
            //odataSettings.BeforeRequest = async reqMessage => {
            //    if (reqMessage.Method != HttpMethod.Get) {
            //        await GetToken();
            //        if (string.IsNullOrEmpty(token)) {
            //            return;
            //        }
            //        reqMessage.Headers.Add("x-csrf-token", token);
            //    }
            //};

            //odataClient = new ODataClient(odataSettings);
        }

        private async Task GetToken() {
            try {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                request.Headers.Add("X-CSRF-Token", "fetch");
                HttpResponseMessage response = await httpClient.SendAsync(request);

                if (response.StatusCode != HttpStatusCode.OK) {
                    Console.WriteLine($"Can't get CSRF token. Status {response.StatusCode}");
                }

                var hasToken = response.Headers.Contains("X-CSRF-Token");
                if (!hasToken) {
                    Console.WriteLine("Can't get CSRF token. No X-CSRF-Token header");
                    return;
                }

                token = response.Headers.GetValues("X-CSRF-Token").FirstOrDefault();

            } catch (Exception e) {
                Console.WriteLine($"Can't get CSRF token. {e.Message}");
            }
        }

        //public async Task<PlanDTO> UploadPlan(int droId, PlanDTO plan)
        //{
        //    try
        //    {
        //        var updated = await odataClient.For<PlanDTO>("PlanSet").Filter(p => p.NodeId == droId).Set(plan).UpdateEntryAsync();
        //        return updated;
        //    }
        //    catch (Exception e)
        //    {
        //        PlanDTO newPlan = await odataClient.For<PlanDTO>("PlanSet")
        //           .Set(plan)
        //           .InsertEntryAsync();

        //        return newPlan;
        //    }
        //}

        public async Task<IEnumerable<CoworkingRoom>> FindCoworkingRooms() {
            try {
                await GetToken();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{uri}/RoomSet");
                request.Headers.Add("X-CSRF-Token", "fetch");
                //request.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await httpClient.SendAsync(request);
                if (response.StatusCode != HttpStatusCode.OK) {
                    Console.WriteLine($"Request error. Status {response.StatusCode}");
                    return new List<CoworkingRoom>();
                }
                string responseBody = await response.Content.ReadAsStringAsync();
                var rooms = JsonConvert.DeserializeObject<OdataResponse<OdataResults<CoworkingRoom>>>(responseBody);
                return rooms.d.results;
            } catch (Exception e) {
                Console.WriteLine($"Find Coworking Room Error: {e.Message}");
                return new List<CoworkingRoom>();
            }
        }
    }

    public class PlanDTO {
        public int NodeId { get; set; }
        public string Svg { get; set; }
    }

    public class CoworkingRoom {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }

    public class OdataResponse<T> {
        public T d { get; set; }
    }

    public class OdataResults<T> {
        public List<T> results { get; set; }
    }
}
