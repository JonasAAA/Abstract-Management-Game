namespace Game1.Resources
{
    [Serializable]
    public sealed class BasicRes : IResource
    {
        public BasicResInd resInd;
        public readonly ulong mass, area;
        public readonly Color color;

        private readonly ResAmounts basicIngredients;

        public BasicRes(BasicResInd resInd, ulong mass, ulong area, Color color)
        {
            this.resInd = resInd;
            if (mass is 0)
                throw new ArgumentOutOfRangeException();
            this.mass = mass;
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

        ulong IResource.Mass
            => mass;

        ResAmounts IResource.BasicIngredients
            => basicIngredients;
    }
}
