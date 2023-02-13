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
        public void TooMuchAvailableEnergyFillsAndHasUnused()
        {
            AssertEquality
            (
                expected:
                (
                    allocatedEnergies: new()
                    {
                        ElectricalEnergy.CreateFromJoules(valueInJ: 10),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9)
                    },
                    unusedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 1)
                ),
                actual: Algorithms.SplitEnergyEvenly
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
            AssertEquality
            (
                expected:
                (
                    allocatedEnergies: new()
                    {
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 8),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 6)
                    },
                    unusedEnergy: ElectricalEnergy.zero
                ),
                actual: Algorithms.SplitEnergyEvenly
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
            AssertEquality
            (
                expected:
                (
                    allocatedEnergies: new()
                    {
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 5)
                    },
                    unusedEnergy: ElectricalEnergy.zero
                ),
                actual: Algorithms.SplitEnergyEvenly
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

        private static void AssertEquality<T>((List<T> allocatedEnergies, T unusedEnergy) expected, (List<T> allocatedEnergies, T unusedEnergy) actual)
        {
            Console.WriteLine($"{string.Join(", ", expected.allocatedEnergies)}");
            Console.WriteLine($"{string.Join(", ", actual.allocatedEnergies)}");
            CollectionAssert.AreEqual(expected: expected.allocatedEnergies, actual: actual.allocatedEnergies);
            Assert.AreEqual(expected: expected.unusedEnergy, actual: actual.unusedEnergy);
        }
    }
}
