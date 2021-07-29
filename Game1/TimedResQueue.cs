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
            if (duration.TotalSeconds < 0)
                throw new ArgumentException();
            this.duration = duration;
            
            endTimes = new();
            resAmounts = new();
        }

        public void Enqueue(ConstUIntArray newResAmounts)
        {
            endTimes.Enqueue(C.TotalGameTime + duration);
            resAmounts.Enqueue(newResAmounts);
        }

        public ConstUIntArray DoneResAmounts()
        {
            ConstUIntArray doneResAmounts = new();
            while (endTimes.Count > 0 && endTimes.Peek() < C.TotalGameTime)
            {
                doneResAmounts += resAmounts.Dequeue();
                endTimes.Dequeue();
            }
            return doneResAmounts;
        }

        // could have a version of this without taking resInd
        public void GetData(int resInd, out List<double> completionProps, out List<uint> resAmounts)
        {
            completionProps = new();
            resAmounts = new();

            Debug.Assert(endTimes.Count == this.resAmounts.Count);

            foreach (var (endTime, resAmount) in endTimes.Zip(this.resAmounts))
            {
                if (resAmount[resInd] is 0)
                    continue;
                completionProps.Add(C.DonePart(endTime: endTime, duration: duration));
                resAmounts.Add(resAmount[resInd]);
            }

            Debug.Assert(completionProps.Count == resAmounts.Count);
        }

        public double PeekCompletionProp()
        {
            if (Empty)
                throw new InvalidOperationException();
            return C.DonePart(endTime: endTimes.Peek(), duration: duration);
        }
    }
}
