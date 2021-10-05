using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class TimedPacketQueue : TimedQueue<(ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people)>
    {
        public IEnumerable<Person> People
            => people;

        public ulong TotalWeight { get; private set; }
        //public readonly TimeSpan duration;

        //private TimeSpan currentTime, lastEndTime;
        //private readonly Queue<TimeSpan> endTimeQueue;
        //private readonly Queue<(ResAmountsPacketsByDestin, List<Person>)> packetsQueue;

        //public bool Empty
        //    => endTimeQueue.Count is 0;

        private readonly MyHashSet<Person> people;

        public TimedPacketQueue(TimeSpan duration)
            : base(duration: duration)
        {
            //if (duration <= TimeSpan.Zero)
            //    throw new ArgumentException();
            //this.duration = duration;
            //currentTime = TimeSpan.Zero;
            //lastEndTime = TimeSpan.MinValue;

            //endTimeQueue = new();
            //packetsQueue = new();
            TotalWeight = 0;
            people = new();
        }

        //public void Update(TimeSpan elapsed, double electrPropor)
        //{
        //    if (!C.IsInSuitableRange(value: electrPropor))
        //        throw new ArgumentOutOfRangeException();
        //    currentTime += elapsed * electrPropor;
        //}

        public override void Enqueue((ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people) packets)
        {
            var newPeople = packets.people.Clone();
            if (packets.resAmountsPackets.Empty && newPeople.Count() is 0)
                return;
            people.UnionWith(newPeople);
            base.Enqueue(element: (packets.resAmountsPackets, newPeople));
            //lastEndTime = currentTime + duration;
            //endTimeQueue.Enqueue(lastEndTime);
            //packetsQueue.Enqueue((resAmountsPackets, people));
            TotalWeight += packets.resAmountsPackets.TotalWeight + newPeople.TotalWeight();
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people)
            => Enqueue(packets: (resAmountsPackets, people));

        //public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, List<Person> people)
        //{
        //    if (resAmountsPackets.Empty && people.Count is 0)
        //        return;
        //    lastEndTime = currentTime + duration;
        //    endTimeQueue.Enqueue(lastEndTime);
        //    packetsQueue.Enqueue((resAmountsPackets, people));
        //    TotalWeight += resAmountsPackets.TotalWeight + people.TotalWeight();
        //}

        public override IEnumerable<(ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people)> DoneElements()
        {
            var result = base.DoneElements().ToArray();
            foreach (var (resAmountsPackets, people) in result)
            {
                TotalWeight -= resAmountsPackets.TotalWeight + people.TotalWeight();
                this.people.ExceptWith(people);
            }
            return result;
        }

        public (ResAmountsPacketsByDestin resAmountsPackets, MyHashSet<Person> people) DonePacketsAndPeople()
        {
            ResAmountsPacketsByDestin doneResAmountsPackets = new();
            MyHashSet<Person> donePeople = new();
            foreach (var (resAmountsPackets, people) in DoneElements())
            {
                doneResAmountsPackets.Add(resAmountsPackets: resAmountsPackets);
                donePeople.UnionWith(people);
            }
            return
            (
                resAmountsPackets: doneResAmountsPackets,
                people: donePeople
            );
        }

        //public (ResAmountsPacketsByDestin resAmountsPackets, List<Person> people) DoneElement()
        //{
        //    ResAmountsPacketsByDestin doneResAmountsPackets = new();
        //    List<Person> donePeople = new();

        //    while (endTimeQueue.Count > 0 && endTimeQueue.Peek() < currentTime)
        //    {
        //        var (resAmountsPackets, people) = packetsQueue.Dequeue();
        //        doneResAmountsPackets.Add(resAmountsPackets);
        //        donePeople.AddRange(people);
        //        endTimeQueue.Dequeue();
        //    }
        //    TotalWeight -= doneResAmountsPackets.TotalWeight + donePeople.TotalWeight();
        //    return (doneResAmountsPackets, donePeople);
        //}

        //public IEnumerable<(double complProp, ConstULongArray resAmounts, int numPeople)> GetData()
        //{
        //    Debug.Assert(endTimeQueue.Count == packetsQueue.Count);

        //    foreach (var (endTime, (resAmountsPackets, people)) in endTimeQueue.Zip(packetsQueue))
        //    {
        //        Debug.Assert(!resAmountsPackets.Empty || people.Count > 0);
        //        yield return
        //        (
        //            complProp: C.DonePart(timeLeft: endTime - currentTime, duration: duration),
        //            resAmounts: resAmountsPackets.ResAmounts,
        //            numPeople: people.Count
        //        );
        //    }
        //}

        //public double LastCompletionProp()
        //{
        //    if (Empty)
        //        throw new InvalidOperationException();
        //    return C.DonePart(timeLeft: lastEndTime - currentTime, duration: duration);
        //}
    }
}
