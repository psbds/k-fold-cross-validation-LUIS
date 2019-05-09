using Psbds.LUIS.Experiment.Core.Model;
using Psbds.LUIS.Experiment.Core.Model.LuisApplication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Fold
{
    public interface IFoldFactory
    {

        List<FoldModel> SeparateFolds(Utterance[] modelUtterances, int numberOfFolds);

    }
}
