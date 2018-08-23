using Psbds.LUIS.Experiment.Core.Helpers;
using Psbds.LUIS.Experiment.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Core
{
    public abstract class FoldFactory
    {

        public abstract List<FoldModel> SeparateFolds(ApplicationVersionUtteranceModel[] modelUtterances, int numberOfFolds);

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
