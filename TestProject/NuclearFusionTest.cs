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
        public void NuclearFusionGeneralTest()
            => Assert.AreEqual<(ulong nonReactingAmount, ulong fusionProductAmount)>
            (
                expected: (nonReactingAmount: 965, fusionProductAmount: 35),
                actual: Algorithms.NuclearFusionSingleRawMat
                (
                    amount: 1000,
                    compositionArea: Area<UDouble>.CreateFromMetSq(5415),
                    surfaceGravity: SurfaceGravity.CreateFromMetPerSecSq(2),
                    surfaceGravityExponent: 2,
                    temperature: Temperature.CreateFromK(3),
                    temperatureExponent: 2,
                    duration: TimeSpan.FromSeconds(0.2),
                    reactionNumberRounder: reactionNum => (ulong)reactionNum,
                    fusionReactionStrengthCoeff: (UDouble)1 / 7
                )
            );
    }
}
