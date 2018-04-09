using Newtonsoft.Json;
using Psbds.LUIS.Experiment.Core;
using Psbds.LUIS.Experiment.Core.Model;
using System;
using System.Collections.Generic;
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
            var client = new LuisClient(applicationKey);

            var data = client.ExportApplication(applicationId, "0.2").Result;
            var o = JsonConvert.DeserializeObject<ApplicationVersionModel>(data);

            var experiment = new Core.Experiment();

            var folds = experiment.SeparateFolds(o, 5).Take(1);

            var tasks = new List<TestItem>();
            for (var i = 0; i < folds.Count(); i++)
            {
                var index = i;
                var fold = folds.ElementAt(i);

                var testItem = new TestItem
                {
                    VersionId = "xp" + index
                };
                testItem.ImportingTask = Task.Run(async () =>
                {
                    var foldData = o.DeepClone();
                    foldData.Intents = foldData.Intents.Where(x => fold.TrainingSet.Any(y => y.Intent == x.Name)).ToArray();
                    foldData.Utterances = fold.TrainingSet.ToArray();
                    var response = await client.ImportVersion(applicationId, testItem.VersionId, foldData);
                    return response;
                })
                .ContinueWith(async (lastTask) =>
                {
                    var lastTaskResponse = await lastTask;
                    var result = client.TrainVersion(applicationId, testItem.VersionId).Result;
                })
                .ContinueWith(async (lastTask) =>
                {
                    var lastTaskResponse = await lastTask;
                    var isTrained = false;
                    while (!isTrained)
                    {
                        var result = await client.GetVersionTrainingStatus(applicationId, testItem.VersionId);
                        var resultObject = JsonConvert.DeserializeObject<VersionTrainingStatusModel[]>(result);
                        if (resultObject.All(x => x.Details.StatusId == 0 || x.Details.StatusId == 1))
                        {
                            isTrained = true;
                        }
                        System.Console.WriteLine("Sleeping for 10 seconds");
                        Thread.Sleep(10000);
                    }
                    return Task.CompletedTask;
                });


                /*  testItem.CreatingDataSetTask = Task.Run(async () =>
                  {
                      var testId = await client.CreateTestDataSet(applicationId, testItem.VersionId, fold.TestSet.ToArray());
                      testItem.TestId = Regex.Replace(testId, "\"", "");
                  });
                  */
                tasks.Add(testItem);
            }


            var importingTasks = tasks.Select(x => x.ImportingTask).ToArray();
            //  var dataSetTasks = tasks.Select(x => x.CreatingDataSetTask).ToArray();

            Task.WaitAll(importingTasks);
            //    Task.WaitAll(dataSetTasks);

            //     var utterances = new List<ApplicationVersionUtteranceModel>(o.Utterances).Take(10).ToArray();


            //  ;
            // var testResult = client.RunDataSetTest(applicationId, "0.2", tId).Result;
            System.Console.Read();
        }
    }

    public class TestItem
    {

        public string VersionId { get; set; }

        public Task ImportingTask { get; set; }

        public Task CreatingDataSetTask { get; set; }

        public string TestId { get; set; }
    }
    public static class ExtensionMethods
    {
        // Deep clone
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
