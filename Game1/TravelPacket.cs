using System.Collections.Generic;

namespace Game1
{
    public class TravelPacket
    {
        public ConstULongArray ResAmounts
            => resAmounts;
        public IEnumerable<Person> People
            => people;
        public int NumPeople
            => people.Count;
        public bool Empty
            => TotalWeight is 0;
        public ulong TotalWeight { get; private set; }

        private ULongArray resAmounts;
        private readonly List<Person> people;

        public TravelPacket()
        {
            resAmounts = new();
            people = new();
            TotalWeight = 0;
        }

        //public TravelPacket(ConstULongArray resAmounts, IEnumerable<Person> people)
        //{
        //    this.resAmounts = resAmounts.ToULongArray();
        //    this.people = people.ToList();
        //    TotalWeight = resAmounts.TotalWeight() + this.people.TotalWeight();
        //}

        public void Add(TravelPacket travelPacket)
        {
            resAmounts += travelPacket.resAmounts;
            people.AddRange(travelPacket.people);
            TotalWeight += travelPacket.TotalWeight;
        }

        public void Add(ConstULongArray resAmounts)
        {
            this.resAmounts += resAmounts;
            TotalWeight += resAmounts.TotalWeight();
        }

        public void Add(int resInd, ulong resAmount)
        {
            resAmounts[resInd] += resAmount;
            TotalWeight += Resource.all[resInd].weight * resAmount;
        }

        public void Add(IEnumerable<Person> people)
        {
            this.people.AddRange(people);
            TotalWeight += people.TotalWeight();
        }

        public void Add(Person person)
        {
            people.Add(person);
            TotalWeight += person.weight;
        }

        public void Remove(ConstULongArray resAmounts)
        {
            this.resAmounts -= resAmounts;
            TotalWeight -= resAmounts.TotalWeight();
        }
    }
}
