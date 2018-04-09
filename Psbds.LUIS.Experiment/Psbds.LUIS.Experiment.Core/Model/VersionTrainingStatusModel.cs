using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Model
{
    [Serializable]
    public class VersionTrainingStatusModel
    {
        [JsonProperty("modelId")]
        public string ModelId { get; set; }

        [JsonProperty("details")]
        public VersionTrainingStatusDetailsModel Details { get; set; }
    }

    [Serializable]
    public class VersionTrainingStatusDetailsModel
    {

        [JsonProperty("statusId")]
        public int StatusId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("exampleCount")]
        public long ExampleCount { get; set; }

    }
}
