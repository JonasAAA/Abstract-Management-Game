namespace Game1.Resources
{
    [Serializable]
    public sealed class BasicRes : IResource
    {
        public BasicResInd resInd;
        public readonly Mass mass;
        public readonly ulong area;
        public readonly Color color;

        private readonly ResAmounts basicIngredients;

        public BasicRes(BasicResInd resInd, Mass mass, ulong area, Color color)
        {
            this.resInd = resInd;
            if (mass.isZero)
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

        Mass IResource.Mass
            => mass;

        ResAmounts IResource.BasicIngredients
            => basicIngredients;
    }
}
