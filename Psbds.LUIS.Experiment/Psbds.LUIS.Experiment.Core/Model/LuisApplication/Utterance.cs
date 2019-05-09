using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Model.LuisApplication
{
    /// <summary>
    /// This Class Representes an Utterance in LUIS Model
    /// <see cref="https://westus.dev.cognitive.microsoft.com/docs/services/5890b47c39e2bb17b84a55ff/operations/5890b47c39e2bb052c5b9c08"/>
    /// </summary>
    [Serializable]
    public class Utterance
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("intent")]
        public string Intent { get; set; }

        [JsonProperty("entities")]
        public UtteranceEntity[] Entities { get; set; }
    }
}
