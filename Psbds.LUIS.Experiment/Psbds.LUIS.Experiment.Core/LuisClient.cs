using Newtonsoft.Json;
using Psbds.LUIS.Experiment.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Psbds.LUIS.Experiment.Core
{
    public class LuisClient
    {
        private const string BASE_URL = "https://westus.api.cognitive.microsoft.com";
        private const string WEB_API_URL = "https://westus.api.cognitive.microsoft.com/luis/webapi/v2.0/apps";
        private const string API_URL = "https://westus.api.cognitive.microsoft.com/luis/api/v2.0/apps";

        private readonly string _applicationKey;

        public LuisClient(string applicationKey)
        {
            _applicationKey = applicationKey;
        }

        public async Task<String> CreateApplication(object body)
           => await SendPostRequest($"{API_URL}", body);

        public async Task<String> DeleteApplication(string applicationId)
         => await SendDeleteRequest($"{API_URL}/{applicationId}");

        public async Task<String> ExportVersion(string applicationId, string applicationVersion)
            => await SendGetRequest($"{API_URL}/{applicationId}/versions/{applicationVersion}/export");

        public async Task<String> ImportVersion(string applicationId, string version, object body)
            => await SendPostRequest($"{API_URL}/{applicationId}/versions/import?versionId={version}", body);

        public async Task<String> TrainVersion(string applicationId, string version)
            => await SendPostRequest($"{API_URL}/{applicationId}/versions/{version}/train", null);

        public async Task<String> GetVersionTrainingStatus(string applicationId, string version)
            => await SendGetRequest($"{API_URL}/{applicationId}/versions/{version}/train");

        public async Task<String> RunDataSetTest(string applicationId, string version, string dataSetId)
            => await SendGetRequest($"{WEB_API_URL}/{applicationId}/versions/{version}/testdatasets/{dataSetId}/run");

        public async Task<String> CreateTestDataSet(string applicationId, string dataSetName, object body)
            => await SendPostRequest($"{WEB_API_URL}/{applicationId}/testdatasets?dataSetName={dataSetName}", body);



        private async Task<String> SendGetRequest(string path)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this._applicationKey);
                httpClient.BaseAddress = new Uri(BASE_URL);
                var result = await httpClient.GetAsync(path);
                var content = await result.Content.ReadAsStringAsync();
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception(content);
                }
                return content;
            }
        }

        private async Task<String> SendDeleteRequest(string path)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this._applicationKey);
                httpClient.BaseAddress = new Uri(BASE_URL);
                var result = await httpClient.DeleteAsync(path);
                var content = await result.Content.ReadAsStringAsync();
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception(content);
                }
                return content;
            }
        }

        private async Task<String> SendPostRequest(string path, object body)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.BaseAddress = new Uri(BASE_URL);
                httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this._applicationKey);

                var json = JsonConvert.SerializeObject(body);
                var result = await httpClient.PostAsync(path, new StringContent(json, Encoding.UTF8, "application/json"));
                var content = await result.Content.ReadAsStringAsync();
                if (!result.IsSuccessStatusCode)
                {
                    throw new Exception(content);
                }
                return content;
            }
        }

    }
}
