using Game1;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class RawMaterialProptertiesTest
    {
        private const uint maxRelevantRawMatInd = 20;

        [TestMethod]
        public void FusionLeavesSameArea()
        {
            for (uint ind = 0; ind < maxRelevantRawMatInd; ind++)
                Assert.AreEqual
                (
                    expected: ResAndIndustryAlgos.RawMaterialArea(ind: ind + 1),
                    actual: 2 * ResAndIndustryAlgos.RawMaterialArea(ind: ind)
                );
        }

        [TestMethod]
        public void MaxDensityIsOne()
        {
            Assert.IsTrue
            (
                Math.Abs
                (
                    Enumerable.Range(start: 0, count: (int)maxRelevantRawMatInd).Max
                    (
                        ind => RawMatDensity((uint)ind)
                    ) - 1
                ) < 0.000001
            );

            static double RawMatDensity(uint ind)
                => (double)ResAndIndustryAlgos.RawMaterialMass(ind: ind).valueInKg / ResAndIndustryAlgos.RawMaterialArea(ind: ind).valueInMetSq;
        }

        [TestMethod]
        public void RawMatDensityDecreases()
        {
            for (uint ind = 0; ind < maxRelevantRawMatInd; ind++)
                Assert.IsTrue
                (
                    RawMatDensity(ind: ind) > RawMatDensity(ind: ind + 1)
                );

            static double RawMatDensity(uint ind)
                => (double)ResAndIndustryAlgos.RawMaterialMass(ind: ind).valueInKg / ResAndIndustryAlgos.RawMaterialArea(ind: ind).valueInMetSq;
        }

        [TestMethod]
        public void FusionReactionProducesEnergy()
        {
            for (uint ind = 0; ind < maxRelevantRawMatInd; ind++)
                Assert.IsTrue(2 * ResAndIndustryAlgos.RawMaterialMass(ind: ind) > ResAndIndustryAlgos.RawMaterialMass(ind: ind + 1));
        }

        [TestMethod]
        public void ProporOfMassTransformedToEnergyByFusionDecreases()
        {
            for (uint ind = 0; ind < maxRelevantRawMatInd; ind++)
            {
                Console.WriteLine($"{ind} {ProporOfMassTransformedToEnergy(ind: ind)} {ProporOfMassTransformedToEnergy(ind: ind + 1)}");
                Assert.IsTrue(ProporOfMassTransformedToEnergy(ind: ind) > ProporOfMassTransformedToEnergy(ind: ind + 1));
            }

            static double ProporOfMassTransformedToEnergy(uint ind)
            {
                double curMatMass = ResAndIndustryAlgos.RawMaterialMass(ind: ind).valueInKg,
                    nextMatMass = ResAndIndustryAlgos.RawMaterialMass(ind: ind + 1).valueInKg;
                return (2 * curMatMass - nextMatMass) / (2 * curMatMass);
            }
        }

        [TestMethod]
        public void FirstRawMatHasSmallMass()
        {
            Assert.IsTrue(ResAndIndustryAlgos.RawMaterialMass(ind: 0).valueInKg <= 10);
        }
    }
}
