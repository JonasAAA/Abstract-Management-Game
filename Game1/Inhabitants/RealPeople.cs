using Game1.Industries;

namespace Game1.Inhabitants
{
    [Serializable]
    public class RealPeople
    {
        public static RealPeople CreateEmpty(MassCounter locationMassCounter, PeopleCounter locationPeopleCounter)
            => new(locationMassCounter: locationMassCounter, locationPeopleCounter: locationPeopleCounter);

        public static RealPeople CreateFromSource(RealPeople realPeopleSource)
            => CreateFromSource(realPeopleSource: realPeopleSource, locationMassCounter: realPeopleSource.locationMassCounter, locationPeopleCounter: realPeopleSource.locationPeopleCounter);

        public static RealPeople CreateFromSource(RealPeople realPeopleSource, MassCounter locationMassCounter, PeopleCounter locationPeopleCounter)
        {
            RealPeople newRealPeople = new(locationMassCounter: locationMassCounter, locationPeopleCounter: locationPeopleCounter);
            newRealPeople.TransferAllFrom(realPeopleSource: realPeopleSource);
            return newRealPeople;
        }

        public NumPeople Count
            => new((ulong)virtualToRealPeople.Count);

        public Mass Mass { get; private set; }

        private readonly MassCounter locationMassCounter;
        private readonly PeopleCounter locationPeopleCounter;
        private readonly Dictionary<VirtualPerson, RealPerson> virtualToRealPeople;

        private RealPeople(MassCounter locationMassCounter, PeopleCounter locationPeopleCounter)
        {
            Mass = Mass.zero;
            virtualToRealPeople = new();
            this.locationMassCounter = locationMassCounter;
            this.locationPeopleCounter = locationPeopleCounter;
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

        public Score AverageHappiness()
        {
            if (Count.IsZero)
                throw new InvalidOperationException("0 people don't have average happiness");
            return Score.Average
            (
                scores:
                    (from person in virtualToRealPeople.Values
                     select person.Happiness).ToArray()
            );
        }

        public Score AverageMomentaryHappiness()
        {
            if (Count.IsZero)
                throw new InvalidOperationException("0 people don't have average mometary happiness");
            return Score.Average
            (
                scores:
                    (from person in virtualToRealPeople.Values
                     select person.MomentaryHappiness).ToArray()
            );
        }

        public UDouble TotalSkill(IndustryType industryType)
            => virtualToRealPeople.Values.Sum(realPerson => (UDouble)realPerson.ActualSkill(industryType: industryType));

        public bool Contains(VirtualPerson person)
            => virtualToRealPeople.ContainsKey(person);

        public void TransferFromIfPossible(RealPeople realPersonSource, VirtualPerson person)
        {
            if (realPersonSource.virtualToRealPeople.TryGetValue(key: person, value: out RealPerson? realPerson))
                TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);
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
            realPerson.SetLocationCounters(locationMassCounter: locationMassCounter, locationPeopleCounter: locationPeopleCounter);
            Mass += realPerson.Mass;
            virtualToRealPeople.Add(key: realPerson.asVirtual, value: realPerson);
        }

        private bool Remove(VirtualPerson person)
        {
            if (virtualToRealPeople.Remove(key: person, value: out RealPerson? realPerson))
            {
                Mass -= realPerson.Mass;
                return true;
            }
            return false;
        }

#if DEBUG2
        ~RealPeople()
        {
            if (virtualToRealPeople.Count != 0)
                throw new();
        }
#endif
    }
}
