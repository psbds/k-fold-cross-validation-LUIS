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
        static void Main(string[] args)
        {
            var applicationId = "";
            var applicationKey = "";


            var experiment = new Core.Experiment(applicationKey, applicationId);
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var t = experiment.RunExperiment().Result;
            stopWatch.Stop();

            ColoredConsole.WriteLine($"Experiment Run in {stopWatch.Elapsed.TotalSeconds} Seconds.", ConsoleColor.Green);
            System.Console.Read();

        }
    }
}
