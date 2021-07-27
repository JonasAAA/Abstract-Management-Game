using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game1;
using System;
using System.Collections.ObjectModel;

namespace TestProject
{
    [TestClass]
    public class ProportionalSplittingTest
    {
        [TestMethod]
        public void Negative_proportion_is_invalid()
        {
            // arrange
            ProporSplitter splitter = new();
            ReadOnlyCollection<double> proportions = new(new double[] { -1, 2 });

            // act and assert
            Assert.ThrowsException<ArgumentException>(() => splitter.Proportions = proportions);
        }

        [TestMethod]
        public void Negative_split_amount_is_invalid()
        {
            // arrange
            ProporSplitter splitter = new();
            splitter.Proportions = new(new double[] { 1, 1 });
            int splitAmount = -1;

            // act and assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => splitter.CanSplit(amount: splitAmount));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => splitter.Split(amount: splitAmount));
        }

        [TestMethod]
        public void Cannot_split_by_zero_proportion()
        {
            // arrange
            ProporSplitter splitter = new();
            splitter.Proportions = new(new double[] { 0, 0 });
            int splitAmount = 1;

            // act and assert
            Assert.IsFalse(splitter.CanSplit(amount: splitAmount));
            Assert.ThrowsException<Exception>(() => splitter.Split(amount: splitAmount));
        }

        [TestMethod]
        public void Cannot_split_by_empty_proportion()
        {
            // arrange
            ProporSplitter splitter = new();
            int splitAmount = 1;

            // act and assert
            Assert.IsFalse(splitter.CanSplit(amount: splitAmount));
            Assert.ThrowsException<Exception>(() => splitter.Split(amount: splitAmount));
        }

        [DataTestMethod]
        [DataRow(new double[] { }, -1)]
        [DataRow(new double[] { 1 }, 2)]
        public void Cannot_insert_in_bad_index(double[] proportions, int index)
        {
            // arrange
            ProporSplitter splitter = new();
            splitter.Proportions = new(proportions);

            // act and assert
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => splitter.InsertVar(index: index));
        }

        [DataTestMethod]
        [DataRow(new double[] { 2, 1 }, 3, new int[] { 2, 1 })]
        [DataRow(new double[] { .51, .49 }, 2, new int[] { 1, 1 })]
        [DataRow(new double[] { .51, .49 }, 3, new int[] { 2, 1 })]
        public void First_splits_proportionally(double[] proportions, int amountToSplit, int[] expectedSplit)
        {
            // arrange
            ProporSplitter splitter = new();
            splitter.Proportions = new(proportions);

            // act and assert
            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit));
            CollectionAssert.AreEqual
            (
                expected: expectedSplit,
                actual: splitter.Split(amount: amountToSplit)
            );
        }

        [DataTestMethod]
        [DataRow(new double[] { .51, .49 }, 1, 1, new int[] { 1, 0 }, new int[] { 0, 1 })]
        public void Splits_sequence_proportionally(double[] proportions, int amountToSplit1, int amountToSplit2, int[] expectedSplit1, int[] expectedSplit2)
        {
            // arrange
            ProporSplitter splitter = new();
            splitter.Proportions = new(proportions);

            // act and assert
            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit1));
            CollectionAssert.AreEqual
            (
                expected: expectedSplit1,
                actual: splitter.Split(amount: amountToSplit1)
            );

            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit2));
            CollectionAssert.AreEqual
            (
                expected: expectedSplit2,
                actual: splitter.Split(amount: amountToSplit2)
            );
        }

        [DataTestMethod]
        [DataRow(new double[] { .51, .49 }, 1, 0, 1, new int[] { 1, 0 }, new int[] { 0, 0, 1 })]
        [DataRow(new double[] { .51, .49 }, 1, 2, 1, new int[] { 1, 0 }, new int[] { 0, 1, 0 })]
        public void After_adding_new_variable_splits_proportionally(double[] proportions, int amountToSplit1, int insertInd, int amountToSplit2, int[] expectedSplit1, int[] expectedSplit2)
        {
            // arrange
            ProporSplitter splitter = new();
            splitter.Proportions = new(proportions);

            // act and assert
            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit1));
            CollectionAssert.AreEqual
            (
                expected: expectedSplit1,
                actual: splitter.Split(amount: amountToSplit1)
            );

            splitter.InsertVar(index: insertInd);
            
            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit2));
            CollectionAssert.AreEqual
            (
                expected: expectedSplit2,
                actual: splitter.Split(amount: amountToSplit2)
            );
        }

        [DataTestMethod]
        [DataRow(new double[] { .51, .49 }, new double[] { .51, .49 }, 1, 1, new int[] { 1, 0 }, new int[] { 1, 0 })]
        public void Reseting_proportion_forgets_previuos_innacuracies(double[] proportions1, double[] proportions2, int amountToSplit1, int amountToSplit2, int[] expectedSplit1, int[] expectedSplit2)
        {
            // arrange
            ProporSplitter splitter = new();
            splitter.Proportions = new(proportions1);

            // act and assert
            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit1));
            CollectionAssert.AreEqual
            (
                expected: expectedSplit1,
                actual: splitter.Split(amount: amountToSplit1)
            );

            splitter.Proportions = new(proportions2);
            
            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit2));
            CollectionAssert.AreEqual
            (
                expected: expectedSplit2,
                actual: splitter.Split(amount: amountToSplit2)
            );
        }
    }
}
