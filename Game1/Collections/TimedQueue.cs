using System.Collections;
using static Game1.WorldManager;

namespace Game1.Collections
{
    [Serializable]
    public class TimedQueue<T> : IReadOnlyCollection<T>
    {
        private static readonly TimeSpan normalizedDuration = TimeSpan.FromSeconds(1);

        public int Count
            => endNormalizedTimeQueue.Count;

        // Normalized time means time is calculated in such a way that normalized duration is 1 second
        private TimeSpan curNormalizedLocalTime, lastNormalizedEndTime;
        private readonly Queue<TimeSpan> endNormalizedTimeQueue;
        private readonly Queue<T> queue;

        public TimedQueue()
        {
            curNormalizedLocalTime = TimeSpan.Zero;
            lastNormalizedEndTime = TimeSpan.MinValue;

            endNormalizedTimeQueue = new();
            queue = new();
        }

        public void Update(TimeSpan duration, Propor workingPropor)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException();
            curNormalizedLocalTime += CurWorldManager.Elapsed * workingPropor * normalizedDuration.TotalSeconds / duration.TotalSeconds;
        }

        public virtual void Enqueue(T element)
        {
            lastNormalizedEndTime = curNormalizedLocalTime + normalizedDuration;
            endNormalizedTimeQueue.Enqueue(lastNormalizedEndTime);
            queue.Enqueue(element);
        }

        public virtual IEnumerable<T> DoneElements()
        {
            while (endNormalizedTimeQueue.Count > 0 && endNormalizedTimeQueue.Peek() < curNormalizedLocalTime)
            {
                endNormalizedTimeQueue.Dequeue();
                yield return queue.Dequeue();
            }
        }

        public IEnumerable<(Propor complPropor, T element)> GetData()
        {
            Debug.Assert(endNormalizedTimeQueue.Count == queue.Count);

            foreach (var (normalizedEndTime, element) in endNormalizedTimeQueue.Zip(queue))
                yield return
                (
                    complPropor: C.DonePropor(timeLeft: normalizedEndTime - curNormalizedLocalTime, duration: normalizedDuration),
                    element
                );
        }

        public Propor LastCompletionPropor()
        {
            if (Count is 0)
                throw new InvalidOperationException();
            return C.DonePropor(timeLeft: lastNormalizedEndTime - curNormalizedLocalTime, duration: normalizedDuration);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => queue.GetEnumerator();
    }
}
