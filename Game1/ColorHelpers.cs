namespace Game1
{
    public static class ColorHelpers
    {
        public static Color InterpolateStarColor(Propor normalized, Color start, Color stop)
            => Color.Lerp(start, stop, (float)MyMathHelper.Sqrt((UDouble)normalized));
    }
}
