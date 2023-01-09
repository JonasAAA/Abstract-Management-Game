namespace Game1
{
    [Serializable]
    public class RoundWithHistory
    {
        decimal historicalInaccuracy;

        public RoundWithHistory()
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
