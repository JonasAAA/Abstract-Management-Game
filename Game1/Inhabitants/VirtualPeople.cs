using System.Collections;
using Game1.Collections;

namespace Game1.Inhabitants
{
    [Serializable]
    public sealed class VirtualPeople : IReadOnlyCollection<VirtualPerson>
    {
        public NumPeople Count
            => new((ulong)people.Count);

        int IReadOnlyCollection<VirtualPerson>.Count
            => people.Count;

        private readonly ThrowingSet<VirtualPerson> people;

        public VirtualPeople()
            => people = [];

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
