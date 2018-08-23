using Psbds.LUIS.Experiment.Core.Helpers;
using Psbds.LUIS.Experiment.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Core
{
    public class FoldFactoryStratified : FoldFactory
    {
        public override List<FoldModel> SeparateFolds(ApplicationVersionUtteranceModel[] modelUtterances, int numberOfFolds)
        {
            var folds = CreateFolds(numberOfFolds);

            var utterancesByIntent = GroupAndShuffle(modelUtterances, numberOfFolds);

            foreach (var intent in utterancesByIntent)
            {
                var utterances = intent.Split(numberOfFolds);
                for (var i = 0; i < numberOfFolds; i++)
                {
                    var fold = folds[i];
                    fold.TestSet.AddRange(utterances.ElementAt(i).ToList());
                    folds.Where(x => x != fold).ToList().ForEach(x => x.TrainingSet.AddRange(utterances.ElementAt(i)));
                }
            }
            folds.SelectMany(x => x.TrainingSet.GroupBy(y => y.Intent));
            return folds;
        }

        private IEnumerable<IGrouping<string, ApplicationVersionUtteranceModel>> GroupAndShuffle(ApplicationVersionUtteranceModel[] modelUtterances, int ignoreLessThan)
        {
            var utterancesByIntent = new List<ApplicationVersionUtteranceModel>(modelUtterances)
                .GroupBy(x => x.Intent)
                .Where(x => x.Count() >= ignoreLessThan)
                .OrderBy(x => new Guid().ToString());

            return utterancesByIntent;
        }
    }
}
