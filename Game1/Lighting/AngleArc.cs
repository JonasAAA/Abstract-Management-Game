namespace Game1.Lighting
{
    /// <summary>
    /// Arc is in clockwise direction
    /// </summary>
    [Serializable]
    public readonly struct AngleArc : IComparable<AngleArc>
    {
        [Serializable]
        public readonly struct Params
        {
            public readonly double startAngle, endAngle;
            public readonly Length radius;

            public Params(double startAngle, double endAngle, Length radius)
            {
                this.startAngle = MyMathHelper.WrapAngle(angle: startAngle);
                this.endAngle = MyMathHelper.WrapAngle(angle: endAngle);
                this.radius = radius;
            }
        }

        public readonly double startAngle, endAngle;
        public readonly Length radius;
        public readonly ILightCatchingObject lightCatchingObject;

        public AngleArc(Params parameters, ILightCatchingObject lightCatchingObject)
            : this(startAngle: parameters.startAngle, endAngle: parameters.endAngle, radius: parameters.radius, lightCatchingObject: lightCatchingObject)
        { }

        public AngleArc(double startAngle, double endAngle, Length radius, ILightCatchingObject lightCatchingObject)
        {
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.radius = radius;
            this.lightCatchingObject = lightCatchingObject;
        }

        public double GetAngle(bool start)
            => start ? startAngle : endAngle;

        int IComparable<AngleArc>.CompareTo(AngleArc other)
            => radius.CompareTo(other.radius);
    }
}
