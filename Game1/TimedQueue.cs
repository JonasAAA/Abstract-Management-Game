using System.Collections;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class TimedQueue<T> : IEnumerable<T>
    {
        public int Count
            => endTimeQueue.Count;
        public readonly TimeSpan duration;

        private TimeSpan currentLocalTime, lastEndTime;
        private readonly Queue<TimeSpan> endTimeQueue;
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

        public void Update(Propor workingPropor)
            => currentLocalTime += CurWorldManager.Elapsed * workingPropor;

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

        public IEnumerable<(Propor complPropor, T element)> GetData()
        {
            Debug.Assert(endTimeQueue.Count == queue.Count);

            foreach (var (endTime, element) in endTimeQueue.Zip(queue))
                yield return
                (
                    complPropor: C.DonePropor(timeLeft: endTime - currentLocalTime, duration: duration),
                    element: element
                );
        }

        public Propor LastCompletionPropor()
        {
            if (Count is 0)
                throw new InvalidOperationException();
            return C.DonePropor(timeLeft: lastEndTime - currentLocalTime, duration: duration);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => queue.GetEnumerator();
    }
}
