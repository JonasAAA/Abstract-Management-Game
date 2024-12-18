﻿//namespace Game1
//{
//    [Serializable]
//    public class HistoricRounder
//    {
//        private decimal historicalInaccuracy, lastValue;
//        private ulong lastResult;
//        private TimeSpan lastRoundTime;

//        public HistoricRounder()
//        {
//            historicalInaccuracy = 0;
//            lastValue = -1;
//            lastRoundTime = TimeSpan.MinValue;
//            lastResult = 0;
//        }

//        public ulong Round(decimal value, TimeSpan curTime)
//        {
//            if (lastRoundTime == curTime)
//            {
//                if (value != lastValue)
//                    throw new ArgumentException("If no time passed since last rounding, the value to round shouldn't change");
//                return lastResult;
//            }
//            lastValue = value;
//            lastRoundTime = curTime;
//            value += historicalInaccuracy;
//            lastResult = Convert.ToUInt64(value);
//            historicalInaccuracy = value - lastResult;
//            return lastResult;
//        }
//    }
//}
