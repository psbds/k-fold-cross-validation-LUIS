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
    [Serializable]
    public class Experiment
    {
        private string _applicationId;
        private readonly string _applicationKey;
        private readonly LuisClient _luisClient;

        [JsonProperty("applicationVersion")]
        private ApplicationVersionModel _applicationVersion;

        public Experiment(string applicationKey)
        {
            this._applicationKey = applicationKey;
            this._luisClient = new LuisClient(applicationKey);
        }

        public async Task<List<TestResultModel[]>> RunExperiment(string applicationId, string versionId, int numberOfFolds = 5)
        {
            try
            {
                Console.Write($"Downloading Application Version: ");

                _applicationVersion = (await _luisClient.ExportVersion(applicationId, versionId)).DeserializeObject<ApplicationVersionModel>();

                ColoredConsole.WriteLine("Success", ConsoleColor.Green);

                Console.Write($"Creating Experiment Application: ");

                _applicationId = await this.CreateApplication(_applicationVersion.Name, _applicationVersion.Culture);

                ColoredConsole.WriteLine("Success", ConsoleColor.Green);

                var folds = this.SeparateFolds(_applicationVersion, numberOfFolds);

                var tasks = new List<Task<string>>();
                var index = 1;
                foreach (var fold in folds)
                {
                    tasks.Add(Task.Run(async () => await RunExperiment(_applicationVersion, fold, index++)));
                }

                Task.WaitAll(tasks.ToArray());



                return tasks.Select(x => x.Result.DeserializeObject<TestResultModel[]>()).ToList();
            }
            catch (Exception e)
            {
                ColoredConsole.WriteLine(e.Message, ConsoleColor.Red);
                throw;
            }
            finally
            {
                if (!string.IsNullOrEmpty(_applicationId))
                {
                    Console.Write($"Deleting Experiment Application: ");

                    await this._luisClient.DeleteApplication(_applicationId);

                    ColoredConsole.WriteLine("Success", ConsoleColor.Green);
                }
                this._luisClient.Dispose();
            }
        }

        public ConfusionMatrixAnalysis CreateConfusionMatrix(List<TestResultModel[]> foldResults)
        {
            var confusionMatrix = new ConfusionMatrixAnalysis(this._applicationVersion);

            foreach (var testResults in foldResults)
            {
                foreach (var utterance in testResults.Where(x => !x.IsCorrect))
                {
                    var confusionModel = confusionMatrix.MatrixItems.FirstOrDefault(x => x.ExpectedIntentName == utterance.IntentLabel);
                    if (confusionModel == null)
                    {
                        confusionModel = new MatrixItem(confusionMatrix, utterance.IntentLabel);
                        confusionMatrix.MatrixItems.Add(confusionModel);
                    }
                    var confusion = confusionModel.Confusions.FirstOrDefault(x => x.FoundIntent == utterance.FirstIntent.Name);
                    if (confusion == null)
                    {
                        confusion = new Confusion(confusionModel, utterance.FirstIntent.Name);
                        confusionModel.Confusions.Add(confusion);
                    }
                    var predictions = utterance.IntentPredictions.OrderByDescending(x => x.Score).Take(10).Select(x => new UtteranceIntents(x.Name, x.Score)).ToList();
                    confusion.Utterances.Add(new Utterance(confusion, utterance.Text, utterance.TokenizedText, utterance.FirstIntent.Score,predictions));
                }
            }

            return confusionMatrix;
        }

        private async Task<string> RunExperiment(ApplicationVersionModel applicationVersion, FoldModel fold, int index)
        {
            var versionId = Guid.NewGuid().ToString().Substring(0, 5) + index;

            try
            {
                Console.WriteLine($"Uploading Version: {index}");

                await ImportFoldVersion(applicationVersion, fold, versionId);

                ColoredConsole.WriteLine($"Success Uploading Version: {index}", ConsoleColor.Green);

                Console.WriteLine($"Training Version: {index}");

                await _luisClient.TrainVersion(_applicationId, versionId);

                await this.WaitForTraining(versionId);

                ColoredConsole.WriteLine($"Success Training Version: {versionId}", ConsoleColor.Green);

                var testId = (await _luisClient.CreateTestDataSet(_applicationId, versionId, fold.TestSet.ToArray())).DeserializeObject<string>();

                return await _luisClient.RunDataSetTest(_applicationId, versionId, testId);
            }
            catch (Exception e)
            {
                ColoredConsole.WriteLine(e.Message, ConsoleColor.Red);
                throw;
            }
        }

        #region [ Creating Folds ]

        private List<FoldModel> SeparateFolds(ApplicationVersionModel model, int numberOfFolds)
        {
            var folds = CreateFolds(numberOfFolds);

            var utterancesByIntent = ExtractUtterancesForFolds(model, 0).OrderBy(x => new Guid().ToString());

            var values = utterancesByIntent.Split(numberOfFolds);
            for (var i = 0; i < numberOfFolds; i++)
            {
                var fold = folds[i];
                fold.TestSet.AddRange(values.ElementAt(i).ToList());

                folds.Where(x => x != fold).ToList().ForEach(x => x.TrainingSet.AddRange(values.ElementAt(i)));
            }
            return folds;
        }

        private IEnumerable<ApplicationVersionUtteranceModel> ExtractUtterancesForFolds(ApplicationVersionModel model, int ignoreLessThan)
        {
            var utterancesByIntent = new List<ApplicationVersionUtteranceModel>(model.Utterances).GroupBy(x => x.Intent);
            utterancesByIntent.Where(x => x.Count() < ignoreLessThan).ToList().ForEach(x => Console.WriteLine($"Ignored Intent #{x.Key}: {x.Count()} Values"));
            utterancesByIntent = utterancesByIntent.Where(x => x.Count() > ignoreLessThan);
            return utterancesByIntent.SelectMany(x => x);
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
            var response = await _luisClient.ImportVersion(_applicationId, versionName, foldVersion);
            return response;
        }

        private async Task WaitForTraining(string versionId)
        {
            var isTrained = false;
            while (!isTrained)
            {
                isTrained = await this.IsVersionTrained(versionId);
                if (isTrained)
                    break;

                Console.WriteLine($"Version {versionId} is Training. Sleeping for 10 seconds", ConsoleColor.Magenta);
                Thread.Sleep(10000);
            }
        }

        private async Task<bool> IsVersionTrained(string versionId)
        {
            var result = await _luisClient.GetVersionTrainingStatus(_applicationId, versionId);
            var resultObject = JsonConvert.DeserializeObject<TrainingStatusModel[]>(result);
            return resultObject.All(x => x.Details.StatusId == 0 || x.Details.StatusId == 1);
        }

        private async Task<string> CreateApplication(string name, string language)
        {
            var response = await this._luisClient.CreateApplication(new
            {
                name = $"Experiment-{name}-{Guid.NewGuid().ToString().Substring(0, 5)}",
                description = $"Experiment-{name}-{new Guid().ToString()}",
                culture = language,
                usageScenario = "",
                domain = "",
                initialVersionId = "1.0"
            });
            return response.DeserializeObject<string>();
        }
    }


}
