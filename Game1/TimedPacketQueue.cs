using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Game1
{
    [DataContract]
    public class TimedPacketQueue : TimedQueue<(ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people)>
    {
        public IEnumerable<Person> People
            => people;

        [DataMember]
        public ulong TotalWeight { get; private set; }

        [DataMember]
        private readonly MyHashSet<Person> people;

        public TimedPacketQueue(TimeSpan duration)
            : base(duration: duration)
        {
            TotalWeight = 0;
            people = new();
        }

        public override void Enqueue((ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people) packets)
        {
            var newPeople = packets.people.Clone();
            if (packets.resAmountsPackets.Empty && newPeople.Count() is 0)
                return;
            people.UnionWith(newPeople);
            base.Enqueue(element: (packets.resAmountsPackets, newPeople));
            TotalWeight += packets.resAmountsPackets.TotalWeight + newPeople.TotalWeight();
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people)
            => Enqueue(packets: (resAmountsPackets, people));

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
    }
}
