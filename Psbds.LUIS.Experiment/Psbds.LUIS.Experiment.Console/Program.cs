using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Psbds.LUIS.Experiment.Core;
using Psbds.LUIS.Experiment.Core.Helpers;
using Psbds.LUIS.Experiment.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Psbds.LUIS.Experiment.Console
{
    class Program
    {
        public static string appId;
        public static string appKey;
        public static string appVersion;
        public static string directory;

        static void Main(string[] args)
        {
            AskInformation(ref appId, "Please provide your application id or EXIT:");
            AskInformation(ref appKey, "Please provide your application key or EXIT:");
            AskInformation(ref appVersion, "Please provide your app version or EXIT:");
            AskInformation(ref directory, "Please provide your directory for saving the files or EXIT:");
            if (!Directory.Exists(directory))
            {
                throw new Exception("Directoy does not exists.");
            }
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var experiment = new Core.Experiment(appKey);
            var experimentResults = experiment.RunExperiment(appId, appVersion, true, 5).Result;

            stopWatch.Stop();

            var count = 0;
            var utterancesFile = new StringBuilder();
            utterancesFile.AppendLine("correct;utterance;expected_intent;first_intent;second_intent");
            var accuracies = new List<double>();
            foreach (var result in experimentResults)
            {
                var resultList = result.ToList();

                ColoredConsole.WriteLine($"Test Results for test {count}:", ConsoleColor.Green);
                ColoredConsole.WriteLine($"\t Accuracy Intent 1        : {result.AccuracyOnFirstIntent().ToString("F2")}%.", ConsoleColor.Cyan);
                ColoredConsole.WriteLine($"\t Accuracy Intent 1/2      : {result.AccuracyUntilSecondtIntent().ToString("F2")}%.", ConsoleColor.Cyan);
                ColoredConsole.WriteLine($"\t Accuracy Intent 1/2/3    : {result.AccuracyUntilThirdIntent().ToString("F2")}%.", ConsoleColor.Cyan);
                ColoredConsole.WriteLine($"\t Accuracy Intent 1/2/3/4  : {result.AccuracyUntilForthIntent().ToString("F2")}%.", ConsoleColor.Cyan);
                ColoredConsole.WriteLine($"\t Accuracy Intent 1/2/3/4/5: {result.AccuracyUntilFifthIntent().ToString("F2")}%.", ConsoleColor.Cyan);

                resultList.ForEach(utterance =>
                {
                    utterancesFile.AppendLine($"" +
                        $"{utterance.IsCorrect};" +
                        $"{utterance.Text};" +
                        $"{utterance.IntentLabel};" +
                        $"{utterance.FirstIntent.Name}({utterance.FirstIntent.Score});" +
                        $"{utterance.SecondIntent.Name}({utterance.SecondIntent.Score});");
                });

                count++;
            }

            ColoredConsole.WriteLine($"Average Accuracy", ConsoleColor.Green);
            ColoredConsole.WriteLine($"\t Accuracy Intent 1        : {(experimentResults.Sum(x => x.AccuracyOnFirstIntent()) / experimentResults.Count()).ToString("F2")}%.", ConsoleColor.Cyan);
            ColoredConsole.WriteLine($"\t Accuracy Intent 1/2      : {(experimentResults.Sum(x => x.AccuracyUntilSecondtIntent()) / experimentResults.Count()).ToString("F2")}%.", ConsoleColor.Cyan);
            ColoredConsole.WriteLine($"\t Accuracy Intent 1/2/3    : {(experimentResults.Sum(x => x.AccuracyUntilThirdIntent()) / experimentResults.Count()).ToString("F2")}%.", ConsoleColor.Cyan);
            ColoredConsole.WriteLine($"\t Accuracy Intent 1/2/3/4  : {(experimentResults.Sum(x => x.AccuracyUntilForthIntent()) / experimentResults.Count()).ToString("F2")}%.", ConsoleColor.Cyan);
            ColoredConsole.WriteLine($"\t Accuracy Intent 1/2/3/4/5: {(experimentResults.Sum(x => x.AccuracyUntilFifthIntent()) / experimentResults.Count()).ToString("F2")}%.", ConsoleColor.Cyan);

            var confusionMatrix = experiment.CreateConfusionMatrix(experimentResults);

            WriteConfusionHtmlFile(confusionMatrix);

            WriteConfusionFile(confusionMatrix.MatrixItems);

            WriteUtterancesFile(utterancesFile.ToString());

            ColoredConsole.WriteLine($"Experiment Run in {stopWatch.Elapsed.TotalSeconds} Seconds.", ConsoleColor.Green);
            System.Console.Read();

        }

        private static void WriteConfusionFile(List<MatrixItem> confusionMatrix)
        {
            var confusionFile = new StringBuilder();
            confusionFile.AppendLine("confusion_count;intent;confusions");
            foreach (var intent in confusionMatrix.OrderByDescending(x => x.Confusions.Count))
            {
                var line = $"{intent.Confusions.Sum(x => x.Utterances.Count)};{intent.ExpectedIntentName}";
                foreach (var conf in intent.Confusions)
                {
                    line += $";{conf.FoundIntent}({conf.Utterances.Count})";
                }
                confusionFile.AppendLine(line);
            }

            using (var writer = new StreamWriter($"{directory}/ConfusionMatrix-{DateTime.Now.ToString().CleanFileName()}.csv", false, Encoding.UTF8))
            {
                writer.Write(confusionFile);
            }
        }

        private static void WriteConfusionHtmlFile(ConfusionMatrixAnalysis confusionMatrix)
        {

            var templateFilePath = Path.Combine(AppContext.BaseDirectory, "templates", "results.html");
            string templateFile = File.ReadAllText(templateFilePath, Encoding.UTF8);


            var maxConfusions = confusionMatrix.MatrixItems.Max(x => x.Confusions.Count);

            var confusionColumns = new StringBuilder();
            var confusionHeaders = new StringBuilder();
            for (var i = 0; i < maxConfusions; i++)
            {
                confusionHeaders.AppendLine($"<th style='width:150px !important;'>intent_{i + 1}</th>");
                confusionColumns.AppendLine(JsonConvert.SerializeObject(new { data = $"intent_{i + 1}" }) + ",");
            }
            templateFile = templateFile.Replace("#CONFUSION_COLUMNS#", confusionColumns.ToString());
            templateFile = templateFile.Replace("#CONFUSION_HEADERS#", confusionHeaders.ToString());

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

            var data = confusionMatrix.MatrixItems.Select(x =>
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

            templateFile = templateFile.Replace("#DATA#", JsonConvert.SerializeObject(data));


            var intents = new JObject();
            confusionMatrix.ApplicationVersion.Utterances.GroupBy(x => x.Intent).ToList().ForEach(item =>
            {
                intents.Add(item.Key, new JArray(item.Select(x => x.Text).ToArray()));
            });

            templateFile = templateFile.Replace("#INTENTS#", JsonConvert.SerializeObject(intents));

            using (var writer = new StreamWriter($"{directory}/ConfusionMatrix-{DateTime.Now.ToString().CleanFileName()}.html", false, Encoding.UTF8))
            {
                writer.Write(templateFile);
            }

        }

        private static void WriteUtterancesFile(string utterancesFile)
        {
            using (var writer = new StreamWriter($"{directory}/ExperimentResults-{DateTime.Now.ToString().CleanFileName()}.csv", false, Encoding.UTF8))
            {
                writer.Write(utterancesFile);
            }
        }

        private static void AskInformation(ref string value, string phrase)
        {
            System.Console.WriteLine(phrase);
            value = System.Console.ReadLine();
            if (value.ToUpper() == "EXIT")
            {
                throw new ArgumentException();
            }
            if (String.IsNullOrEmpty(value))
                AskInformation(ref value, phrase);
        }

    }

}
