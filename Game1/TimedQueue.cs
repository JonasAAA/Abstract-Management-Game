using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public class TimedQueue<T>
    {
        public int Count
            => endTimeQueue.Count;
        [DataMember]
        public readonly TimeSpan duration;
        
        [DataMember]
        private TimeSpan currentLocalTime, lastEndTime;
        [DataMember]
        private readonly Queue<TimeSpan> endTimeQueue;
        [DataMember]
        private readonly Queue<T> queue;

        public TimedQueue(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException();
            this.duration = duration;
            currentLocalTime = TimeSpan.Zero;
            lastEndTime = TimeSpan.MinValue;

            endTimeQueue = new();
            queue = new();
        }

        public void Update(double workingPropor)
        {
            if (!C.IsInSuitableRange(value: workingPropor))
                throw new ArgumentOutOfRangeException();
            currentLocalTime += Elapsed * workingPropor;
        }

        public virtual void Enqueue(T element)
        {
            lastEndTime = currentLocalTime + duration;
            endTimeQueue.Enqueue(lastEndTime);
            queue.Enqueue(element);
        }

        public virtual IEnumerable<T> DoneElements()
        {
            while (endTimeQueue.Count > 0 && endTimeQueue.Peek() < currentLocalTime)
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
                    complProp: C.DonePart(timeLeft: endTime - currentLocalTime, duration: duration),
                    element: element
                );
        }

        public double LastCompletionProp()
        {
            if (Count is 0)
                throw new InvalidOperationException();
            return C.DonePart(timeLeft: lastEndTime - currentLocalTime, duration: duration);
        }
    }
}
