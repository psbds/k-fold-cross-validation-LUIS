using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Psbds.LUIS.Experiment.Core.Model
{
    [Serializable]
    public class ApplicationVersionModel
    {

        [JsonProperty("luis_schema_version")]
        public string LuisSchemaVersion { get; set; }

        [JsonProperty("versionId")]
        public string VersionId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("desc")]
        public string Description { get; set; }

        [JsonProperty("culture")]
        public string Culture { get; set; }

        [JsonProperty("intents")]
        public ApplicationVersionIntentModel[] Intents { get; set; }

        [JsonProperty("composites")]
        public object[] CompositeEntities { get; set; }

        [JsonProperty("entities")]
        public ApplicationVersionIntentModel[] Entities { get; set; }

        [JsonProperty("closedLists")]
        public ApplicationVersionEntityClosedListsModel[] EntitiesClosedList { get; set; }

        [JsonProperty("bing_entities")]
        public string[] BingEntities { get; set; }

        [JsonProperty("model_features")]
        public ApplicationVersionModelFeatureModel[] ModelFeatures { get; set; }

        [JsonProperty("regex_features")]
        public string[] RegexFeatures { get; set; }

        [JsonProperty("utterances")]
        public ApplicationVersionUtteranceModel[] Utterances { get; set; }

    }


    [Serializable]
    public class ApplicationVersionIntentModel
    {

        [JsonProperty("name")]
        public string Name { get; set; }

    }

    [Serializable]
    public class ApplicationVersionEntityModel
    {

        [JsonProperty("name")]
        public string Name { get; set; }

    }

    [Serializable]
    public class ApplicationVersionEntityClosedListsModel
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("subLists")]
        public ApplicationVersionEntityClosedListSubListModel[] SubLists { get; set; }

    }

    [Serializable]
    public class ApplicationVersionEntityClosedListSubListModel
    {

        [JsonProperty("canonicalForm")]
        public string CanonicalForm { get; set; }

        [JsonProperty("list")]
        public string[] List { get; set; }

    }

    [Serializable]
    public class ApplicationVersionModelFeatureModel
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("mode")]
        public bool Mode { get; set; }

        [JsonProperty("words")]
        public string Words { get; set; }

        [JsonProperty("activated")]
        public bool Activated { get; set; }

    }

    [Serializable]
    public class ApplicationVersionUtteranceModel
    {

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("entities")]
        public ApplicationVersionUtteranceEntityModel[] Entities { get; set; }

    }

    [Serializable]
    public class ApplicationVersionUtteranceEntityModel
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("startPos")]
        public int StartPos { get; set; }

        [JsonProperty("endPos")]
        public int EndPos { get; set; }

    }
}
