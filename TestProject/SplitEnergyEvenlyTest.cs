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
        private const string splitEnergy = "energy", splitExtraEnergy = "extra energy";

        [TestMethod, TestCategory(splitEnergy)]
        public void TooMuchAvailableEnergyFillsAndHasUnused()
            => AssertEquality
            (
                expected:
                (
                    allocatedEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 10),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9)
                    ],
                    unusedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 1)
                ),
                actual: Algorithms.SplitEnergyEvenly
                (
                    reqEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 10),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9)
                    ],
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 20)
                )
            );

        [TestMethod, TestCategory(splitExtraEnergy)]
        public void ExtraTooMuchAvailableEnergyFillsAndHasUnused()
            => AssertEquality
            (
                expected:
                (
                    allocatedEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 8)
                    ],
                    unusedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 1)
                ),
                actual: Algorithms.SplitExtraEnergyEvenly
                (
                    energies:
                    [
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 1), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 10)),
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 1), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 9))
                    ],
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 18)
                )
            );

        [TestMethod, TestCategory(splitEnergy)]
        public void WhenPossibleSplitsExactlyEvenly()
            => AssertEquality
            (
                expected:
                (
                    allocatedEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 8),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 6)
                    ],
                    unusedEnergy: ElectricalEnergy.zero
                ),
                actual: Algorithms.SplitEnergyEvenly
                (
                    reqEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 6),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 12),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9)
                    ],
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 18)
                )
            );

        [TestMethod, TestCategory(splitEnergy)]
        public void MaximizesMinimumAllocatedRatio()
            => AssertEquality
            (
                expected:
                (
                    allocatedEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 5)
                    ],
                    unusedEnergy: ElectricalEnergy.zero
                ),
                actual: Algorithms.SplitEnergyEvenly
                (
                    reqEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 6),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 13),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 8)
                    ],
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 18)
                )
            );

        [TestMethod, TestCategory(splitExtraEnergy)]
        public void ExtraMaximizesMinimumAllocatedRatio()
            => AssertEquality
            (
                expected:
                (
                    allocatedEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 0),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 7),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4)
                    ],
                    unusedEnergy: ElectricalEnergy.zero
                ),
                actual: Algorithms.SplitExtraEnergyEvenly
                (
                    energies:
                    [
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 10), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 12)),
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 0), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 6)),
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 2), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 13)),
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 1), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 8))
                    ],
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 15)
                )
            );

        [TestMethod, TestCategory(splitEnergy)]
        public void DealWithZeroReqEnergy()
            => AssertEquality
            (
                expected:
                (
                    allocatedEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 0),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 9),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 5)
                    ],
                    unusedEnergy: ElectricalEnergy.zero
                ),
                actual: Algorithms.SplitEnergyEvenly
                (
                    reqEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 0),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 6),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 13),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 8)
                    ],
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 18)
                )
            );

        [TestMethod, TestCategory(splitExtraEnergy)]
        public void ExtraDealWithZeroReqEnergy()
            => AssertEquality
            (
                expected:
                (
                    allocatedEnergies:
                    [
                        ElectricalEnergy.CreateFromJoules(valueInJ: 0),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 7),
                        ElectricalEnergy.CreateFromJoules(valueInJ: 4)
                    ],
                    unusedEnergy: ElectricalEnergy.zero
                ),
                actual: Algorithms.SplitExtraEnergyEvenly
                (
                    energies:
                    [
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 0), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 0)),
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 0), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 6)),
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 2), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 13)),
                        (ownedEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 1), reqEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 8))
                    ],
                    availableEnergy: ElectricalEnergy.CreateFromJoules(valueInJ: 15)
                )
            );

        private static void AssertEquality<T>((List<T> allocatedEnergies, T unusedEnergy) expected, (List<T> allocatedEnergies, T unusedEnergy) actual)
        {
            Console.WriteLine($"Expected allocation: {string.Join(", ", expected.allocatedEnergies)}");
            Console.WriteLine($"Actual allocation:   {string.Join(", ", actual.allocatedEnergies)}");
            CollectionAssert.AreEqual(expected: expected.allocatedEnergies, actual: actual.allocatedEnergies);
            Assert.AreEqual(expected: expected.unusedEnergy, actual: actual.unusedEnergy);
        }
    }
}
