namespace Game1.Industries
{
    [Serializable]
    public class Building
    {
        public ResAmounts Cost
            => resPile.ResAmounts;

        private readonly ResPile resPile;
        private bool isDeleted;

        public Building(ResPile resSource, ResAmounts cost)
        {
            resPile = ResPile.CreateEmpty();
            if (cost.IsEmpty())
                throw new ArgumentException();
            if (resSource.ResAmounts != cost)
                throw new ArgumentException();
            ResPile.TransferAll(source: resSource, destin: resPile);
            isDeleted = false;
        }

        public void Delete(ResPile resDesin)
        {
            if (isDeleted)
                throw new InvalidOperationException();
            ResPile.TransferAll(source: resPile, destin: resDesin);
            isDeleted = true;
        }
    }
}
