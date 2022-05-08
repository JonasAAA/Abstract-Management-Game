//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Game1;
//using System;
//using System.Collections.ObjectModel;

//namespace TestProject
//{
//    [TestClass]
//    public class ProportionalSplittingTest
//    {
//        [TestMethod]
//        public void Negative_proportion_is_invalid()
//        {
//            // arrange
//            ProporSplitter<int> splitter = new();
//            ReadOnlyCollection<double> proportions = new(new double[] { -1, 2 });

//            // act and assert
//            Assert.ThrowsException<ArgumentException>(() => splitter.Proportions = proportions);
//        }

//        [TestMethod]
//        public void Cannot_split_by_zero_proportion()
//        {
//            // arrange
//            ProporSplitter splitter = new();
//            splitter.Proportions = new(new double[] { 0, 0 });
//            uint splitAmount = 1;

//            // act and assert
//            Assert.IsFalse(splitter.CanSplit(amount: splitAmount));
//            Assert.ThrowsException<Exception>(() => splitter.Split(amount: splitAmount));
//        }

//        [TestMethod]
//        public void Cannot_split_by_empty_proportion()
//        {
//            // arrange
//            ProporSplitter splitter = new();
//            uint splitAmount = 1;

//            // act and assert
//            Assert.IsFalse(splitter.CanSplit(amount: splitAmount));
//            Assert.ThrowsException<Exception>(() => splitter.Split(amount: splitAmount));
//        }

//        [DataTestMethod]
//        [DataRow(new double[] { }, -1)]
//        [DataRow(new double[] { 1 }, 2)]
//        public void Cannot_insert_in_bad_index(double[] proportions, int index)
//        {
//            // arrange
//            ProporSplitter splitter = new();
//            splitter.Proportions = new(proportions);

//            // act and assert
//            Assert.ThrowsException<ArgumentOutOfRangeException>(() => splitter.InsertVar(index: index));
//        }

//        [DataTestMethod]
//        [DataRow(new double[] { 2, 1 }, (uint)3, new uint[] { 2, 1 })]
//        [DataRow(new double[] { .51, .49 }, (uint)2, new uint[] { 1, 1 })]
//        [DataRow(new double[] { .51, .49 }, (uint)3, new uint[] { 2, 1 })]
//        public void First_splits_proportionally(double[] proportions, uint amountToSplit, uint[] expectedSplit)
//        {
//            // arrange
//            ProporSplitter splitter = new();
//            splitter.Proportions = new(proportions);

//            // act and assert
//            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit));
//            CollectionAssert.AreEqual
//            (
//                expected: expectedSplit,
//                actual: splitter.Split(amount: amountToSplit)
//            );
//        }

//        [DataTestMethod]
//        [DataRow(new double[] { .51, .49 }, (uint)1, (uint)1, new uint[] { 1, 0 }, new uint[] { 0, 1 })]
//        public void Splits_sequence_proportionally(double[] proportions, uint amountToSplit1, uint amountToSplit2, uint[] expectedSplit1, uint[] expectedSplit2)
//        {
//            // arrange
//            ProporSplitter splitter = new();
//            splitter.Proportions = new(proportions);

//            // act and assert
//            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit1));
//            CollectionAssert.AreEqual
//            (
//                expected: expectedSplit1,
//                actual: splitter.Split(amount: amountToSplit1)
//            );

//            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit2));
//            CollectionAssert.AreEqual
//            (
//                expected: expectedSplit2,
//                actual: splitter.Split(amount: amountToSplit2)
//            );
//        }

//        [DataTestMethod]
//        [DataRow(new double[] { .51, .49 }, (uint)1, 0, (uint)1, new uint[] { 1, 0 }, new uint[] { 0, 0, 1 })]
//        [DataRow(new double[] { .51, .49 }, (uint)1, 2, (uint)1, new uint[] { 1, 0 }, new uint[] { 0, 1, 0 })]
//        public void After_adding_new_variable_splits_proportionally(double[] proportions, uint amountToSplit1, int insertInd, uint amountToSplit2, uint[] expectedSplit1, uint[] expectedSplit2)
//        {
//            // arrange
//            ProporSplitter splitter = new();
//            splitter.Proportions = new(proportions);

//            // act and assert
//            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit1));
//            CollectionAssert.AreEqual
//            (
//                expected: expectedSplit1,
//                actual: splitter.Split(amount: amountToSplit1)
//            );

//            splitter.InsertVar(index: insertInd);
            
//            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit2));
//            CollectionAssert.AreEqual
//            (
//                expected: expectedSplit2,
//                actual: splitter.Split(amount: amountToSplit2)
//            );
//        }

//        [DataTestMethod]
//        [DataRow(new double[] { .51, .49 }, new double[] { .51, .49 }, (uint)1, (uint)1, new uint[] { 1, 0 }, new uint[] { 1, 0 })]
//        public void Reseting_proportion_forgets_previuos_innacuracies(double[] proportions1, double[] proportions2, uint amountToSplit1, uint amountToSplit2, uint[] expectedSplit1, uint[] expectedSplit2)
//        {
//            // arrange
//            ProporSplitter splitter = new();
//            splitter.Proportions = new(proportions1);

//            // act and assert
//            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit1));
//            CollectionAssert.AreEqual
//            (
//                expected: expectedSplit1,
//                actual: splitter.Split(amount: amountToSplit1)
//            );

//            splitter.Proportions = new(proportions2);
            
//            Assert.IsTrue(splitter.CanSplit(amount: amountToSplit2));
//            CollectionAssert.AreEqual
//            (
//                expected: expectedSplit2,
//                actual: splitter.Split(amount: amountToSplit2)
//            );
//        }
//    }
//}
