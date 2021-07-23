using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game1;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Splitter splitter = new(proportions: new double[] { 4, 1 });
            int[] amounts = splitter.Split(amount: 1);
            amounts = amounts.Zip(splitter.Split(amount: 1), (a, b) => a + b).ToArray();
            amounts = amounts.Zip(splitter.Split(amount: 1), (a, b) => a + b).ToArray();
            amounts = amounts.Zip(splitter.Split(amount: 1), (a, b) => a + b).ToArray();
            amounts = amounts.Zip(splitter.Split(amount: 1), (a, b) => a + b).ToArray();
            CollectionAssert.AreEqual(expected: new int[] { 4, 1 }, actual: amounts);
        }
    }
}
