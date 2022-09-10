using System.Collections;

namespace Game1.Inhabitants
{
    [Serializable]
    public class VirtualPeople : IEnumerable<VirtualPerson>
    {
        public NumPeople Count
            => new(people.Count);

        private readonly MySet<VirtualPerson> people;

        public VirtualPeople()
            => people = new();

        public void Add(VirtualPerson person)
            => people.Add(person);

        public bool Contains(VirtualPerson person)
            => people.Contains(person);

        public void Remove(VirtualPerson person)
            => people.Remove(person);

        IEnumerator<VirtualPerson> IEnumerable<VirtualPerson>.GetEnumerator()
            => people.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => ((IEnumerable)people).GetEnumerator();
    }
}
