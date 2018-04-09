using Psbds.LUIS.Experiment.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Psbds.LUIS.Experiment.Core
{
    public class Experiment
    {
        public Experiment()
        {

        }

        public List<ApplicationFoldUtterancesModel> SeparateFolds(ApplicationVersionModel model, int numberOfFolds)
        {
            var groups = new List<ApplicationFoldUtterancesModel>(Enumerable.Repeat(new ApplicationFoldUtterancesModel(), numberOfFolds));

            var utterancesByIntent = new List<ApplicationVersionUtteranceModel>(model.Utterances).GroupBy(x => x.Intent);

            utterancesByIntent.Where(x => x.Count() < 5).ToList().ForEach(x =>
            {
                Console.WriteLine($"Ignored Intent #{x.Key}: {x.Count()} Values");
            });
            utterancesByIntent = utterancesByIntent.Where(x => x.Count() >= 5);
            foreach (var fold in groups)
            {
                foreach (var utteranceGroup in utterancesByIntent)
                {
                    if (utteranceGroup.Count() < 5)
                    {
                        continue;
                    }
                    var values = utteranceGroup.Split(5).ToArray();
                    fold.TrainingSet.AddRange(values[0].ToList());
                    fold.TrainingSet.AddRange(values[1].ToList());
                    fold.TrainingSet.AddRange(values[2].ToList());
                    fold.TrainingSet.AddRange(values[3].ToList());
                    fold.TestSet.AddRange(values[4].ToList());
                }
            }

            return groups;

        }

        public class ApplicationFoldUtterancesModel
        {

            public List<ApplicationVersionUtteranceModel> TrainingSet { get; set; } = new List<ApplicationVersionUtteranceModel>();

            public List<ApplicationVersionUtteranceModel> TestSet { get; set; } = new List<ApplicationVersionUtteranceModel>();

        }


        public static IEnumerable<List<T>> splitList<T>(List<T> locations, int nSize = 30)
        {
            for (int i = 0; i < locations.Count; i += nSize)
            {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }
    }

    static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }
    }
}
