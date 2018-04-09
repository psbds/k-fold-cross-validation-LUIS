using Newtonsoft.Json;
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



        public async Task<String> ImportVersion(string applicationId, string version, object body)
        {
            var url = $"{API_URL}/{applicationId}/versions/import?versionId={version}";
            Console.WriteLine($"Importing Application: {url}");

            var response = await SendPostRequest(url, body);
            Console.WriteLine($"Finished Importing Application: {url}");

            return response;
        }

        public async Task<String> TrainVersion(string applicationId, string version)
        {
            var url = $"{API_URL}/{applicationId}/versions/{version}/train";
            Console.WriteLine($"Send Training Request Application: {url}");

            var response = await SendPostRequest(url, null);
            Console.WriteLine($"Finished Send Training Request Application: {url}");

            return response;
        }

        public async Task<String> GetVersionTrainingStatus(string applicationId, string version)
        {
            var url = $"{API_URL}/{applicationId}/versions/{version}/train";
            Console.WriteLine($"Get Training Status: {url}");

            var response = await SendGetRequest(url);
            Console.WriteLine($"Finished Get Training Status: {url}");

            return response;
        }




        public async Task<String> RunDataSetTest(string applicationId, string version, string dataSetId)
        {
            var url = $"{WEB_API_URL}/{applicationId}/versions/{version}/testdatasets/{dataSetId}/run";
            Console.WriteLine($"Running Data Set: {url}");
            var response = await SendGetRequest(url);

            Console.WriteLine($"Finished Running Data Se: {url}");

            return response;
        }

        public async Task<String> CreateTestDataSet(string applicationId, string dataSetName, object body)
        {
            var url = $"{WEB_API_URL}/{applicationId}/testdatasets?dataSetName={dataSetName}";
            Console.WriteLine($"Creating Data Set: {url}");
            var response = await SendPostRequest(url, body);
            Console.WriteLine($"Finished Creating Data Set: {url}");

            return response;
        }

        public async Task<String> ExportApplication(string applicationId, string applicationVersion)
        {
            var url = $"{API_URL}/{applicationId}/versions/{applicationVersion}/export";
            Console.WriteLine($"Exporting Application: {url}");

            var response = await SendGetRequest(url);

            Console.WriteLine($"Finished Exporting Application: {url}");

            return response;
        }


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
