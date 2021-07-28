using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class TimedResQueue
    {
        private readonly TimeSpan duration;
        private readonly Queue<TimeSpan> endTimes;
        private readonly Queue<ConstUIntArray> resAmounts;

        public bool Empty
            => endTimes.Count is 0;

        public TimedResQueue(TimeSpan duration)
        {
            this.duration = duration;
            
            endTimes = new();
            resAmounts = new();
        }

        public void Enqueue(ConstUIntArray newResAmounts)
        {
            endTimes.Enqueue(C.GameTime.TotalGameTime + duration);
            resAmounts.Enqueue(newResAmounts);
        }

        public ConstUIntArray DoneResAmounts()
        {
            ConstUIntArray doneResAmounts = new();
            while (endTimes.Count > 0 && endTimes.Peek() < C.GameTime.TotalGameTime)
            {
                doneResAmounts += resAmounts.Dequeue();
                endTimes.Dequeue();
            }
            return doneResAmounts;
        }

        public void GetData(int resInd, out List<double> completionProps, out List<uint> resAmounts)
        {
            completionProps = new();
            resAmounts = new();

            Debug.Assert(endTimes.Count == this.resAmounts.Count);

            foreach (var (endTime, resAmount) in endTimes.Zip(this.resAmounts))
            {
                completionProps.Add(1 - (endTime.TotalSeconds - C.GameTime.TotalGameTime.TotalSeconds) / duration.TotalSeconds);
                if (completionProps[^1] < -C.minPosDouble || completionProps[^1] > 1 + C.minPosDouble)
                    throw new Exception();
                resAmounts.Add(resAmount[resInd]);
            }

            Debug.Assert(completionProps.Count == resAmounts.Count);
        }
    }
}
