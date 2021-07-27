using System;
using System.Collections.Generic;

namespace Game1
{
    public class TimedResQueue
    {
        private readonly TimeSpan duration;
        private readonly Queue<TimeSpan> endTimes;
        private readonly Queue<ConstIntArray> resAmounts;

        public bool Empty
            => endTimes.Count is 0;

        public TimedResQueue(TimeSpan duration)
        {
            this.duration = duration;
            
            endTimes = new();
            resAmounts = new();
        }

        public void Enqueue(ConstIntArray newResAmounts)
        {
            endTimes.Enqueue(C.GameTime.TotalGameTime + duration);
            resAmounts.Enqueue(newResAmounts);
        }

        public ConstIntArray DoneResAmounts()
        {
            ConstIntArray doneResAmounts = new();
            while (endTimes.Count > 0 && endTimes.Peek() < C.GameTime.TotalGameTime)
            {
                doneResAmounts += resAmounts.Dequeue();
                endTimes.Dequeue();
            }
            return doneResAmounts;
        }
    }
}
