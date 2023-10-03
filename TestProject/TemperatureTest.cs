using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game1.Resources;
using Game1;

namespace TestProject
{
    [TestClass]
    public class TemperatureTest
    {
        [TestMethod]
        public void TemperatureToAndFromHeatEnergyAreConsistent()
        {
            var heatEnergy = HeatEnergy.CreateFromJoules(valueInJ: 3561);
            var heatCapacity = HeatCapacity.CreateFromJPerK(valueInJPerK: 214);
            Assert.AreEqual
            (
                expected: heatEnergy,
                actual: ResAndIndustryAlgos.HeatEnergyFromTemperature
                (
                    temperature: ResAndIndustryAlgos.CalculateTemperature(heatEnergy: heatEnergy, heatCapacity: heatCapacity),
                    heatCapacity: heatCapacity
                )
            );
        }
    }
}
