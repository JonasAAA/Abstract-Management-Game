using Game1;
using Game1.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class ProductClassTest
    {
        [TestMethod]
        public void AllProductClassesPresent()
        {
            var allProductClasses = TestHelpers.GetAllPublicStaticFieldValuesInType<ProductClass>(type: typeof(ProductClass));

            CollectionAssert.AreEquivalent
            (
                expected: allProductClasses.Select(arg => arg.value).ToList(),
                actual: ProductClass.all.ToList()
            );

            CollectionAssert.AreEquivalent
            (
                expected: allProductClasses.Select(arg => arg.name).ToList(),
                actual: typeof(ProductClass)
                    .GetMethod(name: nameof(ProductClass.SwitchExpression))
                    !.GetParameters()
                    .Select(paramInfo => paramInfo.Name)
                    .ToList()
            );
        }

        [TestMethod]
        public void AllProductClassesDistinct()
            => CollectionAssert.AllItemsAreUnique(ProductClass.all.ToList());

        [TestMethod]
        public void ProductClassMaterialPurposesPartition()
        {
            var productClasses = ProductClass.all;
            var mentionedMaterialPurposes = productClasses.SelectMany(productClass => productClass.matPurposeToAmount.Keys).ToList();
            CollectionAssert.AllItemsAreUnique(mentionedMaterialPurposes);
            CollectionAssert.AreEquivalent
            (
                expected: MaterialPurpose.all.ToList(),
                actual: mentionedMaterialPurposes
            );
        }

        [TestMethod]
        public void MaterialPurposesGetNonZeroAreas()
        {
            var productClasses = ProductClass.all;
            foreach (var productClass in productClasses)
            {
                Assert.IsTrue(productClass.matPurposeToAmount.Count > 0);
                Assert.IsTrue(productClass.matPurposeToAmount.Values.All(value => value > 0));
            }
        }
    }
}
