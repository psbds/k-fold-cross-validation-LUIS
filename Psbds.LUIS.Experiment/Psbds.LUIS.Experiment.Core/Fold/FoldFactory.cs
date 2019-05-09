using Psbds.LUIS.Experiment.Core.Fold;
using Psbds.LUIS.Experiment.Core.Helpers;
using Psbds.LUIS.Experiment.Core.Model;
using Psbds.LUIS.Experiment.Core.Model.LuisApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Core
{
    public abstract class FoldFactory : IFoldFactory
    {
        public abstract List<FoldModel> SeparateFolds(Utterance[] modelUtterances, int numberOfFolds);

        protected List<FoldModel> CreateFolds(int numberOfFolds)
        {
            var list = new List<FoldModel>();
            for (var i = 0; i < numberOfFolds; i++)
            {
                list.Add(new FoldModel());
            }
            return list;
        }
    }
}
