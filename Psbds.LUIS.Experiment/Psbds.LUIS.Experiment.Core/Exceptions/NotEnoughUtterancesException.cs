using System;
using System.Collections.Generic;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Exceptions
{
    public class NotEnoughUtterancesException : Exception
    {
        public NotEnoughUtterancesException(string message) : base(message) { }
    }
}
