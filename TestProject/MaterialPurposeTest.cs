using Game1.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class MaterialPurposeTest
    {
        [TestMethod]
        public void AllMaterialPurposesPresent()
            => CollectionAssert.AreEquivalent
            (
                expected: TestHelpers.GetAllPublicStaticFieldValuesInType<MaterialPurpose>(type: typeof(MaterialPurpose))
                    .Select(arg => arg.value).ToList(),
                actual: MaterialPurpose.all.ToList()
            );

        [TestMethod]
        public void AllMaterialPurposesDistinct()
            => CollectionAssert.AllItemsAreUnique(MaterialPurpose.all.ToList());
    }
}
