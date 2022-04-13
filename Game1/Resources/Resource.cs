namespace Game1.Resources
{
    [Serializable]
    public class Resource
    {
        public readonly ResInd resInd;
        public readonly ulong weight;

        public Resource(ResInd resInd, ulong weight)
        {
            this.resInd = resInd;
            if (weight is 0)
                throw new ArgumentOutOfRangeException();
            this.weight = weight;
        }
    }
}
