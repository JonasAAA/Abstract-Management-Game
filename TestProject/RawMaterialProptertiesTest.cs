using Game1;
using Game1.PrimitiveTypeWrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class RawMaterialProptertiesTest
    {
        [TestMethod]
        public void AllRawMatAreasAreTheSame()
            // This test is vacuous for now, this is here if the calculation is changed in the future to tell future me (by e.g. not compiling)
            // that I relied on all raw mat areas being the same
            => Assert.AreEqual(expected: ResAndIndustryAlgos.rawMaterialArea, actual: ResAndIndustryAlgos.rawMaterialArea);

        [TestMethod]
        public void MaxDensityIsOne()
        {
            Assert.IsTrue
            (
                Math.Abs
                (
                    Enumerable.Range(start: 0, count: (int)ResAndIndustryAlgos.maxRawMatInd + 1).Max
                    (
                        ind => RawMatDensity((uint)ind)
                    ) - 1
                ) < 0.000001
            );

            static double RawMatDensity(uint ind)
                => (double)ResAndIndustryAlgos.RawMaterialMass(ind: ind).valueInKg / ResAndIndustryAlgos.rawMaterialArea.valueInMetSq;
        }

        [TestMethod]
        public void RawMatDensityDecreases()
        {
            for (uint ind = 0; ind < ResAndIndustryAlgos.maxRawMatInd; ind++)
                Assert.IsTrue
                (
                    RawMatDensity(ind: ind) > RawMatDensity(ind: ind + 1)
                );

            static double RawMatDensity(uint ind)
                => (double)ResAndIndustryAlgos.RawMaterialMass(ind: ind).valueInKg / ResAndIndustryAlgos.rawMaterialArea.valueInMetSq;
        }

        [TestMethod]
        public void FusionReactionProducesEnergy()
        {
            for (uint ind = 0; ind < ResAndIndustryAlgos.maxRawMatInd; ind++)
                Assert.IsTrue(ResAndIndustryAlgos.RawMaterialMass(ind: ind) > ResAndIndustryAlgos.RawMaterialMass(ind: ind + 1));
        }

        [TestMethod]
        public void FusionGeneratesLessEnergyFromLaterRawMats()
        {
            for (uint ind = 0; ind < ResAndIndustryAlgos.maxRawMatInd - 1; ind++)
                Assert.IsTrue(FusionEnergyFromRawMat(ind: ind) > FusionEnergyFromRawMat(ind: ind + 1));

            static double FusionEnergyFromRawMat(uint ind)
            {
                double curMatMass = ResAndIndustryAlgos.RawMaterialMass(ind: ind).valueInKg,
                    nextMatMass = ResAndIndustryAlgos.RawMaterialMass(ind: ind + 1).valueInKg;
                return (curMatMass - nextMatMass) * ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(ind: ind);
            }
        }

        [TestMethod]
        public void AllRawMatsHaveSmallMass()
        {
            for (uint ind = 0; ind <= ResAndIndustryAlgos.maxRawMatInd; ind++)
                Assert.IsTrue(ResAndIndustryAlgos.RawMaterialMass(ind: ind).valueInKg <= 60);
        }

        [TestMethod]
        public void LatestRawMatCannotFuse()
            => Assert.AreEqual
            (
                expected: (UDouble)0,
                actual: ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(ind: ResAndIndustryAlgos.maxRawMatInd)
            );

        [TestMethod]
        public void RawMatResistivityMinThenMidThenMax()
        {
            for (uint ind = 0; ind <= ResAndIndustryAlgos.maxRawMatInd; ind++)
            {
                Assert.IsTrue(ResAndIndustryAlgos.RawMatResistivityMin(ind: ind).resistivity <= ResAndIndustryAlgos.RawMatResistivityMid(ind: ind));
                Assert.IsTrue(ResAndIndustryAlgos.RawMatResistivityMid(ind: ind) <= ResAndIndustryAlgos.RawMatResistivityMax(ind: ind).resistivity);
            }
        }

        [TestMethod]
        public void RawMatMinMidMaxBetweenZeroAndOne()
        {
            for (uint ind = 0; ind <= ResAndIndustryAlgos.maxRawMatInd; ind++)
            {
                Assert.IsTrue((double)ResAndIndustryAlgos.RawMatResistivityMin(ind: ind).resistivity is >= 0 and <= 1);
                Assert.IsTrue((double)ResAndIndustryAlgos.RawMatResistivityMid(ind: ind) is >= 0 and <= 1);
                Assert.IsTrue((double)ResAndIndustryAlgos.RawMatResistivityMax(ind: ind).resistivity is >= 0 and <= 1);
            }
        }
    }
}
