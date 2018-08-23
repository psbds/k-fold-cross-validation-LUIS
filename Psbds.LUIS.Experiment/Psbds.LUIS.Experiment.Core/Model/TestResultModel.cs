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
        public string IntentLabel { get; set; }

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


        [JsonIgnore]
        public TestResultIntentModel ThirdIntent
        {
            get
            {
                return IntentPredictions.OrderByDescending(y => y.Score).Skip(2).FirstOrDefault();
            }
        }

        [JsonIgnore]
        public TestResultIntentModel ForthIntent
        {
            get
            {
                return IntentPredictions.OrderByDescending(y => y.Score).Skip(3).FirstOrDefault();
            }
        }

        [JsonIgnore]
        public TestResultIntentModel FifthIntent
        {
            get
            {
                return IntentPredictions.OrderByDescending(y => y.Score).Skip(4).FirstOrDefault();
            }
        }

        [JsonIgnore]
        public bool IsCorrect
        {
            get
            {
                return IntentLabel == FirstIntent?.Name;
            }
        }

        [JsonIgnore]
        public bool IsCorrectOnSecond
        {
            get
            {
                return IntentLabel == SecondIntent?.Name;
            }
        }

        [JsonIgnore]
        public bool IsCorrectOnThird
        {
            get
            {
                return IntentLabel == ThirdIntent?.Name;
            }
        }

        [JsonIgnore]
        public bool IsCorrectOnFourth
        {
            get
            {
                return IntentLabel == ForthIntent?.Name;
            }
        }


        [JsonIgnore]
        public bool IsCorrectOnFifth
        {
            get
            {
                return IntentLabel == FifthIntent?.Name;
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

    [Serializable]
    public static class TestResultModelExtensions
    {

        public static double AccuracyOnFirstIntent(this TestResultModel[] list)
        {
            var correct = list.Where(x => x.IsCorrect).Count();
            var accuracy = ((double)correct / (double)list.Count()) * 100;

            return accuracy;
        }

        public static double AccuracyUntilSecondtIntent(this TestResultModel[] list)
        {
            var correct = list.Where(x => x.IsCorrect || x.IsCorrectOnSecond).Count();
            var accuracy = ((double)correct / (double)list.Count()) * 100;

            return accuracy;
        }

        public static double AccuracyUntilThirdIntent(this TestResultModel[] list)
        {
            var correct = list.Where(x => x.IsCorrect || x.IsCorrectOnSecond || x.IsCorrectOnThird).Count();
            var accuracy = ((double)correct / (double)list.Count()) * 100;

            return accuracy;
        }

        public static double AccuracyUntilForthIntent(this TestResultModel[] list)
        {
            var correct = list.Where(x => x.IsCorrect || x.IsCorrectOnSecond || x.IsCorrectOnThird || x.IsCorrectOnFourth).Count();
            var accuracy = ((double)correct / (double)list.Count()) * 100;

            return accuracy;
        }

        public static double AccuracyUntilFifthIntent(this TestResultModel[] list)
        {
            var correct = list.Where(x => x.IsCorrect || x.IsCorrectOnSecond || x.IsCorrectOnThird || x.IsCorrectOnFourth || x.IsCorrectOnFifth).Count();
            var accuracy = ((double)correct / (double)list.Count()) * 100;

            return accuracy;
        }

    }
}
