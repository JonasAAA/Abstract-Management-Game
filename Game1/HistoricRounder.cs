namespace Game1
{
    [Serializable]
    public class HistoricRounder
    {
        private decimal historicalInaccuracy;

        public HistoricRounder()
            => historicalInaccuracy = 0;

        public ulong Round(decimal value)
        {
            value += historicalInaccuracy;
            ulong result = Convert.ToUInt64(value);
            historicalInaccuracy = value - result;
            return result;
        }
    }
}
