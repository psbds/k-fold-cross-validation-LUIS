using System;
using System.Collections.Generic;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Model
{
    public class ConfusionMatrixModel
    {
        public ConfusionMatrixModel(string intentName)
        {
            this.IntentName = intentName;
        }

        public string IntentName { get; set; }

        public List<ConfusionMatrixItemModel> Confusions { get; set; } = new List<ConfusionMatrixItemModel>();
    }

    public class ConfusionMatrixItemModel
    {
        public ConfusionMatrixItemModel(string intentName)
        {
            this.IntentName = intentName;
        }
        public string IntentName { get; set; }

        public int Count { get; set; }
    }
}
