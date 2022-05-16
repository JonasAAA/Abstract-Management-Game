namespace Game1.Resources
{
    [Serializable]
    public class BasicRes : IResource
    {
        public BasicResInd resInd;
        public readonly ulong mass, area;
        public readonly Color color;

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
        }

        ResInd IResource.ResInd
            => resInd;

        ulong IResource.Mass
            => mass;
    }
}
