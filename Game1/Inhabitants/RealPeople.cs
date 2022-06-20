using Game1.Industries;
using System.Diagnostics.CodeAnalysis;

namespace Game1.Inhabitants
{
    [Serializable]
    public class RealPeople : IHasMass
    {
        public ulong Count
            => (ulong)virtualToRealPeople.Count;

        public ulong Mass { get; private set; }

        private readonly Dictionary<VirtualPerson, RealPerson> virtualToRealPeople;

        public RealPeople()
        {
            Mass = 0;
            virtualToRealPeople = new();
        }

        public RealPeople(RealPeople peopleSource)
            : this()
            => TransferAllFrom(peopleSource: peopleSource);

        public void AddByMagic(RealPerson realPerson)
            => Add(realPerson: realPerson);

        /// <summary>
        /// Unlike the usual foreach loop, can change the collection with personalAction
        /// </summary>
        public void ForEach(Action<RealPerson> personalAction)
        {
            // ToList is more performant than ToArray according to https://stackoverflow.com/a/16323412
            // TODO(performance) if this is too slow, this method could take a "needCopy" argument and copy the Values only if that's needed
            foreach (var person in virtualToRealPeople.Values.ToList())
                personalAction(person);
        }

        /// <param name="personalUpdate">if null, will use default update</param>
        public void Update(RealPerson.UpdateParams updateParams, Action<RealPerson>? personalUpdate)
        {
            personalUpdate ??= realPerson => IActivityCenter.UpdatePersonDefault(person: realPerson);
            foreach (var realPerson in virtualToRealPeople.Values)
                realPerson.Update(updateParams: updateParams, update: () => personalUpdate(realPerson));
        }

        public UDouble TotalSkill(IndustryType industryType)
            => virtualToRealPeople.Values.Sum(realPerson => (UDouble)realPerson.skills[industryType]);

        public bool Contains(VirtualPerson virtualPerson)
            => virtualToRealPeople.ContainsKey(virtualPerson);

        public void TransferFromIfPossible(RealPeople personSource, VirtualPerson virtualPerson)
        {
            if (personSource.Remove(virtualPerson: virtualPerson, out RealPerson? realPerson))
                Add(realPerson: realPerson);
        }

        public void TransferFrom(RealPeople personSource, RealPerson realPerson)
        {
            if (!personSource.Remove(virtualPerson: realPerson.asVirtual))
                throw new ArgumentException();
            Add(realPerson: realPerson);
        }

        public void TransferAllFrom(RealPeople peopleSource)
        {
            foreach (var realPerson in peopleSource.virtualToRealPeople.Values)
                TransferFrom(personSource: peopleSource, realPerson: realPerson);
        }

        private void Add(RealPerson realPerson)
        {
            Mass += realPerson.Mass;
            virtualToRealPeople.Add(key: realPerson.asVirtual, value: realPerson);
        }

        private bool Remove(VirtualPerson virtualPerson, [NotNullWhen(true)] out RealPerson? realPerson)
        {
            if (virtualToRealPeople.Remove(key: virtualPerson, value: out realPerson))
            {
                Mass -= realPerson.Mass;
                return true;
            }
            return false;
        }

        private bool Remove(VirtualPerson virtualPerson)
            => Remove(virtualPerson: virtualPerson, realPerson: out RealPerson? _);

#if DEBUG
        ~RealPeople()
        {
            if (virtualToRealPeople.Count != 0)
                throw new();
        }
#endif
    }
}
