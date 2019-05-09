using Microsoft.VisualStudio.TestTools.UnitTesting;
using Psbds.LUIS.Experiment.Core;
using Psbds.LUIS.Experiment.Core.Exceptions;
using Psbds.LUIS.Experiment.Core.Model;
using Psbds.LUIS.Experiment.Core.Model.LuisApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LUIS.Experiment.UnitTests.Core.Fold
{
    [TestClass]
    public class FoldFactoryClusterUnitTests
    {
        private Utterance[] CreateUtteranceArray()
        {
            var utterances = new List<Utterance>();

            for (var i = 0; i < 100; i++)
            {
                var utterance = new Utterance()
                {
                    Intent = $"intent-{i}",
                    Text = $"text-{i}",
                    Entities = new UtteranceEntity[] {
                        new UtteranceEntity() { StartPos = 0, EndPos = 1, Entity = $"entity-{i}"
                        }
                    }
                };
                utterances.Add(utterance);
            }
            return utterances.ToArray();
        }

        [TestMethod]
        public void Should_Separate_Array_Into_The_Given_Number_of_Folds()
        {
            const int NUMBER_OF_FOLDS = 5;

            var utterances = CreateUtteranceArray();

            var foldFactory = new FoldFactoryCluster();
            var foldModel = foldFactory.SeparateFolds(utterances.ToArray(), NUMBER_OF_FOLDS);

            Assert.AreEqual(foldModel.Count, NUMBER_OF_FOLDS);
        }

        [TestMethod]
        [ExpectedException(typeof(NotEnoughUtterancesException))]
        public void Should_Throw_Error_If_Given_Number_Of_Folds_Is_Less_Than_Number_Of_Utterances()
        {
            var utterances = CreateUtteranceArray();
            var numberOfFolds = utterances.Length + 1;

            var foldFactory = new FoldFactoryCluster();
            foldFactory.SeparateFolds(utterances.ToArray(), numberOfFolds);
        }

        [TestMethod]
        public void Should_Separate_Array_Into_The_Given_Number_of_Folds_And_Sets_Should_Contain_All_Examples()
        {
            const int NUMBER_OF_FOLDS = 5;

            var utterances = CreateUtteranceArray();

            var foldFactory = new FoldFactoryCluster();
            var foldModel = foldFactory.SeparateFolds(utterances.ToArray(), NUMBER_OF_FOLDS);

            foreach (var fold in foldModel)
            {
                Assert.AreEqual(fold.TestSet.Count, fold.TestSet.Select(x => x.Text).Distinct().Count(), "Examples in TestSet should not repeat");

                Assert.AreEqual(fold.TrainingSet.Count, fold.TrainingSet.Select(x => x.Text).Distinct().Count(), "Examples in TrainingSet should not repeat");

                Assert.IsTrue(utterances.All(x => fold.TestSet.Any(utest => utest.Text == x.Text) || fold.TrainingSet.Any(utrain => utrain.Text == x.Text)), "Utterances Must be Either in TestSet or TrainingSet");
            }
        }


        [TestMethod]
        public void Should_Separate_Array_Into_The_Given_Number_of_Folds_Randomized()
        {
            const int NUMBER_OF_FOLDS = 5;

            var utterances = CreateUtteranceArray();

            var foldFactory = new FoldFactoryCluster();
            var foldModel1 = foldFactory.SeparateFolds(utterances.ToArray(), NUMBER_OF_FOLDS);
            var foldModel2 = foldFactory.SeparateFolds(utterances.ToArray(), NUMBER_OF_FOLDS);

            var testSetKeys = new List<string>();
            var traningSetKeys = new List<string>();


            foreach (var fold in foldModel1.Union(foldModel2))
            {
                testSetKeys.Add(String.Join(',', fold.TestSet.OrderBy(x => x.Text).Select(x => x.Text)));
                traningSetKeys.Add(String.Join(',', fold.TrainingSet.OrderBy(x => x.Text).Select(x => x.Text)));
            }

            Assert.AreEqual(testSetKeys.Distinct().Count(), NUMBER_OF_FOLDS * 2, "Test Sets should be completely random for each fold.");
            Assert.AreEqual(traningSetKeys.Distinct().Count(), NUMBER_OF_FOLDS * 2, "Training Sets should be completely random for each fold.");

        }
    }
}