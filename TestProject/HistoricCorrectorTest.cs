using Game1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TestProject
{
    [TestClass]
    public class HistoricCorrectorTest
    {
        private static IEnumerable<object[]> TestHistoricCorrectorInput
        {
            get
            {
                Random random = new(Seed: 125);
                return new object[][]
                {
                    [(Func<int, int>)(x => x - 1), new int[] { 1, -5, 3, 7, 2 }],
                    [(Func<int, int>)(x => random.Next() % 100), new int[] { 1, -5, 3, 7, 2 }],
                };
            }
        }

        [DynamicData(nameof(TestHistoricCorrectorInput))]
        [DataTestMethod]
        public void TestHistoricCorrector(Func<int, int> func, int[] targets)
        {
            // The following bit should hold for any func and any targets array.
            HistoricCorrector<int> historicalCorrector = new();
            int targetsSum = 0, resultsSum = 0;
            foreach (var target in targets)
            {
                var historicCorrectorWithTarget = historicalCorrector.WithTarget(target: target);
                var suggestion = historicCorrectorWithTarget.suggestion;
                var result = func(suggestion);
                historicalCorrector = historicCorrectorWithTarget.WithValue(value: result);

                targetsSum += target;
                resultsSum += result;

                // Basically I want to get a suggestion such that the difference between targets sum up till now
                // and results sum up till now are as close as possible. Since the last funcition call could have done
                // anything, the following is the theoretically best result.
                // So this is to test that HistoricCorrector is best possible
                Assert.AreEqual(targetsSum - resultsSum, suggestion - result);
            }
        }

        // Can't test the following straight-up as per https://stackoverflow.com/questions/60288748/is-it-possible-to-make-a-test-method-generic-while-using-mstest-as-test-framewor
        //public void TestHistoricCorrector<TValue>(Func<TValue, TValue> func, TValue[] targets)
        //    where TValue : IAdditiveIdentity<TValue, TValue>, IAdditionOperators<TValue, TValue, TValue>, ISubtractionOperators<TValue, TValue, TValue>
        //{
        //    // The following bit should hold for any func and any targets array.
        //    HistoricCorrector<TValue> historicalCorrector = new();
        //    TValue targetsSum = TValue.AdditiveIdentity, resultsSum = TValue.AdditiveIdentity;
        //    foreach (var target in targets)
        //    {
        //        var historicCorrectorWithTarget = historicalCorrector.WithTarget(target: target);
        //        var suggestion = historicCorrectorWithTarget.suggestion;
        //        var result = func(suggestion);
        //        historicalCorrector = historicCorrectorWithTarget.WithValue(value: result);

        //        targetsSum += target;
        //        resultsSum += result;

        //        // Basically I want to get a suggestion such that the difference between targets sum up till now
        //        // and results sum up till now are as close as possible. Since the last funcition call could have done
        //        // anything, the following is the theoretically best result.
        //        // So this is to test that HistoricCorrector is best possible
        //        Assert.AreEqual(targetsSum - resultsSum, suggestion - result);
        //    }
        //}
    }
}
