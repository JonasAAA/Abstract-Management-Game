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
        private readonly Queue<ConstULongArray> resAmounts;
        private ConstULongArray totalResAmounts;

        public bool Empty
            => endTimes.Count is 0;

        public TimedResQueue(TimeSpan duration)
        {
            if (duration.TotalSeconds < 0)
                throw new ArgumentException();
            this.duration = duration;
            
            endTimes = new();
            resAmounts = new();
            totalResAmounts = new();
        }

        public ulong TotalWeight()
            => totalResAmounts.TotalWeight();

        public void Enqueue(ConstULongArray newResAmounts)
        {
            endTimes.Enqueue(C.TotalGameTime + duration);
            resAmounts.Enqueue(newResAmounts);
            totalResAmounts += newResAmounts;
        }

        public ConstULongArray DoneResAmounts()
        {
            ConstULongArray doneResAmounts = new();
            while (endTimes.Count > 0 && endTimes.Peek() < C.TotalGameTime)
            {
                doneResAmounts += resAmounts.Dequeue();
                endTimes.Dequeue();
            }
            totalResAmounts -= doneResAmounts;
            return doneResAmounts;
        }

        // could have a version of this without taking resInd
        public void GetData(int resInd, out List<double> completionProps, out List<ulong> resAmounts)
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
