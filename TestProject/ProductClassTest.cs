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
            var allProductClasses = TestHelpers.GetAllPublicStaticFieldValuesInType<IProductClass>(type: typeof(IProductClass));

            CollectionAssert.AreEquivalent
            (
                expected: allProductClasses.Select(arg => arg.value).ToList(),
                actual: IProductClass.all.ToList()
            );

            CollectionAssert.AreEquivalent
            (
                expected: allProductClasses.Select(arg => arg.name).ToList(),
                actual: typeof(IProductClass)
                    .GetMethod(name: nameof(IProductClass.SwitchExpression))
                    !.GetParameters()
                    .Select(paramInfo => paramInfo.Name)
                    .ToList()
            );
        }

        [TestMethod]
        public void AllProductClassesDistinct()
            => CollectionAssert.AllItemsAreUnique(IProductClass.all.ToList());

        [TestMethod]
        public void ProductClassMaterialPurposesPartition()
        {
            var productClasses = IProductClass.all;
            var mentionedMaterialPurposes = productClasses.SelectMany(productClass => productClass.MatPurposeToAmount.Keys).ToList();
            CollectionAssert.AllItemsAreUnique(mentionedMaterialPurposes);
            CollectionAssert.AreEquivalent
            (
                expected: IMaterialPurpose.all.ToList(),
                actual: mentionedMaterialPurposes
            );
        }

        [TestMethod]
        public void MaterialPurposesGetNonZeroAreas()
        {
            var productClasses = IProductClass.all;
            foreach (var productClass in productClasses)
            {
                Assert.IsTrue(productClass.MatPurposeToAmount.Count > 0);
                Assert.IsTrue(productClass.MatPurposeToAmount.Values.All(value => value > 0));
            }
        }
    }
}
