using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Psbds.LUIS.Experiment.Core.Helpers
{
    public static class ObjectExtensions
    {

        public static T DeepClone<T>(this T a)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(a));
        }

    }
}
