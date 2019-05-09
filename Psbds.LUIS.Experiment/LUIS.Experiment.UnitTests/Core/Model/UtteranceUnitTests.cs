using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Psbds.LUIS.Experiment.Core.Model.LuisApplication;
using System;
using System.Collections.Generic;
using System.Text;

namespace LUIS.Experiment.UnitTests.Model
{
    [TestClass]
    public class UtteranceUnitTests
    {
        [TestMethod]
        public void Should_Deserialize_Object_Correctly()
        {
            var jsonString = @"{
                ""text"": ""fly to cairo"",
                ""intent"": ""BookFlight"",
                ""entities"": [
                    {
                        ""entity"": ""Location::LocationTo"",
                        ""startPos"": 7,
                        ""endPos"": 11
                    }
                ]
            }";
            var utterance = JsonConvert.DeserializeObject<Utterance>(jsonString);

            Assert.AreEqual(utterance.Text, "fly to cairo");
            Assert.AreEqual(utterance.Intent, "BookFlight");
        }
    }
}
