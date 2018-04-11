using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Model
{
    [Serializable]
    public class TestResultModel
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("tokenizedText")]
        public string[] TokenizedText { get; set; }

        [JsonProperty("intentId")]
        public string IntentId { get; set; }

        [JsonProperty("intentLabel")]
        public string intentLabel { get; set; }

        [JsonProperty("entityLabels")]
        public TestResultEntityModel[] EntityLabels { get; set; }

        [JsonProperty("intentPredictions")]
        public TestResultIntentModel[] IntentPredictions { get; set; }


        [JsonIgnore]
        public TestResultIntentModel FirstIntent
        {
            get
            {
                return IntentPredictions.OrderByDescending(y => y.Score).FirstOrDefault();
            }
        }

        [JsonIgnore]
        public TestResultIntentModel SecondIntent
        {
            get
            {
                return IntentPredictions.OrderByDescending(y => y.Score).Skip(1).FirstOrDefault();
            }
        }
    }

    [Serializable]
    public class TestResultEntityModel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("entityName")]
        public string EntityName { get; set; }

        [JsonProperty("startTokenIndex")]
        public int StartTokenIndex { get; set; }

        [JsonProperty("endTokenIndex")]
        public int EndTokenIndex { get; set; }

        [JsonProperty("entityType")]
        public int EntityType { get; set; }

    }

    [Serializable]
    public class TestResultIntentModel
    {

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("score")]
        public double Score { get; set; }

    }
}
