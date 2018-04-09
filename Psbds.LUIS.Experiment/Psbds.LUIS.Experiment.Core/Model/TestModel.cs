using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Psbds.LUIS.Experiment.Core.Model
{
    public class TestModel
    {
        public int FoldIndex { get; set; }

        public string VersionId { get; set; }

        public Task ImportingTask { get; set; }

        public Task CreatingDataSetTask { get; set; }

        public string TestId { get; set; }
    }
}
