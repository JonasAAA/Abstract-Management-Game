namespace Game1
{
    [Serializable]
    public class Resource
    {
        public readonly int id;
        public readonly ulong weight;

        public Resource(int id, ulong weight)
        {
            this.id = id;
            if (weight is 0)
                throw new ArgumentOutOfRangeException();
            this.weight = weight;
        }
    }
}
