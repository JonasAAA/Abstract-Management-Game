using Game1.Collections;
using Game1.Industries;
using static Game1.WorldManager;

namespace Game1.Inhabitants
{
    /// <summary>
    /// For each person, at least one enjoyment will have Score.highest value, and at leat one enjoyment will value Score.lowest value.
    /// Happiness currently only influences productivity.
    /// </summary>
    [Serializable]
    public readonly struct RealPeopleStats
    {
        public static readonly RealPeopleStats empty = default;

        static RealPeopleStats()
            => empty = new
            (
                totalMass: Mass.zero,
                totalNumPeople: NumPeople.zero,
                totalReqWatts: 0,
                timeCoefficient: Propor.empty,
                age: TimeSpan.Zero,
                allocEnergyPropor: Propor.empty,
                happiness: Score.lowest,
                momentaryHappiness: Score.lowest,
                enjoyments: new(value: Score.lowest),
                talents: new(value: Score.lowest),
                skills: new(value: Score.lowest),
                totalProductivities: new(value: 0)
            );

        public static RealPeopleStats ForNewPerson(Mass totalMass, ulong reqWatts, TimeSpan age, Score startingHappiness, EnumDict<IndustryType, Score> enjoyments, EnumDict<IndustryType, Score> talents, EnumDict<IndustryType, Score> skills)
        {
            if (age < TimeSpan.Zero)
                throw new ArgumentException();
            return new
            (
                totalMass: totalMass,
                totalNumPeople: new(1),
                totalReqWatts: reqWatts, 
                timeCoefficient: Propor.full,
                age: age,
                allocEnergyPropor: Propor.full,
                happiness: startingHappiness,
                momentaryHappiness: startingHappiness,
                enjoyments: Score.ScaleToHaveHighestAndLowest(scores: enjoyments),
                talents: talents,
                skills: skills
            );
        }

        public bool IsEmpty
            => totalNumPeople.IsZero;

        // This is init-only property to allow Stats with { AllocEnergyPropor = ... } type of syntax
        /// <summary>
        /// How much of REQUESTED energy is given. So if request 0, get 0, the proportion is full
        /// </summary>
        public Propor AllocEnergyPropor { get; init; }

        public readonly Mass totalMass;
        public readonly NumPeople totalNumPeople;
        public readonly ulong totalReqWatts;
        public readonly Propor timeCoefficient;
        public readonly TimeSpan age;
        public readonly Score happiness, momentaryHappiness;
        public readonly EnumDict<IndustryType, Score> enjoyments, talents, skills;
        public readonly EnumDict<IndustryType, UDouble> totalProductivities;

        public RealPeopleStats(Mass totalMass, NumPeople totalNumPeople, ulong totalReqWatts, Propor timeCoefficient, TimeSpan age, Propor allocEnergyPropor, Score happiness,
            Score momentaryHappiness, EnumDict<IndustryType, Score> enjoyments, EnumDict<IndustryType, Score> talents, EnumDict<IndustryType, Score> skills)
            : this
            (
                totalMass: totalMass,
                totalNumPeople: totalNumPeople,
                totalReqWatts: totalReqWatts,
                timeCoefficient: timeCoefficient,
                age: age,
                allocEnergyPropor: allocEnergyPropor,
                happiness: happiness,
                momentaryHappiness: momentaryHappiness,
                enjoyments: enjoyments,
                talents: talents,
                skills: skills,
                totalProductivities: new
                (
                    selector: industryType => totalNumPeople.value * (UDouble)Score.WeightedAverageOfTwo
                    (
                        score1: happiness,
                        score2: skills[industryType],
                        score1Propor: CurWorldConfig.productivityHappinessWeight
                    )
                )
            )
        { }

        private RealPeopleStats(Mass totalMass, NumPeople totalNumPeople, ulong totalReqWatts, Propor timeCoefficient, TimeSpan age, Propor allocEnergyPropor, Score happiness,
            Score momentaryHappiness, EnumDict<IndustryType, Score> enjoyments, EnumDict<IndustryType, Score> talents, EnumDict<IndustryType, Score> skills, EnumDict<IndustryType, UDouble> totalProductivities)
        {
            this.totalMass = totalMass;
            this.totalNumPeople = totalNumPeople;
            this.totalReqWatts = totalReqWatts;
            this.timeCoefficient = timeCoefficient;
            this.age = age;
            this.AllocEnergyPropor = allocEnergyPropor;
            this.happiness = happiness;
            this.momentaryHappiness = momentaryHappiness;
            this.enjoyments = enjoyments;
            this.talents = talents;
            this.skills = skills;
            this.totalProductivities = totalProductivities;
        }

        public RealPeopleStats CombineWith(RealPeopleStats other)
        {
            if ((totalNumPeople + other.totalNumPeople).IsZero)
                return empty;

            var current = this;

            return new
            (
                totalMass: totalMass + other.totalMass,
                totalNumPeople: totalNumPeople + other.totalNumPeople,
                totalReqWatts: totalReqWatts + other.totalReqWatts,
                timeCoefficient: Propor.Create
                (
                    part: totalNumPeople.value * timeCoefficient + other.totalNumPeople.value * other.timeCoefficient,
                    whole: totalNumPeople.value + other.totalNumPeople.value
                )!.Value,
                age: (totalNumPeople.value * age + other.totalNumPeople.value * other.age) / (totalNumPeople.value + other.totalNumPeople.value),
                allocEnergyPropor: Propor.Create
                (
                    part: totalNumPeople.value * AllocEnergyPropor + other.totalNumPeople.value * other.AllocEnergyPropor,
                    whole: totalNumPeople.value + other.totalNumPeople.value
                )!.Value,
                happiness: CombinedScore(stats => stats.happiness),
                momentaryHappiness: CombinedScore(stats => stats.momentaryHappiness),
                enjoyments: new(industryType => CombinedScore(stats => stats.enjoyments[industryType])),
                talents: new(industryType => CombinedScore(stats => stats.talents[industryType])),
                skills: new(industryType => CombinedScore(stats => stats.skills[industryType]))
            );

            Score CombinedScore(Func<RealPeopleStats, Score> selector)
                => Score.WeightedAverage
                (
                    (weight: current.totalNumPeople.value, score: selector(current)),
                    (weight: other.totalNumPeople.value, score: selector(other))
                );
        }

        public RealPeopleStats Subtract(RealPeopleStats other)
        {
            if (totalNumPeople == other.totalNumPeople)
                return empty;

            var current = this;

            return new
            (
                totalMass: totalMass - other.totalMass,
                totalNumPeople: totalNumPeople - other.totalNumPeople,
                totalReqWatts: totalReqWatts - other.totalReqWatts,
                timeCoefficient: Propor.Create
                (
                    part: totalNumPeople.value * timeCoefficient - other.totalNumPeople.value * other.timeCoefficient,
                    whole: totalNumPeople.value - other.totalNumPeople.value
                )!.Value,
                age: MyMathHelper.Max(TimeSpan.Zero, (totalNumPeople.value * age - other.totalNumPeople.value * other.age) / (totalNumPeople.value - other.totalNumPeople.value)),
                allocEnergyPropor: Propor.Create
                (
                    part: totalNumPeople.value * AllocEnergyPropor - other.totalNumPeople.value * other.AllocEnergyPropor,
                    whole: totalNumPeople.value - other.totalNumPeople.value
                )!.Value,
                happiness: SubtractedScore(stats => stats.happiness),
                momentaryHappiness: SubtractedScore(stats => stats.momentaryHappiness),
                enjoyments: new(industryType => SubtractedScore(stats => stats.enjoyments[industryType])),
                talents: new(industryType => SubtractedScore(stats => stats.talents[industryType])),
                skills: new(industryType => SubtractedScore(stats => stats.skills[industryType]))
            );

            Score SubtractedScore(Func<RealPeopleStats, Score> selector)
                => Score.WeightedAverageWithPossiblyNegativeWeights
                (
                    (weight: (long)current.totalNumPeople.value, score: selector(current)),
                    (weight: -(long)other.totalNumPeople.value, score: selector(other))
                );
        }

        public override string ToString()
            => IsEmpty switch
            {
                true => "No people are here",
                false => $"Number of people {totalNumPeople}\naverage time coefficient {timeCoefficient}\naverage age {age}\naverage happiness {happiness:0.00}\naverage momentary happiness {momentaryHappiness:0.00}\n"
            };
    }
}
