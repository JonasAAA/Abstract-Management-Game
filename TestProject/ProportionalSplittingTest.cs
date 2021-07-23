using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game1;
using System;

namespace TestProject
{
    [TestClass]
    public class ProportionalSplittingTest
    {
        [TestMethod]
        public void Negative_proportion_is_invalid()
        {
            // arrange
            double[] proportions = new double[] { -1, 2 };

            // act and assert
            Assert.ThrowsException<ArgumentException>(() => new ProporSplitter(proportions: proportions));
        }

        [TestMethod]
        public void All_zero_proportion_is_invalid()
        {
            // arrange
            double[] proportions = new double[] { 0, 0, 0 };

            // act and assert
            Assert.ThrowsException<ArgumentException>(() => new ProporSplitter(proportions: proportions));
        }

        [TestMethod]
        public void Negative_amount_to_split_is_invalid()
        {
            // arrange
            ProporSplitter splitter = new
            (
                proportions: new double[] { 2, 1 }
            );
            int amountToSplit = -1;

            // act and assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => splitter.Split(amount: amountToSplit));
        }

        [DataTestMethod]
        [DataRow(new double[] { 2, 1 }, 3, new int[] { 2, 1 })]
        [DataRow(new double[] { .51, .49 }, 2, new int[] { 1, 1 })]
        [DataRow(new double[] { .51, .49 }, 3, new int[] { 2, 1 })]
        public void First_splits_proportionally(double[] proportions, int amountToSplit, int[] expectedSplit)
        {
            // arrange
            ProporSplitter splitter = new(proportions: proportions);

            // act
            int[] actualSplit = splitter.Split(amount: amountToSplit);

            // assert
            CollectionAssert.AreEqual(expected: expectedSplit, actual: actualSplit);
        }

        [DataTestMethod]
        [DataRow(new double[] { .51, .49 }, 1, 1, new int[] { 1, 0 }, new int[] { 0, 1 })]
        public void Splits_sequence_proportionally(double[] proportions, int amountToSplit1, int amountToSplit2, int[] expectedSplit1, int[] expectedSplit2)
        {
            // arrange
            ProporSplitter splitter = new(proportions: proportions);

            // act
            int[] actualSplit1 = splitter.Split(amount: amountToSplit1);
            int[] actualSplit2 = splitter.Split(amount: amountToSplit2);

            // assert
            CollectionAssert.AreEqual(expected: expectedSplit1, actual: actualSplit1);
            CollectionAssert.AreEqual(expected: expectedSplit2, actual: actualSplit2);
        }
    }
}
