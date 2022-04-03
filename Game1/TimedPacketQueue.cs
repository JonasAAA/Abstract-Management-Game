namespace Game1
{
    [Serializable]
    public class TimedPacketQueue : TimedQueue<(ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people)>
    {
        public int PeopleCount
            => people.Count;
        public IEnumerable<Person> People
            => people;

        public ReadOnlyULongArray TotalResAmounts { get; private set; }
        public ulong TotalWeight { get; private set; }

        private readonly MySet<Person> people;

        public TimedPacketQueue(TimeSpan duration)
            : base(duration: duration)
        {
            TotalResAmounts = new();
            TotalWeight = 0;
            people = new();
        }

        public override void Enqueue((ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people) packets)
        {
            var newPeople = packets.people.ToArray();
            if (packets.resAmountsPackets.Empty && newPeople.Length is 0)
                return;
            people.UnionWith(newPeople);
            base.Enqueue(element: (packets.resAmountsPackets, newPeople));
            TotalResAmounts += packets.resAmountsPackets.ResAmounts;
            TotalWeight += packets.resAmountsPackets.TotalWeight + newPeople.TotalWeight();
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people)
            => Enqueue(packets: (resAmountsPackets, people));

        public override IEnumerable<(ResAmountsPacketsByDestin resAmountsPackets, IEnumerable<Person> people)> DoneElements()
        {
            var result = base.DoneElements().ToArray();
            foreach (var (resAmountsPackets, people) in result)
            {
                TotalResAmounts -= resAmountsPackets.ResAmounts;
                TotalWeight -= resAmountsPackets.TotalWeight + people.TotalWeight();
                this.people.ExceptWith(people);
            }
            return result;
        }

        public (ResAmountsPacketsByDestin resAmountsPackets, MySet<Person> people) DonePacketsAndPeople()
        {
            ResAmountsPacketsByDestin doneResAmountsPackets = new();
            MySet<Person> donePeople = new();
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
