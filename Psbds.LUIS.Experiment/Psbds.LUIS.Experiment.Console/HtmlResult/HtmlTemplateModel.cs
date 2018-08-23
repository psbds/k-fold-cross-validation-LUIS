using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Psbds.LUIS.Experiment.Console
{
    public class HtmlTemplateModel
    {
        public OverviewTab OverviewTab { get; set; } = new OverviewTab();

        public ErrorTab ErrorTab { get; set; } = new ErrorTab();

    }

    public class OverviewTab
    {
        public List<OverviewTabAccuracy> AccuraciesPerTest { get; set; }

        public double AveragePrecision { get; set; }

        public OverviewTabAccuracy AverageAccuracy
        {
            get
            {
                return new OverviewTabAccuracy
                {
                    AccuracyAt1 = AccuraciesPerTest.Sum(x => x.AccuracyAt1) / AccuraciesPerTest.Count,
                    AccuracyAt2 = AccuraciesPerTest.Sum(x => x.AccuracyAt2) / AccuraciesPerTest.Count,
                    AccuracyAt3 = AccuraciesPerTest.Sum(x => x.AccuracyAt3) / AccuraciesPerTest.Count,
                    AccuracyAt4 = AccuraciesPerTest.Sum(x => x.AccuracyAt4) / AccuraciesPerTest.Count,
                    AccuracyAt5 = AccuraciesPerTest.Sum(x => x.AccuracyAt5) / AccuraciesPerTest.Count
                };
            }
        }

    }

    public class OverviewTabAccuracy
    {

        public double AccuracyAt1 { get; set; }

        public double AccuracyAt2 { get; set; }

        public double AccuracyAt3 { get; set; }

        public double AccuracyAt4 { get; set; }

        public double AccuracyAt5 { get; set; }

        public double[] AsArray
        {
            get
            {
                return new double[]
                {
                    AccuracyAt1, AccuracyAt2,AccuracyAt3, AccuracyAt4, AccuracyAt5
                };
            }
        }
    }

    public class ErrorTab
    {
        public int MaxConfusions { get; set; }

        public IEnumerable<JObject> Errors { get; set; }

        public JObject Intents { get; set; }
    }
}
