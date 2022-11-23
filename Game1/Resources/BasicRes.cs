namespace Game1.Resources
{
    [Serializable]
    public sealed class BasicRes : IResource
    {
        public BasicResInd resInd;
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public readonly ulong area;
        public readonly Color color;

        private readonly ResAmounts basicIngredients;

        public BasicRes(BasicResInd resInd, Mass mass, HeatCapacity heatCapacity, ulong area, Color color)
        {
            this.resInd = resInd;
            if (mass.IsZero)
                throw new ArgumentOutOfRangeException();
            Mass = mass;
            if (heatCapacity.IsZero)
                throw new ArgumentOutOfRangeException();
            HeatCapacity = heatCapacity;
            if (area is 0)
                throw new ArgumentOutOfRangeException();
            this.area = area;
            if (color.A != byte.MaxValue)
                throw new ArgumentException();
            this.color = color;

            basicIngredients = new()
            {
                [resInd] = 1
            };
        }

        ResInd IResource.ResInd
            => resInd;

        ResAmounts IResource.BasicIngredients
            => basicIngredients;
    }
}
