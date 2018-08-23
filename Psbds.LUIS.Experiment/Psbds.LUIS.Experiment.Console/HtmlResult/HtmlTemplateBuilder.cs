using Newtonsoft.Json.Linq;
using Psbds.LUIS.Experiment.Core.Model;
using RazorLight;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Console.HtmlResult
{
    public class HtmlTemplateBuilder
    {
        private ConfusionMatrixAnalysis _confusionMatrix { get; set; }

        private List<TestResultModel[]> _experimentResults { get; set; }

        private string _file { get; set; }

        private HtmlTemplateModel _template { get; set; } = new HtmlTemplateModel();

        public HtmlTemplateBuilder(ConfusionMatrixAnalysis confusionMatrix, List<TestResultModel[]> experimentResults)
        {
            _confusionMatrix = confusionMatrix;
            _experimentResults = experimentResults;
        }

        public void Build()
        {
            var maxConfusions = _confusionMatrix.MatrixItems.Max(x => x.Confusions.Count);
            _template = new HtmlTemplateModel
            {
                ErrorTab = new ErrorTab()
                {
                    MaxConfusions = maxConfusions
                }
            };

            Func<Utterance, string> FormatUtterance = (item) =>
            {
                var words = item.Text.Split(' ');
                var tokenAnalysis = item.TokenizedAnalysis;
                var falsePositiveIcon = item.FalsePosiive ? "<span class='badge badge-warning'><i class='fa fa-ban'></i></span>" : "";
                var phrase = String.Join(" ", words.Select(word =>
                {
                    var apperance = tokenAnalysis.FirstOrDefault(x => x.Token == word);
                    if (apperance != null)
                    {
                        var color = apperance.ApperancesInExpectedIntent <= 1 ? "style='color: orange;'" : "";
                        return $"<span data-toggle='tooltip' {color} data-placement='bottom' title='{apperance.ApperancesInExpectedIntent}({apperance.PercentageInExpectedIntent.ToString("F2")}%) Occurences in Expected Intent | {apperance.ApperancesInFoundIntent}({apperance.PercentageInFoundIntent.ToString("F2")}%) Occurences in Found Intent'>{word}</span>";
                    }
                    else
                    {
                        return word;
                    }
                }));
                phrase = falsePositiveIcon + phrase + $"({item.Score})";
                return phrase;
            };

            var data = _confusionMatrix.MatrixItems.Select(x =>
            {
                JObject model = new JObject();
                model["id"] = x.ExpectedIntentName;
                model["confusions"] = x.Confusions.Sum(y => y.Utterances.Count);
                model["expected_intent"] = x.ExpectedIntentName;
                var orderedConfusions = x.Confusions.OrderByDescending(c => c.Utterances.Count).ToList();
                for (var i = 0; i < maxConfusions; i++)
                {
                    var examples = new JArray(orderedConfusions.Count > i ? orderedConfusions[i].Utterances.Select(FormatUtterance).ToArray() : new string[] { });
                    var examplesData = JArray.FromObject(orderedConfusions.Count > i ? orderedConfusions[i].Utterances.ToArray() : new object[] { });
                    model.Add("examples_intent_" + (i + 1), examples);
                    model.Add("examples_data_intent_" + (i + 1), examplesData);
                    model["intent_" + (i + 1)] = orderedConfusions.Count > i ? $"{orderedConfusions[i].FoundIntent} ({orderedConfusions[i].Utterances.Count})" : "-";
                }

                return model;
            });

            var intents = new JObject();
            _confusionMatrix.ApplicationVersion.Utterances.GroupBy(x => x.Intent).ToList().ForEach(item =>
            {
                intents.Add(item.Key, new JArray(item.Select(x => x.Text).ToArray()));
            });

            _template.ErrorTab.Errors = data;
            _template.ErrorTab.Intents = intents;



            var MAIN_TEMPLATE = System.IO.File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "templates", "template.html"), Encoding.UTF8);

            var razorEngine = new RazorLightEngineBuilder().UseMemoryCachingProvider().Build();


            var ERROR_FILE_TEMPLATE = System.IO.File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "templates", "error_template.cshtml"), Encoding.UTF8);
            var ERROR_FILE = razorEngine.CompileRenderAsync("ERROR", ERROR_FILE_TEMPLATE, _template).Result;

            this._file = MAIN_TEMPLATE
                .Replace("#ERROR_TAB#", ERROR_FILE)
                .Replace("#OVERVIEW_TAB#", BuildOverviewTab());


        }

        private string BuildOverviewTab()
        {
            _template.OverviewTab = new OverviewTab();

            _template.OverviewTab.AccuraciesPerTest = _experimentResults.Select(result =>
            {
                return new OverviewTabAccuracy
                {
                    AccuracyAt1 = result.AccuracyOnFirstIntent(),
                    AccuracyAt2 = result.AccuracyUntilSecondtIntent(),
                    AccuracyAt3 = result.AccuracyUntilThirdIntent(),
                    AccuracyAt4 = result.AccuracyUntilForthIntent(),
                    AccuracyAt5 = result.AccuracyUntilFifthIntent()
                };
            }).ToList();

            var precisionSum = _experimentResults.Sum(result =>
            {
                double correctAverageScore = result.Where(x => x.IsCorrect).Sum(x => x.FirstIntent.Score);
                double correctCount = result.Where(x => x.IsCorrect).Count();
                return correctCount > 0 ? correctAverageScore / correctCount : 0;
            });
            _template.OverviewTab.AveragePrecision = (precisionSum / _experimentResults.Count) * 100;

            var razorEngine = new RazorLightEngineBuilder().UseMemoryCachingProvider().Build();
            var OVERVIEW_TEMPLATE = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "templates", "overview_template.cshtml"), Encoding.UTF8);
            return razorEngine.CompileRenderAsync("OVERVIEW", OVERVIEW_TEMPLATE, _template).Result;
        }

        public void WriteToFile(string directory, string fileName)
        {
            using (var writer = new StreamWriter($"{directory}/{fileName}.html", false, Encoding.UTF8))
            {
                writer.Write(this._file);
            }
        }

    }
}
