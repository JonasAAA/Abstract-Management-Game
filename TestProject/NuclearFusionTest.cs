using Game1;
using Game1.PrimitiveTypeWrappers;
using Game1.Resources;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject
{
    [TestClass]
    public class NuclearFusionTest
    {
        [TestMethod]
        public void NonReactingProporForUnitReactionStrengthUnitTimeIsFullNoReactioHappens()
        {
            Assert.AreEqual<(ulong nonReactingAmount, ulong fusionProductAmount)>
            (
                expected: (nonReactingAmount: 1000, fusionProductAmount: 0),
                actual: Algorithms.NuclearFusionSingleRawMat
                (
                    amount: 1000,
                    compositionArea: Area<UDouble>.CreateFromMetSq(351),
                    gravity: 3661,
                    temperature: Temperature.CreateFromK(61),
                    duration: TimeSpan.FromSeconds(1),
                    reactionNumberRounder: reactionNum => (ulong)reactionNum,
                    nonReactingProporForUnitReactionStrengthUnitTime: Propor.full,
                    fusionReactionStrengthCoeff: 10000
                )
            );
        }

        [TestMethod]
        public void NuclearFusionGeneralTest()
        {
            Assert.AreEqual<(ulong nonReactingAmount, ulong fusionProductAmount)>
            (
                expected: (nonReactingAmount: 128, fusionProductAmount: 4936),
                actual: Algorithms.NuclearFusionSingleRawMat
                (
                    amount: 10000,
                    compositionArea: Area<UDouble>.CreateFromMetSq(5415),
                    gravity: 2,
                    temperature: Temperature.CreateFromK(3),
                    duration: TimeSpan.FromSeconds(0.2),
                    reactionNumberRounder: reactionNum => (ulong)reactionNum,
                    nonReactingProporForUnitReactionStrengthUnitTime: (Propor)(1.0 / 7),
                    fusionReactionStrengthCoeff: (UDouble)1 / 11
                )
            );
        }
    }
}
