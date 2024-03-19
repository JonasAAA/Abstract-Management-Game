using Game1.MyMath;
using Game1.PrimitiveTypeWrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject
{
    [TestClass]
    public class MyMathHelperTest
    {
        [TestMethod]
        public void AngleProporTestEdgeCase()
            => Assert.AreEqual
            (
                expected: Propor.empty,
                actual: MyMathHelper.AngleProporOfFull
                (
                    startAngle: 0,
                    endAngle: 2 * MyMathHelper.pi
                )
            );

        [TestMethod]
        public void AngleProporSimpleTest()
            => Assert.IsTrue
            (
                MyMathHelper.AngleProporOfFull
                (
                    startAngle: MyMathHelper.pi / 2,
                    endAngle: MyMathHelper.pi
                ).IsCloseTo((Propor)0.25)
            );

        [TestMethod]
        public void AngleProporWrappingTest()
            => Assert.IsTrue
            (
                MyMathHelper.AngleProporOfFull
                (
                    startAngle: MyMathHelper.pi,
                    endAngle: MyMathHelper.pi / 2
                ).IsCloseTo((Propor)0.75)
            );
    }
}
