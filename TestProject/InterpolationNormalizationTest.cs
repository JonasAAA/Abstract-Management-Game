using Game1;
using Game1.MyMath;
using Game1.PrimitiveTypeWrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject
{
    [TestClass]
    public sealed class InterpolationNormalizationTest
    {
        [TestMethod]
        [DataRow(0.2, 1, 5)]
        [DataRow(0.9, -10, 100)]
        public void InterpolateIsInverseOfNormalize(double normalized, double min, double max)
            => Assert.IsTrue
            (
                MyMathHelper.AreClose
                (
                    (Propor)normalized,
                    Algorithms.Normalize
                    (
                        value: Algorithms.Interpolate
                        (
                            normalized: (Propor)normalized,
                            start: min,
                            stop: max
                        ),
                        start: min,
                        stop: max
                    )
                )
            );

        [TestMethod]
        [DataRow(0)]
        [DataRow(0.2563)]
        [DataRow(0.125)]
        [DataRow(0.685)]
        public void NormalizationAndInterpolationDoNothingWhenMinZeroMaxOne(double value)
        {
            Assert.IsTrue
            (
                MyMathHelper.AreClose
                (
                    (Propor)value,
                    Algorithms.Normalize(value: value, start: 0, stop: 1)
                )
            );

            Assert.IsTrue
            (
                MyMathHelper.AreClose
                (
                    value,
                    Algorithms.Interpolate(normalized: (Propor)value, start: 0, stop: 1)
                )
            );
        }
    }
}
