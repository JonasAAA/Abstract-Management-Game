using Game1.Industries;

namespace Game1.Inhabitants
{
    [Serializable]
    public class RealPeople : IWithRealPeopleStats
    {
        [Serializable]
        public readonly record struct Statistics(Mass Mass, NumPeople NumPeople, Score AverageHappiness, Score AverageMomentaryHappiness)
        {
            public static readonly Statistics empty;

            static Statistics()
                => empty = new
                (
                    Mass: Mass.zero,
                    NumPeople: NumPeople.zero,
                    AverageHappiness: Score.lowest,
                    AverageMomentaryHappiness: Score.lowest
                );

            public Statistics CombineWith(Statistics other)
                => new
                (
                    Mass: Mass + other.Mass,
                    NumPeople: NumPeople + other.NumPeople,
                    AverageHappiness: Score.WeightedAverage
                    (
                        (weight: NumPeople.value, score: AverageHappiness),
                        (weight: other.NumPeople.value, score: other.AverageHappiness)
                    ),
                    AverageMomentaryHappiness: Score.WeightedAverage
                    (
                        (weight: NumPeople.value, score: AverageMomentaryHappiness),
                        (weight: other.NumPeople.value, score: other.AverageMomentaryHappiness)
                    )
                );

            public string HappinessStats()
                => NumPeople.IsZero switch
                {
                    true => "no happiness stats as no\npeople are here\n",
                    false => $"average happiness {AverageHappiness:0.00}\naverage momentary happiness {AverageMomentaryHappiness:0.00}\n"
                };

            public override string ToString()
                => $"Number of people {NumPeople}\n{HappinessStats()}";
        }

        public static RealPeople CreateEmpty(LocationCounters locationCounters)
            => new(locationCounters: locationCounters);

        public static RealPeople CreateFromSource(RealPeople realPeopleSource)
            => CreateFromSource(realPeopleSource: realPeopleSource, locationCounters: realPeopleSource.locationCounters);

        public static RealPeople CreateFromSource(RealPeople realPeopleSource, LocationCounters locationCounters)
        {
            RealPeople newRealPeople = new(locationCounters: locationCounters);
            newRealPeople.TransferAllFrom(realPeopleSource: realPeopleSource);
            return newRealPeople;
        }

        public NumPeople NumPeople
            => RealPeopleStats.NumPeople;

        public Statistics RealPeopleStats { get; private set;}

        private readonly LocationCounters locationCounters;
        private readonly Dictionary<VirtualPerson, RealPerson> virtualToRealPeople;

        private RealPeople(LocationCounters locationCounters)
        {
            RealPeopleStats = new(Mass: Mass.zero, NumPeople: NumPeople.zero, AverageHappiness: Score.lowest, AverageMomentaryHappiness: Score.lowest);
            virtualToRealPeople = new();
            this.locationCounters = locationCounters;
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
            RealPeopleStats = RealPeopleStats with
            {
                AverageHappiness = Score.Average
                (
                    scores:
                        (from person in virtualToRealPeople.Values
                         select person.Happiness).ToArray()
                ),
                AverageMomentaryHappiness = Score.Average
                (
                    scores:
                        (from person in virtualToRealPeople.Values
                         select person.MomentaryHappiness).ToArray()
                )
            };
            Debug.Assert(RealPeopleStats.NumPeople.value == (ulong)virtualToRealPeople.Count);
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
            realPerson.SetLocationCounters(locationCounters: locationCounters);
            virtualToRealPeople.Add(key: realPerson.asVirtual, value: realPerson);
            RealPeopleStats = RealPeopleStats with
            {
                Mass = RealPeopleStats.Mass + realPerson.Mass,
                NumPeople = RealPeopleStats.NumPeople + NumPeople.one
            };
        }

        private bool Remove(VirtualPerson person)
        {
            if (virtualToRealPeople.Remove(key: person, value: out RealPerson? realPerson))
            {
                RealPeopleStats = RealPeopleStats with
                {
                    Mass = RealPeopleStats.Mass - realPerson.Mass,
                    NumPeople = RealPeopleStats.NumPeople - NumPeople.one
                };
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
