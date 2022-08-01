#region Namespaces
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Simple.OData.Client;
using System.Net.Http.Headers;
using System.Threading.Tasks;
#endregion

namespace AlfaMap.Coworking
{

    public sealed partial class CoworkingClient
    {
        private readonly Uri uri = new Uri("!!!"); // TODO
        private readonly string planSetName = "Constants.CoworkingPlanSetName"; // TODO!!
        private string token;
        private readonly HttpClient httpClient = new HttpClient();
        private ODataClientSettings odataSettings;
        private readonly ODataClient odataClient;

        public CoworkingClient()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "Constants.CoworkingBasicAuth"); // TODO

            odataSettings = new ODataClientSettings(httpClient);
            odataSettings.BaseUri = uri;
            odataSettings.AfterResponse = resMessage =>
            {
                var isGet = resMessage.RequestMessage.Method == HttpMethod.Get;
                var hasToken = resMessage.Headers.Contains("X-CSRF-Token");
                if (isGet && hasToken)
                {
                    token = resMessage.Headers.GetValues("X-CSRF-Token").FirstOrDefault();
                }
            };
            odataSettings.BeforeRequest = reqMessage =>
            {
                if (reqMessage.Method != HttpMethod.Get)
                {
                    reqMessage.Headers.Add("x-csrf-token", token);
                }
                else
                {
                    reqMessage.Headers.Add("X-CSRF-Token", "fetch");
                }
            };

            odataClient = new ODataClient(odataSettings);
        }

        public async Task<PlanDTO> UploadPlan(int droId, PlanDTO plan)
        {
            try
            {
               var updated = await odataClient.For<PlanDTO>(planSetName).Filter(p => p.NodeId == droId).Set(plan).UpdateEntryAsync();
                return updated;
            }
            catch (Exception e)
            {
                PlanDTO newPlan = await odataClient.For<PlanDTO>(planSetName)
                   .Set(plan)
                   .InsertEntryAsync();

                return newPlan;
            }
        }

        //public async Task<>

    }

    public class PlanDTO
    {
        public int NodeId { get; set; }
        public string Svg { get; set; }
    }
}
