using Psbds.LUIS.Experiment.Core.Model;

namespace Psbds.LUIS.Experiment.Core
{
    internal class List
    {
        private ApplicationVersionUtteranceModel[] utterances;

        public List(ApplicationVersionUtteranceModel[] utterances)
        {
            this.utterances = utterances;
        }
    }
}