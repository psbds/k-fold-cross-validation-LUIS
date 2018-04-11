using Newtonsoft.Json;
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
            var confusionMatrix = new List<ConfusionMatrixModel>();
            foreach (var result in experimentResults)
            {
                var resultList = result.ToList();
                var wrongUtterancesCount = resultList.Where(x => x.intentLabel != x.IntentPredictions.OrderByDescending(y => y.Score).FirstOrDefault().Name).Count();
                var rightUtterancesCount = resultList.Where(x => x.intentLabel == x.IntentPredictions.OrderByDescending(y => y.Score).FirstOrDefault().Name).Count();
                var accuracy = ((double)rightUtterancesCount / (double)resultList.Count()) * 100;
                accuracies.Add(accuracy);
                ColoredConsole.WriteLine($"Test Results for test {count}: Accuracy: {accuracy.ToString("F2")}%. {rightUtterancesCount} Right, {wrongUtterancesCount} Wrong.", ConsoleColor.Cyan);

                resultList.ForEach(utterance =>
                {
                    var isCorrect = utterance.intentLabel == utterance.FirstIntent.Name;
                    if (!isCorrect)
                    {
                        var confusionModel = confusionMatrix.FirstOrDefault(x => x.IntentName == utterance.intentLabel);
                        if (confusionModel == null)
                        {
                            confusionModel = new ConfusionMatrixModel(utterance.intentLabel);
                            confusionMatrix.Add(confusionModel);
                        }
                        var confusion = confusionModel.Confusions.FirstOrDefault(x => x.IntentName == utterance.FirstIntent.Name);
                        if (confusion == null)
                        {
                            confusion = new ConfusionMatrixItemModel(utterance.FirstIntent.Name);
                            confusionModel.Confusions.Add(confusion);
                        }
                        confusion.Count++;
                    }

                    utterancesFile.AppendLine($"" +
                        $"{isCorrect};" +
                        $"{utterance.Text};" +
                        $"{utterance.intentLabel};" +
                        $"{utterance.FirstIntent.Name}({utterance.FirstIntent.Score});" +
                        $"{utterance.SecondIntent.Name}({utterance.SecondIntent.Score});");
                });

                count++;
            }

            ColoredConsole.WriteLine($"Average Accuracy Result: {(accuracies.Sum() / accuracies.Count()).ToString("F2")}%", ConsoleColor.Cyan);

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

            WriteConfusionFile(confusionFile.ToString());
            WriteUtterancesFile(utterancesFile.ToString());

            ColoredConsole.WriteLine($"Experiment Run in {stopWatch.Elapsed.TotalSeconds} Seconds.", ConsoleColor.Green);
            System.Console.Read();

        }


        private static void WriteConfusionFile(string confusionFile)
        {
            using (var writer = new StreamWriter($"{directory}/ConfusionMatrix-{DateTime.Now.ToString().Replace(":", "-")}.csv", false, Encoding.UTF8))
            {
                writer.Write(confusionFile);
            }
        }

        private static void WriteUtterancesFile(string utterancesFile)
        {
            using (var writer = new StreamWriter($"{directory}/ExperimentResults-{DateTime.Now.ToString().Replace(":", "-")}.csv", false, Encoding.UTF8))
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
