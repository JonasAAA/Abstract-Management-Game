namespace Game1.Resources
{
    [Serializable]
    public readonly struct ResAmount
    {
        public readonly ResInd resInd;
        public readonly ulong amount;

        public ResAmount(ResInd resInd, ulong amount)
        {
            this.resInd = resInd;
            this.amount = amount;
        }
    }
}
