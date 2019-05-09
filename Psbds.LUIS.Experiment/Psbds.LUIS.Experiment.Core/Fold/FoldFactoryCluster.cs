using Psbds.LUIS.Experiment.Core.Exceptions;
using Psbds.LUIS.Experiment.Core.Helpers;
using Psbds.LUIS.Experiment.Core.Model;
using Psbds.LUIS.Experiment.Core.Model.LuisApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Core
{
    public class FoldFactoryCluster : FoldFactory
    {
        public override List<FoldModel> SeparateFolds(Utterance[] utterances, int numberOfFolds)
        {
            if (utterances.Length < numberOfFolds)
            {
                throw new NotEnoughUtterancesException("Number of Folds is Higher than the number of utterances");
            }

            var folds = CreateFolds(numberOfFolds);

            Random rnd = new Random();

            var randomizedUtterances = utterances.ToList().OrderBy(x => rnd.Next()).ToArray();

            var values = randomizedUtterances.Split(numberOfFolds);
            for (var i = 0; i < numberOfFolds; i++)
            {
                var fold = folds[i];
                fold.TestSet.AddRange(values.ElementAt(i).ToList());

                folds.Where(x => x != fold).ToList().ForEach(x => x.TrainingSet.AddRange(values.ElementAt(i)));
            }
            return folds;
        }
    }
}
