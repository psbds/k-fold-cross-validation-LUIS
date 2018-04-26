using System;
using System.Collections.Generic;
using System.Linq;
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

        public List<ConfusionMatrixItemUtteranceModel> Utterances { get; set; } = new List<ConfusionMatrixItemUtteranceModel>();

        public int Count { get; set; }
    }

    public class ConfusionMatrixItemUtteranceModel
    {
        public ConfusionMatrixItemUtteranceModel(string intent)
        {
            Intent = intent;
        }

        public string Intent { get; }

        public string Text { get; set; }

        public double Score { get; set; }


        public Dictionary<string, int> ApperancesInModel(List<string> list, string intentName)
        {
            var examples = list.Where(x => x != Text);
            var records = new Dictionary<string, int>();
            var characters = (Text == null ? "" : Text).Split(' ');

            characters.Distinct()
                .ToList()
                .ForEach(word =>
                {
                    var count = examples.Count(x => x.ToUpper().Contains(word.ToUpper()));
                    records.Add(word, count);
                });

            return records;
        }


    }
}
