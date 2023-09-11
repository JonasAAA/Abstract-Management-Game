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
                expected: TestHelpers.GetAllPublicStaticFieldValuesInType<IMaterialPurpose>(type: typeof(IMaterialPurpose)),
                actual: 
                IMaterialPurpose.all.ToList()
            );

        [TestMethod]
        public void AllMaterialPurposesDistinct()
            => CollectionAssert.AllItemsAreUnique(IMaterialPurpose.all.ToList());
    }
}
