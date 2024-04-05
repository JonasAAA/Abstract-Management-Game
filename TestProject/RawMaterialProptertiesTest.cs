using Game1;
using Game1.GlobalTypes;
using Game1.PrimitiveTypeWrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
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
                    Enum.GetValues<RawMaterialID>().Max
                    (
                        rawMatID => RawMatDensity(rawMatID)
                    ) - 1
                ) < 0.000001
            );

            static double RawMatDensity(RawMaterialID rawMatID)
                => (double)ResAndIndustryAlgos.RawMaterialMass(rawMatID: rawMatID).valueInKg / ResAndIndustryAlgos.rawMaterialArea.valueInMetSq;
        }

        [TestMethod]
        public void RawMatDensityDecreases()
        {
            foreach (var rawMatID in Enum.GetValues<RawMaterialID>())
                if (rawMatID.Next() is RawMaterialID nextRawMatID)
                    Debug.Assert(RawMatDensity(rawMatID: rawMatID) > RawMatDensity(rawMatID: nextRawMatID));

            static double RawMatDensity(RawMaterialID rawMatID)
                => (double)ResAndIndustryAlgos.RawMaterialMass(rawMatID: rawMatID).valueInKg / ResAndIndustryAlgos.rawMaterialArea.valueInMetSq;
        }

        [TestMethod]
        public void FusionReactionProducesEnergy()
        {
            foreach (var rawMatID in Enum.GetValues<RawMaterialID>())
                if (rawMatID.Next() is RawMaterialID nextRawMatID)
                    Assert.IsTrue(ResAndIndustryAlgos.RawMaterialMass(rawMatID: rawMatID) > ResAndIndustryAlgos.RawMaterialMass(rawMatID: nextRawMatID));
        }

        [TestMethod]
        public void FusionGeneratesLessEnergyFromLaterRawMats()
        {
            foreach (var rawMatID in Enum.GetValues<RawMaterialID>())
                if (rawMatID.Next() is RawMaterialID nextRawMatID && nextRawMatID.Next() is RawMaterialID nextNextRawMatID)
                    Assert.IsTrue(FusionEnergyFromRawMat(rawMatID, nextRawMatID) > FusionEnergyFromRawMat(nextRawMatID, nextNextRawMatID));

            static double FusionEnergyFromRawMat(RawMaterialID rawMatID, RawMaterialID nextRawMatID)
            {
                double curMatMass = ResAndIndustryAlgos.RawMaterialMass(rawMatID: rawMatID).valueInKg,
                    nextMatMass = ResAndIndustryAlgos.RawMaterialMass(rawMatID: nextRawMatID).valueInKg;
                return (curMatMass - nextMatMass) * ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(rawMatID: rawMatID);
            }
        }

        [TestMethod]
        public void AllRawMatsHaveSmallMass()
        {
            foreach (var rawMatID in Enum.GetValues<RawMaterialID>())
                Assert.IsTrue(ResAndIndustryAlgos.RawMaterialMass(rawMatID: rawMatID).valueInKg <= 60);
        }

        [TestMethod]
        public void LatestRawMatCannotFuse()
            => Assert.AreEqual
            (
                expected: (UDouble)0,
                actual: ResAndIndustryAlgos.RawMaterialFusionReactionStrengthCoeff(rawMatID: RawMaterialIDUtil.lastRawMatID)
            );

        [TestMethod]
        public void RawMatResistivityMinThenMidThenMax()
        {
            foreach (var rawMatID in Enum.GetValues<RawMaterialID>())
            {
                Assert.IsTrue(ResAndIndustryAlgos.RawMatResistivityMin(rawMatID: rawMatID).resistivity <= ResAndIndustryAlgos.RawMatResistivityMid(rawMatID: rawMatID));
                Assert.IsTrue(ResAndIndustryAlgos.RawMatResistivityMid(rawMatID: rawMatID) <= ResAndIndustryAlgos.RawMatResistivityMax(rawMatID: rawMatID).resistivity);
            }
        }

        [TestMethod]
        public void RawMatMinMidMaxBetweenZeroAndOne()
        {
            foreach (var rawMatID in Enum.GetValues<RawMaterialID>())
            {
                Assert.IsTrue((double)ResAndIndustryAlgos.RawMatResistivityMin(rawMatID: rawMatID).resistivity is >= 0 and <= 1);
                Assert.IsTrue((double)ResAndIndustryAlgos.RawMatResistivityMid(rawMatID: rawMatID) is >= 0 and <= 1);
                Assert.IsTrue((double)ResAndIndustryAlgos.RawMatResistivityMax(rawMatID: rawMatID).resistivity is >= 0 and <= 1);
            }
        }
    }
}
