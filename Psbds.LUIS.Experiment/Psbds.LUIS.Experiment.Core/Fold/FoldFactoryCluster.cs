using Psbds.LUIS.Experiment.Core.Helpers;
using Psbds.LUIS.Experiment.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Core
{
    public class FoldFactoryCluster : FoldFactory
    {
        public override List<FoldModel> SeparateFolds(ApplicationVersionUtteranceModel[] modelUtterances, int numberOfFolds)
        {
            var folds = CreateFolds(numberOfFolds);

            var utterancesByIntent = ExtractUtterancesForFolds(modelUtterances, 0).OrderBy(x => new Guid().ToString());

            var values = utterancesByIntent.Split(numberOfFolds);
            for (var i = 0; i < numberOfFolds; i++)
            {
                var fold = folds[i];
                fold.TestSet.AddRange(values.ElementAt(i).ToList());

                folds.Where(x => x != fold).ToList().ForEach(x => x.TrainingSet.AddRange(values.ElementAt(i)));
            }
            return folds;
        }

        private IEnumerable<ApplicationVersionUtteranceModel> ExtractUtterancesForFolds(ApplicationVersionUtteranceModel[] modelUtterances, int ignoreLessThan)
        {
            var utterancesByIntent = new List<ApplicationVersionUtteranceModel>(modelUtterances).GroupBy(x => x.Intent);
            utterancesByIntent = utterancesByIntent.Where(x => x.Count() > ignoreLessThan);

            return utterancesByIntent.SelectMany(x => x);
        }
    }
}
