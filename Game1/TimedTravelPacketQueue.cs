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
        private readonly Queue<TimeSpan> endTimeQueue;
        private readonly Queue<TravelPacket> travelPacketQueue;

        public bool Empty
            => endTimeQueue.Count is 0;

        public TimedTravelPacketQueue(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
                throw new ArgumentException();
            this.duration = duration;
            
            endTimeQueue = new();
            travelPacketQueue = new();
            TotalWeight = 0;
        }

        public void Enqueue(TravelPacket travelPacket)
        {
            if (travelPacket.Empty)
                return;
            endTimeQueue.Enqueue(C.TotalGameTime + duration);
            travelPacketQueue.Enqueue(travelPacket);
            TotalWeight += travelPacket.TotalWeight;
        }

        public TravelPacket DoneTravelPacket()
        {
            TravelPacket doneTravelPacket = new();
            while (endTimeQueue.Count > 0 && endTimeQueue.Peek() < C.TotalGameTime)
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
                    complProp: C.DonePart(endTime: endTime, duration: duration),
                    resAmounts: travelPacket.ResAmounts,
                    numPeople: travelPacket.NumPeople
                );
            }
        }

        public double PeekCompletionProp()
        {
            if (Empty)
                throw new InvalidOperationException();
            return C.DonePart(endTime: endTimeQueue.Peek(), duration: duration);
        }
    }
}
