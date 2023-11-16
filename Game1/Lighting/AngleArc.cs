namespace Game1.Lighting
{
    /// <summary>
    /// Arc is in clockwise direction
    /// </summary>
    [Serializable]
    public readonly struct AngleArc(double startAngle, double endAngle, Length radius, ILightCatchingObject lightCatchingObject) : IComparable<AngleArc>
    {
        [Serializable]
        public readonly struct Params(double startAngle, double endAngle, Length radius)
        {
            public readonly double
                startAngle = MyMathHelper.WrapAngle(angle: startAngle),
                endAngle = MyMathHelper.WrapAngle(angle: endAngle);
            public readonly Length radius = radius;
        }

        public readonly double startAngle = startAngle, endAngle = endAngle;
        public readonly Length radius = radius;
        public readonly ILightCatchingObject lightCatchingObject = lightCatchingObject;

        public AngleArc(Params parameters, ILightCatchingObject lightCatchingObject)
            : this(startAngle: parameters.startAngle, endAngle: parameters.endAngle, radius: parameters.radius, lightCatchingObject: lightCatchingObject)
        { }

        public double GetAngle(bool start)
            => start ? startAngle : endAngle;

        int IComparable<AngleArc>.CompareTo(AngleArc other)
            => radius.CompareTo(other.radius);
    }
}
