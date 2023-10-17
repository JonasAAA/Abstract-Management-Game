using static Game1.WorldManager;

namespace Game1
{
    public static class WorldFunctions
    {
        public static TimeSpan LinkTravelTime(Length linkLength)
            => TimeSpan.FromSeconds(linkLength.valueInM / CurWorldConfig.linkTravelSpeed);

        public static UDouble LinkJoulesPerKg(SurfaceGravity surfaceGravity1, SurfaceGravity surfaceGravity2, Length linkLength)
            => (surfaceGravity1.valueInMetPerSeqSq + surfaceGravity2.valueInMetPerSeqSq) * CurWorldConfig.linkJoulesPerUnitGravitAccel + linkLength.valueInM * CurWorldConfig.linkJoulesPerMeterOfDistance;

        /// <summary>
        /// I.e. gravitational acceleration, see https://en.wikipedia.org/wiki/Surface_gravity
        /// </summary>
        public static SurfaceGravity SurfaceGravity(Mass mass, AreaInt resArea)
            // gravitExponent is divided by 2 as sqrt(resArea) is the width (up to a constant factor)
            => PrimitiveTypeWrappers.SurfaceGravity.CreateFromMetPerSeqSq
            (
                valueInMetPerSeqSq: CurWorldConfig.gravitConst * mass.valueInKg / MyMathHelper.Pow<UDouble, double>(@base: resArea.valueInMetSq, exponent: CurWorldConfig.gravitExponent / 2)
            );
    }
}
