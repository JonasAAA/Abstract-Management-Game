﻿using Game1;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject
{
    [TestClass]
    public class RoundWithHistoryTest
    {
        [TestMethod]
        public void WholeNumbersNoRounding()
        {
            RoundWithHistory roundWithHistory = new();
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10));
            Assert.AreEqual(expected: 5u, actual: roundWithHistory.Round(value: 5));
            Assert.AreEqual(expected: 132u, actual: roundWithHistory.Round(value: 132));
        }

        [TestMethod]
        public void AccurateHistoricalRounding()
        {
            RoundWithHistory roundWithHistory = new();
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m));
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m));
            Assert.AreEqual(expected: 11u, actual: roundWithHistory.Round(value: 10.25m));
            Assert.AreEqual(expected: 10u, actual: roundWithHistory.Round(value: 10.25m));
        }
    }
}
