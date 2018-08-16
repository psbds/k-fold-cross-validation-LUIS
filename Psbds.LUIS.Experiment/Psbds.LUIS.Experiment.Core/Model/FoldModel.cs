using System;
using System.Collections.Generic;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Model
{
    [Serializable]
    public class FoldModel
    {

        public List<ApplicationVersionUtteranceModel> TrainingSet { get; set; } = new List<ApplicationVersionUtteranceModel>();

        public List<ApplicationVersionUtteranceModel> TestSet { get; set; } = new List<ApplicationVersionUtteranceModel>();

    }
}
