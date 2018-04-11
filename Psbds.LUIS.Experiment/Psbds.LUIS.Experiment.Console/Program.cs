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

            var count = 0;
            foreach (var res in experimentResults)
            {
                var wrongUtterances = res.Where(x => x.intentLabel != x.IntentPredictions.OrderByDescending(y => y.Score).FirstOrDefault().Name);
                var rightUtterances = res.Where(x => x.intentLabel == x.IntentPredictions.OrderByDescending(y => y.Score).FirstOrDefault().Name);
                ColoredConsole.WriteLine($"Test Results for test {count}: Accuracy: {((double)rightUtterances.Count() / (double)res.Count()) * 100}%. {rightUtterances.Count()} Right, {wrongUtterances.Count()} Wrong.", ConsoleColor.Cyan);
                count++;
            }

            ColoredConsole.WriteLine($"Experiment Run in {stopWatch.Elapsed.TotalSeconds} Seconds.", ConsoleColor.Green);
            System.Console.Read();

        }

        public static void AskInformation(ref string value, string phrase)
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
