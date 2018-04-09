using Newtonsoft.Json;
using Psbds.LUIS.Experiment.Core.Helpers;
using Psbds.LUIS.Experiment.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Psbds.LUIS.Experiment.Core
{
    public class Experiment
    {
        private readonly string applicationId;
        private readonly string applicationKey;
        private readonly LuisClient luisClient;

        public Experiment(string applicationKey, string applicationId)
        {
            this.applicationId = applicationId;
            this.applicationKey = applicationKey;
            this.luisClient = new LuisClient(applicationKey);
        }

        public async Task<string> RunExperiment(int numberOfFolds = 5)
        {
            Console.Write($"Downloading Application Version: ");

            var applicationVersion = (await luisClient.ExportVersion(applicationId, "0.2")).DeserializeObject<ApplicationVersionModel>();

            ColoredConsole.WriteLine("Success", ConsoleColor.Green);


            var folds = this.SeparateFolds(applicationVersion, numberOfFolds);

            var tasks = new List<TestModel>();
            var index = 0;
            foreach (var fold in folds)
            {
                var testItem = new TestModel
                {
                    VersionId = Guid.NewGuid().ToString().Substring(0, 5) + index
                };

                testItem.ImportingTask = Task
                    .Run(async () =>
                    {
                        Console.WriteLine($"Uploading Version: {testItem.VersionId}");

                        await ImportFoldVersion(applicationVersion, fold, testItem.VersionId);

                        ColoredConsole.WriteLine($"Success Uploading Version: {testItem.VersionId}", ConsoleColor.Green);

                        Console.WriteLine($"Training Version: {testItem.VersionId}");

                        await luisClient.TrainVersion(applicationId, testItem.VersionId);

                        var isTrained = false;
                        while (!isTrained)
                        {
                            var result = await luisClient.GetVersionTrainingStatus(applicationId, testItem.VersionId);
                            var resultObject = JsonConvert.DeserializeObject<TrainingStatusModel[]>(result);
                            if (resultObject.All(x => x.Details.StatusId == 0 || x.Details.StatusId == 1))
                            {
                                isTrained = true;
                            }
                            Console.WriteLine($"Version  {testItem.VersionId} is Training. Sleeping for 10 seconds", ConsoleColor.Magenta);
                            Thread.Sleep(10000);
                        }

                        ColoredConsole.WriteLine($"Success Training Version: {testItem.VersionId}", ConsoleColor.Green);

                    });


                testItem.CreatingDataSetTask = Task.Run(async () =>
                {
                    var testId = await luisClient.CreateTestDataSet(applicationId, testItem.VersionId, fold.TestSet.ToArray());
                    testItem.TestId = Regex.Replace(testId, "\"", "");
                });

                tasks.Add(testItem);
                index++;
            }


            var importingTasks = tasks.Select(x => x.ImportingTask).ToArray();
            var dataSetTasks = tasks.Select(x => x.CreatingDataSetTask).ToArray();

            Task.WaitAll(importingTasks);
            Task.WaitAll(dataSetTasks);

            var runTestTasks = tasks.Select(test => Task.Run(() => luisClient.RunDataSetTest(applicationId, test.VersionId, test.TestId).Result)).ToArray();

            Task.WaitAll(runTestTasks);

            var count = 0;
            foreach (var test in runTestTasks)
            {
                var res = test.Result.DeserializeObject<TestResultModel[]>();
                var wrongUtterances = res.Where(x => x.intentLabel != x.IntentPredictions.OrderByDescending(y => y.Score).FirstOrDefault().Name);
                var rightUtterances = res.Where(x => x.intentLabel == x.IntentPredictions.OrderByDescending(y => y.Score).FirstOrDefault().Name);
                ColoredConsole.WriteLine($"Test Results for test {count}: Accuracy: {((double)rightUtterances.Count() / (double)res.Count()) * 100}%. {rightUtterances.Count()} Right, {wrongUtterances.Count()} Wrong.", ConsoleColor.Cyan);
                count++;
            }
            return "a";
        }

        #region [ Creating Folds ]

        private List<FoldModel> SeparateFolds(ApplicationVersionModel model, int numberOfFolds)
        {
            var folds = CreateFolds(numberOfFolds);

            var utterancesByIntent = ExtractUtterancesForFolds(model, 0);

            var values = utterancesByIntent.SelectMany(x => x).Split(numberOfFolds);
            for (var i = 0; i < numberOfFolds; i++)
            {
                var fold = folds[i];
                fold.TestSet.AddRange(values.ElementAt(i).ToList());

                folds.Where(x => x != fold).ToList().ForEach(x => x.TrainingSet.AddRange(values.ElementAt(i)));
            }
            return folds;
        }

        /*    private List<FoldModel> SeparateFolds(ApplicationVersionModel model, int numberOfFolds)
            {
                var folds = CreateFolds(numberOfFolds);

                var utterancesByIntent = ExtractUtterancesForFolds(model, numberOfFolds);

                foreach (var utteranceGroup in utterancesByIntent)
                {
                    var values = utteranceGroup.Split(numberOfFolds).ToArray();

                    for (var i = 0; i < numberOfFolds; i++)
                    {
                        var fold = folds[i];
                        fold.TestSet.AddRange(values[i].ToList());

                        folds.Where(x => x != fold).ToList().ForEach(x => x.TrainingSet.AddRange(values[i]));
                    }
                }

                return folds;
            }*/

        private IEnumerable<IGrouping<string, ApplicationVersionUtteranceModel>> ExtractUtterancesForFolds(ApplicationVersionModel model, int ignoreLessThan)
        {
            var utterancesByIntent = new List<ApplicationVersionUtteranceModel>(model.Utterances).GroupBy(x => x.Intent);
            utterancesByIntent.Where(x => x.Count() < ignoreLessThan).ToList().ForEach(x => Console.WriteLine($"Ignored Intent #{x.Key}: {x.Count()} Values"));
            utterancesByIntent = utterancesByIntent.Where(x => x.Count() >= ignoreLessThan);
            return utterancesByIntent;
        }

        private List<FoldModel> CreateFolds(int numberOfFolds)
        {
            var list = new List<FoldModel>();
            for (var i = 0; i < numberOfFolds; i++)
            {
                list.Add(new FoldModel());
            }
            return list;
        }

        #endregion

        private async Task<string> ImportFoldVersion(ApplicationVersionModel applicationVersion, FoldModel fold, string versionName)
        {
            var foldVersion = applicationVersion.DeepClone();
            foldVersion.Utterances = fold.TrainingSet.ToArray();
            foldVersion.Intents = foldVersion.Intents.Where(x => foldVersion.Utterances.Any(y => y.Intent == x.Name)).ToArray();
            var response = await luisClient.ImportVersion(this.applicationId, versionName, foldVersion);
            return response;
        }

        private async Task WaitForTraining(string versionId)
        {
            var isTrained = false;
            while (!isTrained)
            {
                var result = await luisClient.GetVersionTrainingStatus(applicationId, versionId);
                var resultObject = JsonConvert.DeserializeObject<TrainingStatusModel[]>(result);
                if (resultObject.All(x => x.Details.StatusId == 0 || x.Details.StatusId == 2))
                {
                    isTrained = true;
                }
                Console.WriteLine("Sleeping for 10 seconds");
                Thread.Sleep(10000);
            }
        }
    }


}
