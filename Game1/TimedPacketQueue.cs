using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class TimedPacketQueue
    {
        public ulong TotalWeight { get; private set; }
        public readonly TimeSpan duration;

        private TimeSpan currentTime, lastEndTime;
        private readonly Queue<TimeSpan> endTimeQueue;
        private readonly Queue<(ResAmountsPacketsByDestin, List<Person>)> packetsQueue;

        public bool Empty
            => endTimeQueue.Count is 0;

        public TimedPacketQueue(TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                throw new ArgumentException();
            this.duration = duration;
            currentTime = TimeSpan.Zero;
            lastEndTime = TimeSpan.MinValue;

            endTimeQueue = new();
            packetsQueue = new();
            TotalWeight = 0;
        }

        public void Update(TimeSpan elapsed, double electrPropor)
        {
            if (!C.IsInSuitableRange(value: electrPropor))
                throw new ArgumentOutOfRangeException();
            currentTime += elapsed * electrPropor;
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, List<Person> people)
        {
            if (resAmountsPackets.Empty && people.Count is 0)
                return;
            lastEndTime = currentTime + duration;
            endTimeQueue.Enqueue(lastEndTime);
            packetsQueue.Enqueue((resAmountsPackets, people));
            TotalWeight += resAmountsPackets.TotalWeight + people.TotalWeight();
        }

        public (ResAmountsPacketsByDestin resAmountsPackets, List<Person> people) DonePackets()
        {
            ResAmountsPacketsByDestin doneResAmountsPackets = new();
            List<Person> donePeople = new();

            while (endTimeQueue.Count > 0 && endTimeQueue.Peek() < currentTime)
            {
                var (resAmountsPackets, people) = packetsQueue.Dequeue();
                doneResAmountsPackets.Add(resAmountsPackets);
                donePeople.AddRange(people);
                endTimeQueue.Dequeue();
            }
            TotalWeight -= doneResAmountsPackets.TotalWeight + donePeople.TotalWeight();
            return (doneResAmountsPackets, donePeople);
        }

        public IEnumerable<(double complProp, ConstULongArray resAmounts, int numPeople)> GetData()
        {
            Debug.Assert(endTimeQueue.Count == packetsQueue.Count);

            foreach (var (endTime, (resAmountsPackets, people)) in endTimeQueue.Zip(packetsQueue))
            {
                Debug.Assert(!resAmountsPackets.Empty || people.Count > 0);
                yield return
                (
                    complProp: C.DonePart(timeLeft: endTime - currentTime, duration: duration),
                    resAmounts: resAmountsPackets.ResAmounts,
                    numPeople: people.Count
                );
            }
        }

        public double LastCompletionProp()
        {
            if (Empty)
                throw new InvalidOperationException();
            return C.DonePart(timeLeft: lastEndTime - currentTime, duration: duration);
        }
    }
}
