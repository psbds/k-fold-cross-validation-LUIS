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

            var experiment = new Core.Experiment(appKey);

            var stopWatch = new Stopwatch();
            stopWatch.Start();


            var experimentResults = experiment.RunExperiment(appId, appVersion).Result;


            stopWatch.Stop();

            ColoredConsole.WriteLine($"Experiment Run in {stopWatch.Elapsed.TotalSeconds} Seconds.", ConsoleColor.Green);

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

            WriteConfusionHtmlFile(experimentResults, confusionMatrix);

            WriteConfusionFile(confusionMatrix);

            WriteUtterancesFile(utterancesFile.ToString());

            ColoredConsole.WriteLine($"Experiment Run in {stopWatch.Elapsed.TotalSeconds} Seconds.", ConsoleColor.Green);
            System.Console.Read();

        }

        private static void WriteConfusionFile(List<ConfusionMatrixModel> confusionMatrix)
        {
            var confusionFile = new StringBuilder();
            confusionFile.AppendLine("confusion_count;intent;confusions");
            foreach (var intent in confusionMatrix.OrderByDescending(x => x.Confusions.Count))
            {
                var line = $"{intent.Confusions.Sum(x => x.Count)};{intent.IntentName}";
                foreach (var conf in intent.Confusions)
                {
                    line += $";{conf.IntentName}({conf.Count})";
                }
                confusionFile.AppendLine(line);
            }

            using (var writer = new StreamWriter($"{directory}/ConfusionMatrix-{DateTime.Now.ToString().CleanFileName()}.csv", false, Encoding.UTF8))
            {
                writer.Write(confusionFile);
            }
        }


        private static void WriteConfusionHtmlFile(List<TestResultModel[]> experimentResults, List<ConfusionMatrixModel> confusionMatrix)
        {
            var applicationModel = experimentResults.SelectMany(x => x);


            var templateFilePath = Path.Combine(AppContext.BaseDirectory, "templates", "results.html");
            string templateFile = File.ReadAllText(templateFilePath, Encoding.UTF8);


            var maxConfusions = confusionMatrix.Max(x => x.Confusions.Count);

            var confusionColumns = new StringBuilder();
            var confusionHeaders = new StringBuilder();
            for (var i = 0; i < maxConfusions; i++)
            {
                confusionHeaders.AppendLine($"<th style='width:150px !important;'>intent_{i + 1}</th>");
                confusionColumns.AppendLine(JsonConvert.SerializeObject(new { data = $"intent_{i + 1}" }) + ",");
            }
            templateFile = templateFile.Replace("#CONFUSION_COLUMNS#", confusionColumns.ToString());
            templateFile = templateFile.Replace("#CONFUSION_HEADERS#", confusionHeaders.ToString());

            Func<ConfusionMatrixItemUtteranceModel, string> FormatUtterance = (item) =>
             {
                 var words = item.Text.Split(' ');
                 var apperances = item.ApperancesInModel(applicationModel.Where(x => x.IntentLabel == item.Intent).Select(x => x.Text).ToList(), item.Intent);
                 var phrase = String.Join(" ", words.Select(word =>
                 {
                     if (apperances.ContainsKey(word))
                     {
                         var color = apperances[word] <= 1 ? "style='color: orange;'" : "";
                         return $"<span data-toggle='tooltip' {color} data-placement='bottom' title='{apperances[word]} Occurence'>{word}</span>";
                     }
                     else
                     {
                         return word;
                     }
                 }));
                 phrase += $"({item.Score})";
                 return phrase;
             };

            var data = confusionMatrix.Select(x =>
            {
                JObject model = new JObject();
                model["id"] = x.IntentName;
                model["confusions"] = x.Confusions.Sum(y => y.Count);
                model["expected_intent"] = x.IntentName;
                var orderedConfusions = x.Confusions.OrderByDescending(c => c.Count).ToList();
                for (var i = 0; i < maxConfusions; i++)
                {
                    var examples = new JArray(orderedConfusions.Count > i ? orderedConfusions[i].Utterances.Select(FormatUtterance).ToArray() : new string[] { });
                    model.Add("examples_intent_" + (i + 1), examples);
                    model["intent_" + (i + 1)] = orderedConfusions.Count > i ? $"{orderedConfusions[i].IntentName} ({orderedConfusions[i].Count})" : "-";
                }

                return model;
            });

            templateFile = templateFile.Replace("#DATA#", JsonConvert.SerializeObject(data));
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
