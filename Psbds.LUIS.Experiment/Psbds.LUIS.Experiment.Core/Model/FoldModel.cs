using Psbds.LUIS.Experiment.Core.Model.LuisApplication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Model
{
    [Serializable]
    public class FoldModel
    {

        public List<Utterance> TrainingSet { get; set; } = new List<Utterance>();

        public List<Utterance> TestSet { get; set; } = new List<Utterance>();

    }
}
