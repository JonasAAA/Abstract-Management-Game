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
        public static readonly RealPeopleStats empty;

        static RealPeopleStats()
            => empty = new
            (
                totalMass: Mass.zero,
                totalNumPeople: NumPeople.zero,
                timeCoefficient: Propor.empty,
                age: TimeSpan.Zero,
                happiness: Score.lowest,
                momentaryHappiness: Score.lowest,
                enjoyments: new(value: Score.lowest),
                talents: new(value: Score.lowest),
                skills: new(value: Score.lowest)
            );

        public static RealPeopleStats ForNewPerson(Mass totalMass, TimeSpan age, Score startingHappiness, EnumDict<IndustryType, Score> enjoyments, EnumDict<IndustryType, Score> talents, EnumDict<IndustryType, Score> skills)
        {
            if (age < TimeSpan.Zero)
                throw new ArgumentException();
            return new
            (
                totalMass: totalMass,
                totalNumPeople: new(1),
                timeCoefficient: Propor.full,
                age: age,
                happiness: startingHappiness,
                momentaryHappiness: startingHappiness,
                enjoyments: Score.ScaleToHaveHighestAndLowest(scores: enjoyments),
                talents: talents,
                skills: skills
            );
        }

        public bool IsEmpty
            => totalNumPeople.IsZero;

        public readonly Mass totalMass;
        public readonly NumPeople totalNumPeople;
        public readonly Propor timeCoefficient;
        public readonly TimeSpan age;
        public readonly Score happiness, momentaryHappiness;
        public readonly EnumDict<IndustryType, Score> enjoyments, talents, skills;

        public RealPeopleStats(Mass totalMass, NumPeople totalNumPeople, Propor timeCoefficient, TimeSpan age, Score happiness,
            Score momentaryHappiness, EnumDict<IndustryType, Score> enjoyments, EnumDict<IndustryType, Score> talents, EnumDict<IndustryType, Score> skills)
        {
            this.totalMass = totalMass;
            this.totalNumPeople = totalNumPeople;
            this.timeCoefficient = timeCoefficient;
            this.age = age;
            this.happiness = happiness;
            this.momentaryHappiness = momentaryHappiness;
            this.enjoyments = enjoyments;
            this.talents = talents;
            this.skills = skills;
        }

        public UDouble ActualTotalSkill(IndustryType industryType)
            => totalNumPeople.value * (UDouble)Score.WeightedAverageOfTwo
            (
                score1: happiness,
                score2: skills[industryType],
                score1Propor: CurWorldConfig.actualSkillHappinessWeight
            );

        public RealPeopleStats CombineWith(RealPeopleStats other)
        {
            if (totalNumPeople + other.totalNumPeople == NumPeople.zero)
                return empty;

            var current = this;

            return new
            (
                totalMass: totalMass + other.totalMass,
                totalNumPeople: totalNumPeople + other.totalNumPeople,
                timeCoefficient: Propor.Create
                (
                    part: totalNumPeople.value * timeCoefficient + other.totalNumPeople.value * other.timeCoefficient,
                    whole: totalNumPeople.value + other.totalNumPeople.value
                )!.Value,
                age: (totalNumPeople.value * age + other.totalNumPeople.value * other.age) / (totalNumPeople.value + other.totalNumPeople.value),
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
                timeCoefficient: Propor.Create
                (
                    part: totalNumPeople.value * timeCoefficient - other.totalNumPeople.value * other.timeCoefficient,
                    whole: totalNumPeople.value - other.totalNumPeople.value
                )!.Value,
                age: MyMathHelper.Max(TimeSpan.Zero, (totalNumPeople.value * age - other.totalNumPeople.value * other.age) / (totalNumPeople.value - other.totalNumPeople.value)),
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
