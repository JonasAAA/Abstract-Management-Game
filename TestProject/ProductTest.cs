using Game1.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class ProductTest
    {
        [TestMethod]
        public void ProductParamsNamesAreUnique()
            => CollectionAssert.AllItemsAreUnique
            (
                Product.productParamsDict.Keys.ToList()
            );

        [TestMethod]
        public void ProductClassesContainAtLeastOneProduct()
            => CollectionAssert.AreEquivalent
            (
                expected: ProductClass.all.ToList(),
                actual: Product.productParamsDict.Values.Select(prodParams => prodParams.productClass).ToHashSet().ToList()
            );
    }
}
