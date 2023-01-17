using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class HistoricRounder
    {
        private decimal historicalInaccuracy;
        private ulong lastResult;
        private readonly TimeSpan lastRoundTime;

        public HistoricRounder()
        {
            historicalInaccuracy = 0;
            lastRoundTime = TimeSpan.MinValue;
            lastResult = 0;
        }

        public ulong Round(decimal value)
        {
            if (lastRoundTime == CurWorldManager.CurTime)
                return lastResult;
            value += historicalInaccuracy;
            lastResult = Convert.ToUInt64(value);
            historicalInaccuracy = value - lastResult;
            return lastResult;
        }
    }
}
