using Game1;
using Game1.PrimitiveTypeWrappers;
using Game1.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject
{
    [TestClass]
    public class HeatEnergyToDissipateTest
    {
        [TestMethod]
        public void ZeroHeatCapacityNonZeroHeatEnergyThrowsError()
            => Assert.ThrowsException<ArgumentException>
            (
                () => Algorithms.HeatEnergyToDissipate
                (
                    heatEnergy: HeatEnergy.CreateFromJoules(valueInJ: 10),
                    heatCapacity: HeatCapacity.CreateFromJPerK(valueInJPerK: 0),
                    surfaceLength: 100,
                    emissivity: (Propor)1,
                    stefanBoltzmannConstant: 1,
                    temperatureExponent: 4
                )
            );

        [TestMethod]
        public void WhenPossibleDissipatesExactNeededEnergy()
            => Assert.AreEqual<HeatEnergy>
            (
                expected: HeatEnergy.CreateFromJoules(valueInJ: 840),
                // 
                actual: Algorithms.HeatEnergyToDissipate
                (
                    heatEnergy: HeatEnergy.CreateFromJoules(valueInJ: 844),
                    heatCapacity: HeatCapacity.CreateFromJPerK(valueInJPerK: 2),
                    surfaceLength: (UDouble).3,
                    emissivity: (Propor).5,
                    stefanBoltzmannConstant: 700,
                    temperatureExponent: 3
                )
            );

        [TestMethod]
        public void ChooseLessInacurateDissipation()
            => Assert.AreEqual<HeatEnergy>
            (
                expected: HeatEnergy.CreateFromJoules(valueInJ: 2),
                actual: Algorithms.HeatEnergyToDissipate
                (
                    heatEnergy: HeatEnergy.CreateFromJoules(valueInJ: 5),
                    heatCapacity: HeatCapacity.CreateFromJPerK(valueInJPerK: 2),
                    surfaceLength: 1,
                    emissivity: (Propor)1,
                    stefanBoltzmannConstant: 1,
                    temperatureExponent: 2
                )
            );
    }
}
