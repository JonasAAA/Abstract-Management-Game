using Game1;
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
            Assert.AreEqual(expected: 10, actual: roundWithHistory.Round(value: 10));
            Assert.AreEqual(expected: 5, actual: roundWithHistory.Round(value: 5));
            Assert.AreEqual(expected: 132, actual: roundWithHistory.Round(value: 132));
            Assert.AreEqual(10, 5);
        }

        [TestMethod]
        public void AccurateHistoricalRounding()
        {
            RoundWithHistory roundWithHistory = new();
            Assert.AreEqual(expected: 10, actual: roundWithHistory.Round(value: 10.25M));
            Assert.AreEqual(expected: 10, actual: roundWithHistory.Round(value: 10.25M));
            Assert.AreEqual(expected: 11, actual: roundWithHistory.Round(value: 10.25M));
            Assert.AreEqual(expected: 10, actual: roundWithHistory.Round(value: 10.25M));
        }
    }
}
