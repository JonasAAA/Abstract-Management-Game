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

        public void Deconstruct(out TRes res, out ulong amount)
        {
            res = this.res;
            amount = this.amount;
        }
    }
}
