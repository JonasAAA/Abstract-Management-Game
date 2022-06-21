using Game1.Industries;
using System.Diagnostics.CodeAnalysis;

namespace Game1.Inhabitants
{
    [Serializable]
    public class RealPeople : IHasMass
    {
        public static RealPeople CreateEmpty()
            => new();

        public static RealPeople CreateFromSource(RealPeople realPeopleSource)
        {
            RealPeople newRealPeople = new();
            newRealPeople.TransferAllFrom(realPeopleSource: realPeopleSource);
            return newRealPeople;
        }

        public ulong Count
            => (ulong)virtualToRealPeople.Count;

        public ulong Mass { get; private set; }

        private readonly Dictionary<VirtualPerson, RealPerson> virtualToRealPeople;

        private RealPeople()
        {
            Mass = 0;
            virtualToRealPeople = new();
        }

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

        /// <param name="personalUpdateSkillsParams">if null, will use default update</param>
        public void Update(RealPerson.UpdateLocationParams updateLocationParams, Func<RealPerson, UpdatePersonSkillsParams?>? personalUpdateSkillsParams)
        {
            personalUpdateSkillsParams ??= realPerson => null;
            foreach (var realPerson in virtualToRealPeople.Values)
                realPerson.Update(updateLocationParams: updateLocationParams, updateSkillsParams: personalUpdateSkillsParams(realPerson));
        }

        public UDouble TotalSkill(IndustryType industryType)
            => virtualToRealPeople.Values.Sum(realPerson => (UDouble)realPerson.Skills[industryType]);

        public bool Contains(VirtualPerson person)
            => virtualToRealPeople.ContainsKey(person);

        public void TransferFromIfPossible(RealPeople realPersonSource, VirtualPerson person)
        {
            if (realPersonSource.Remove(person: person, out RealPerson? realPerson))
                Add(realPerson: realPerson);
        }

        public void TransferFrom(RealPeople realPersonSource, RealPerson realPerson)
        {
            if (!realPersonSource.Remove(person: realPerson.asVirtual))
                throw new ArgumentException();
            Add(realPerson: realPerson);
        }

        public void TransferAllFrom(RealPeople realPeopleSource)
        {
            foreach (var realPerson in realPeopleSource.virtualToRealPeople.Values)
                TransferFrom(realPersonSource: realPeopleSource, realPerson: realPerson);
        }

        private void Add(RealPerson realPerson)
        {
            Mass += realPerson.Mass;
            virtualToRealPeople.Add(key: realPerson.asVirtual, value: realPerson);
        }

        private bool Remove(VirtualPerson person, [NotNullWhen(true)] out RealPerson? realPerson)
        {
            if (virtualToRealPeople.Remove(key: person, value: out realPerson))
            {
                Mass -= realPerson.Mass;
                return true;
            }
            return false;
        }

        private bool Remove(VirtualPerson person)
            => Remove(person: person, realPerson: out RealPerson? _);

#if DEBUG
        ~RealPeople()
        {
            if (virtualToRealPeople.Count != 0)
                throw new();
        }
#endif
    }
}
