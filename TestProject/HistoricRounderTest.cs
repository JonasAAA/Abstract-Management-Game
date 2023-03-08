using Game1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject
{
    [TestClass]
    public class HistoricRounderTest
    {
        [TestMethod]
        public void WholeNumbersNoRounding()
        {
            HistoricRounder roundWithHistory = new();
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10, TimeSpan.FromSeconds(1)));
            Assert.AreEqual(expected: 5u, actual: roundWithHistory.Round(value: 5, TimeSpan.FromSeconds(2)));
            Assert.AreEqual(expected: 132u, actual: roundWithHistory.Round(value: 132, TimeSpan.FromSeconds(3)));
        }

        [TestMethod]
        public void AccurateHistoricalRounding()
        {
            HistoricRounder roundWithHistory = new();
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m, TimeSpan.FromSeconds(1)));
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m, TimeSpan.FromSeconds(2)));
            Assert.AreEqual(expected: 11u, actual: roundWithHistory.Round(value: 10.25m, TimeSpan.FromSeconds(3)));
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m, TimeSpan.FromSeconds(4)));
        }

        [TestMethod]
        public void PausedTimeNoNewRounding()
        {
            HistoricRounder roundWithHistory = new();
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m, TimeSpan.FromSeconds(1)));
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m, TimeSpan.FromSeconds(1)));
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m, TimeSpan.FromSeconds(1)));
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m, TimeSpan.FromSeconds(1)));
        }

        [TestMethod]
        public void PausedTimeDifferentValueThrowError()
        {
            HistoricRounder roundWithHistory = new();
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10, TimeSpan.FromSeconds(1)));
            Assert.ThrowsException<ArgumentException>(() => roundWithHistory.Round(value: 5, TimeSpan.FromSeconds(1)));
        }
    }
}
