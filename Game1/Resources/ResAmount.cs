namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResAmount<TRes>
        where TRes : class
    {
        public readonly TRes res;
        public readonly ulong amount;

        public ResAmount(TRes res, ulong amount)
        {
            this.res = res;
            this.amount = amount;
        }
    }
}
