using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Model
{
    [Serializable]
    public class ConfusionMatrixAnalysis
    {

        public static double FalsePositiveThreshold = 0.8;

        public ConfusionMatrixAnalysis(ApplicationVersionModel applicationVersion)
        {
            this.ApplicationVersion = applicationVersion;
        }

        public ApplicationVersionModel ApplicationVersion { get; private set; }

        public List<MatrixItem> MatrixItems { get; private set; } = new List<MatrixItem>();

    }

    [Serializable]
    public class MatrixItem
    {
        public MatrixItem(ConfusionMatrixAnalysis matrixAnalysis, string expectedIntentName)
        {
            this.MatrixAnalysis = matrixAnalysis;
            this.ExpectedIntentName = expectedIntentName;
        }

        public ConfusionMatrixAnalysis MatrixAnalysis { get; private set; }

        public string ExpectedIntentName { get; private set; }

        public List<Confusion> Confusions { get; private set; } = new List<Confusion>();

    }

    [Serializable]
    public class Confusion
    {
        internal Confusion(MatrixItem matrixItem, string confusedIntent)
        {
            this.MatrixItem = matrixItem;
            this.FoundIntent = confusedIntent;
        }

        [JsonIgnore]
        public MatrixItem MatrixItem { get; private set; }

        public string FoundIntent { get; private set; }

        public List<ConfusionMatrixUtterance> Utterances { get; private set; } = new List<ConfusionMatrixUtterance>();

    }

    [Serializable]
    public class ConfusionMatrixUtterance
    {
        public ConfusionMatrixUtterance(Confusion confusion, string text, string[] tokenizedText, double score, List<UtteranceIntents> intents)
        {
            this.Confusion = confusion;
            this.Text = text;
            this.Score = score;
            this.TokenizedText = tokenizedText;
            this.Intents = intents;
        }

        public ConfusionMatrixUtterance()
        {

        }

        [JsonIgnore]
        public Confusion Confusion { get; private set; }

        public string[] TokenizedText { get; private set; }

        public string Text { get; private set; }

        public double Score { get; private set; }

        public List<UtteranceIntents> Intents { get; private set; } = new List<UtteranceIntents>();

        [JsonIgnore]
        public bool FalsePosiive { get { return Score >= ConfusionMatrixAnalysis.FalsePositiveThreshold; } }

        [JsonIgnore]
        private List<TokenizedAnalysis> _tokenizedAnalysis;

        [JsonIgnore]
        public List<TokenizedAnalysis> TokenizedAnalysis
        {
            get
            {
                if (_tokenizedAnalysis == null)
                {
                    var examplesInExpectedIntent = this.Confusion.MatrixItem.MatrixAnalysis.ApplicationVersion.UtterancesByIntent.FirstOrDefault(x => x.Key == this.Confusion.MatrixItem.ExpectedIntentName);
                    var examplesInFoundIntent = this.Confusion.MatrixItem.MatrixAnalysis.ApplicationVersion.UtterancesByIntent.FirstOrDefault(x => x.Key == this.Confusion.FoundIntent);
                    var records = new List<TokenizedAnalysis>();

                    var characters = (Text ?? "").Split(' ');
                    characters.Distinct()
                    .ToList()
                    .ForEach(word =>
                    {
                        var a = this;
                        var countInExpectedIntent = examplesInExpectedIntent.Count(x => x.Text.ToUpper().Contains(word.ToUpper()));
                        var countInFoundIntent = examplesInFoundIntent != null ? examplesInFoundIntent.Count(x => x.Text.ToUpper().Contains(word.ToUpper())) : 0;
                        var percentageInExpectedIntent = ((double)countInExpectedIntent / (double)examplesInExpectedIntent.Count()) * 100;
                        var percentageInFoundIntent = ((double)countInFoundIntent / (double)(examplesInFoundIntent != null ? examplesInFoundIntent.Count() : 1)) * 100;
                        records.Add(new TokenizedAnalysis(word, countInExpectedIntent, countInFoundIntent, percentageInExpectedIntent, countInFoundIntent));
                    });

                    _tokenizedAnalysis = records;
                }
                return _tokenizedAnalysis;
            }
        }
    }

    [Serializable]
    public class TokenizedAnalysis
    {

        public TokenizedAnalysis(string token, int apperancesInExpectedIntent, int apperancesInFoundIntent, double percentageInExpectedIntent, double percentageInFoundIntent)
        {
            this.Token = token;
            this.ApperancesInExpectedIntent = apperancesInExpectedIntent;
            this.ApperancesInFoundIntent = apperancesInFoundIntent;
            this.PercentageInFoundIntent = percentageInFoundIntent;
            this.PercentageInExpectedIntent = percentageInExpectedIntent;
        }

        public string Token { get; private set; }

        public int ApperancesInExpectedIntent { get; private set; }

        public double PercentageInExpectedIntent { get; private set; }

        public int ApperancesInFoundIntent { get; private set; }

        public double PercentageInFoundIntent { get; private set; }

    }

    [Serializable]
    public class UtteranceIntents
    {
        public UtteranceIntents(string intent, double score)
        {
            this.Intent = intent;
            this.Score = score;
        }

        public string Intent { get; private set; }

        public double Score { get; private set; }

    }
}
