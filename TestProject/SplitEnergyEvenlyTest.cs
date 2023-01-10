using Game1;
using Game1.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace TestProject
{
    [TestClass]
    public class SplitEnergyEvenlyTest
    {
        [TestMethod]
        public void TooMuchAvailableEnergyThrowsError()
        {
            Assert.ThrowsException<ArgumentException>
            (
                () => Algorithms.SplitEnergyEvenly<ElectricalEnergy>
                (
                    reqEnergies: new()
                    {
                        ElectricalEnergy.CreateFromJoules(valueInJ: 10),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9)
                    },
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 20)
                )
            );
        }

        [TestMethod]
        public void WhenPossibleSplitsExactlyEvenly()
        {
            CollectionAssert.AreEqual
            (
                expected: new List<ElectricalEnergy>()
                {
                    ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                    ElectricalEnergy.CreateFromJoules(valueInJ: 8),
                    ElectricalEnergy.CreateFromJoules(valueInJ: 6)
                },
                actual: Algorithms.SplitEnergyEvenly<ElectricalEnergy>
                (
                    reqEnergies: new()
                    {
                        ElectricalEnergy.CreateFromJoules(valueInJ: 6),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 12),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9)
                    },
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 18)
                )
            );
        }

        [TestMethod]
        public void MaximizesMinimumAllocatedRatio()
        {
            CollectionAssert.AreEqual
            (
                expected: new List<ElectricalEnergy>()
                {
                    ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                    ElectricalEnergy.CreateFromJoules(valueInJ: 9),
                    ElectricalEnergy.CreateFromJoules(valueInJ: 5)
                },
                actual: Algorithms.SplitEnergyEvenly<ElectricalEnergy>
                (
                    reqEnergies: new()
                    {
                        ElectricalEnergy.CreateFromJoules(valueInJ: 6),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 13),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 8)
                    },
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 18)
                )
            );
        }
    }
}
