using static Game1.WorldManager;

namespace Game1
{
    public static class WorldFunctions
    {
        public static TimeSpan LinkTravelTime(UDouble linkLength)
            => TimeSpan.FromSeconds(linkLength / CurWorldConfig.linkTravelSpeed);

        public static UDouble LinkJoulesPerKg(UDouble surfaceGravity1, UDouble surfaceGravity2, UDouble linkLength)
            => (surfaceGravity1 + surfaceGravity2) * CurWorldConfig.linkJoulesPerNewtonOfGravity + linkLength * CurWorldConfig.linkJoulesPerMeterOfDistance;

        public static UDouble SurfaceGravity(Mass mass, UDouble radius)
            => CurWorldConfig.gravitConst * mass.valueInKg / MyMathHelper.Pow<UDouble, double>(@base: radius, exponent: CurWorldConfig.gravitExponent);
    }
}
