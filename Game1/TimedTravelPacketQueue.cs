using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class TimedTravelPacketQueue
    {
        public ulong TotalWeight { get; private set; }
        public readonly TimeSpan duration;

        private TimeSpan currentTime, lastEndTime;
        private readonly Queue<TimeSpan> endTimeQueue;
        private readonly Queue<TravelPacket> travelPacketQueue;

        public bool Empty
            => endTimeQueue.Count is 0;

        public TimedTravelPacketQueue(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
                throw new ArgumentException();
            this.duration = duration;
            currentTime = TimeSpan.Zero;
            lastEndTime = TimeSpan.MinValue;

            endTimeQueue = new();
            travelPacketQueue = new();
            TotalWeight = 0;
        }

        public void Update(TimeSpan elapsed, double electrPropor)
        {
            if (!C.IsInSuitableRange(value: electrPropor))
                throw new ArgumentOutOfRangeException();
            currentTime += elapsed * electrPropor;
        }

        public void Enqueue(TravelPacket travelPacket)
        {
            if (travelPacket.Empty)
                return;
            lastEndTime = currentTime + duration;
            endTimeQueue.Enqueue(lastEndTime);
            travelPacketQueue.Enqueue(travelPacket);
            TotalWeight += travelPacket.TotalWeight;
        }

        public TravelPacket DoneTravelPacket()
        {
            TravelPacket doneTravelPacket = new();
            while (endTimeQueue.Count > 0 && endTimeQueue.Peek() < currentTime)
            {
                doneTravelPacket.Add(travelPacket: travelPacketQueue.Dequeue());
                endTimeQueue.Dequeue();
            }
            TotalWeight -= doneTravelPacket.TotalWeight;
            return doneTravelPacket;
        }

        public IEnumerable<(double complProp, ConstULongArray resAmounts, int numPeople)> GetData()
        {
            Debug.Assert(endTimeQueue.Count == travelPacketQueue.Count);

            foreach (var (endTime, travelPacket) in endTimeQueue.Zip(travelPacketQueue))
            {
                Debug.Assert(!travelPacket.Empty);
                yield return
                (
                    complProp: C.DonePart(timeLeft: endTime - currentTime, duration: duration),
                    resAmounts: travelPacket.ResAmounts,
                    numPeople: travelPacket.NumPeople
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
