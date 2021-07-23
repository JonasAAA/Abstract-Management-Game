using System;
using System.Collections.Generic;

namespace Game1
{
    public class TimedResQueue
    {
        private readonly TimeSpan duration;
        private readonly Queue<TimeSpan> endTimes;
        private readonly Queue<IntArray> resAmounts;

        public bool Empty
            => endTimes.Count is 0;

        public TimedResQueue(TimeSpan duration)
        {
            this.duration = duration;
            
            endTimes = new();
            resAmounts = new();
        }

        public void Enqueue(IntArray newResAmounts)
        {
            endTimes.Enqueue(C.GameTime.TotalGameTime + duration);
            resAmounts.Enqueue(newResAmounts);
        }

        public IntArray DoneResAmounts()
        {
            IntArray doneResAmounts = new();
            while (endTimes.Count > 0 && endTimes.Peek() < C.GameTime.TotalGameTime)
            {
                doneResAmounts += resAmounts.Dequeue();
                endTimes.Dequeue();
            }
            return doneResAmounts;
        }
    }
}
