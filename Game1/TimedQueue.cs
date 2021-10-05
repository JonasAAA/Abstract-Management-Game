using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class TimedQueue<T>
    {
        public int Count
            => endTimeQueue.Count;
        public readonly TimeSpan duration;

        private TimeSpan currentTime, lastEndTime;
        private readonly Queue<TimeSpan> endTimeQueue;
        private readonly Queue<T> queue;

        public TimedQueue(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException();
            this.duration = duration;
            currentTime = TimeSpan.Zero;
            lastEndTime = TimeSpan.MinValue;

            endTimeQueue = new();
            queue = new();
        }

        public void Update(TimeSpan elapsed, double electrPropor)
        {
            if (!C.IsInSuitableRange(value: electrPropor))
                throw new ArgumentOutOfRangeException();
            currentTime += elapsed * electrPropor;
        }

        public virtual void Enqueue(T element)
        {
            lastEndTime = currentTime + duration;
            endTimeQueue.Enqueue(lastEndTime);
            queue.Enqueue(element);
        }

        public virtual IEnumerable<T> DoneElements()
        {
            while (endTimeQueue.Count > 0 && endTimeQueue.Peek() < currentTime)
            {
                endTimeQueue.Dequeue();
                yield return queue.Dequeue();
            }
        }

        public IEnumerable<(double complProp, T element)> GetData()
        {
            Debug.Assert(endTimeQueue.Count == queue.Count);

            foreach (var (endTime, element) in endTimeQueue.Zip(queue))
                yield return
                (
                    complProp: C.DonePart(timeLeft: endTime - currentTime, duration: duration),
                    element: element
                );
        }

        public double LastCompletionProp()
        {
            if (Count is 0)
                throw new InvalidOperationException();
            return C.DonePart(timeLeft: lastEndTime - currentTime, duration: duration);
        }
    }
}
