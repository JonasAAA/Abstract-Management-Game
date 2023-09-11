//using Game1;
//using Game1.Resources;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Generic;
//using System.Linq;

//namespace TestProject
//{
//    [TestClass]
//    public class ProductClassTest
//    {
//        [TestMethod]
//        public void AllProductClassesPresent()
//            => CollectionAssert.AreEquivalent
//            (
//                expected: TestHelpers.GetAllPublicStaticFieldValuesInType<IProductClass>(type: typeof(IProductClass)),
//                actual: IProductClass.all.ToList()
//            );

//        [TestMethod]
//        public void AllProductClassesDistinct()
//            => CollectionAssert.AllItemsAreUnique(IProductClass.all.ToList());

//        [TestMethod]
//        public void ProductClassMaterialPurposesPartition()
//        {
//            var productClasses = IProductClass.all;
//            var mentionedMaterialPurposes = productClasses.SelectMany(productClass => productClass.MatPurposeToMultipleOfMatTargetAreaDivisor.Keys).ToList();
//            CollectionAssert.AllItemsAreUnique(mentionedMaterialPurposes);
//            CollectionAssert.AreEquivalent
//            (
//                expected: IMaterialPurpose.all.ToList(),
//                actual: mentionedMaterialPurposes
//            );
//        }

//        [TestMethod]
//        public void MaterialPurposesGetNonZeroAreas()
//        {
//            var productClasses = IProductClass.all;
//            foreach (var productClass in productClasses)
//            {
//                Assert.IsTrue(productClass.MatPurposeToMultipleOfMatTargetAreaDivisor.Count > 0);
//                Assert.IsTrue(productClass.MatPurposeToMultipleOfMatTargetAreaDivisor.Values.All(value => value > 0));
//            }
//        }
//    }
//}
