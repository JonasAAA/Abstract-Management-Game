namespace Game1.Resources
{
    [Serializable]
    public class Resource
    {
        public readonly ResInd resInd;
        public readonly ulong mass, area;

        public Resource(ResInd resInd, ulong mass, ulong area)
        {
            this.resInd = resInd;
            if (mass is 0)
                throw new ArgumentOutOfRangeException();
            this.mass = mass;
            this.area = area;
        }
    }
}
