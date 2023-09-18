using static Game1.WorldManager;

namespace Game1
{
    public static class WorldFunctions
    {
        public static TimeSpan LinkTravelTime(UDouble linkLength)
            => TimeSpan.FromSeconds(linkLength / CurWorldConfig.linkTravelSpeed);

        public static UDouble LinkJoulesPerKg(UDouble surfaceGravity1, UDouble surfaceGravity2, UDouble linkLength)
            => (surfaceGravity1 + surfaceGravity2) * CurWorldConfig.linkJoulesPerUnitGravitAccel + linkLength * CurWorldConfig.linkJoulesPerMeterOfDistance;

        /// <summary>
        /// I.e. gravitational acceleration, see https://en.wikipedia.org/wiki/Surface_gravity
        /// </summary>
        public static UDouble SurfaceGravity(Mass mass, AreaInt resArea)
            // gravitExponent is divided by 2 as sqrt(resArea) is the width (up to a constant factor)
            => CurWorldConfig.gravitConst * mass.valueInKg / MyMathHelper.Pow<UDouble, double>(@base: resArea.valueInMetSq, exponent: CurWorldConfig.gravitExponent / 2);
    }
}
