namespace Game1.Resources
{
    [Serializable]
    public class BasicRes : IResource
    {
        public BasicResInd resInd;
        public readonly ulong mass, area;

        public BasicRes(BasicResInd resInd, ulong mass, ulong area)
        {
            this.resInd = resInd;
            if (mass is 0)
                throw new ArgumentOutOfRangeException();
            this.mass = mass;
            if (area is 0)
                throw new ArgumentOutOfRangeException();
            this.area = area;
        }

        ResInd IResource.ResInd
            => resInd;

        ulong IResource.Mass
            => mass;
    }
}
