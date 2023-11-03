namespace Game1
{
    public static class ColorHelpers
    {
        public static Color Interpolate(Propor normalized, Color start, Color stop)
            => Color.Lerp(start, stop, (float)normalized);
    }
}
