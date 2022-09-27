namespace Game1.Inhabitants
{
    [Serializable]
    public readonly record struct RealPeopleStats(Mass TotalMass, NumPeople TotalNumPeople, Propor TimeCoefficient, TimeSpan Age, Score Happiness, Score MomentaryHappiness)
    {
        public static readonly RealPeopleStats empty;

        static RealPeopleStats()
            => empty = new
            (
                TotalMass: Mass.zero,
                TotalNumPeople: NumPeople.zero,
                TimeCoefficient: Propor.empty,
                Age: TimeSpan.Zero,
                Happiness: Score.lowest,
                MomentaryHappiness: Score.lowest
            );

        public bool IsEmpty
            => TotalNumPeople.IsZero;

        public RealPeopleStats CombineWith(RealPeopleStats other)
        {
            if (TotalNumPeople + other.TotalNumPeople == NumPeople.zero)
                return empty;
            return new
            (
                TotalMass: TotalMass + other.TotalMass,
                TotalNumPeople: TotalNumPeople + other.TotalNumPeople,
                TimeCoefficient: Propor.Create
                (
                    part: TotalNumPeople.value * TimeCoefficient + other.TotalNumPeople.value * other.TimeCoefficient,
                    whole: TotalNumPeople.value + other.TotalNumPeople.value
                )!.Value,
                Age: (TotalNumPeople.value * Age + other.TotalNumPeople.value * other.Age) / (TotalNumPeople.value + other.TotalNumPeople.value),
                Happiness: Score.WeightedAverage
                (
                    (weight: TotalNumPeople.value, score: Happiness),
                    (weight: other.TotalNumPeople.value, score: other.Happiness)
                ),
                MomentaryHappiness: Score.WeightedAverage
                (
                    (weight: TotalNumPeople.value, score: MomentaryHappiness),
                    (weight: other.TotalNumPeople.value, score: other.MomentaryHappiness)
                )
            );
        }

        public RealPeopleStats Subtract(RealPeopleStats other)
        {
            if (TotalNumPeople == other.TotalNumPeople)
                return empty;
            return new
            (
                TotalMass: TotalMass - other.TotalMass,
                TotalNumPeople: TotalNumPeople - other.TotalNumPeople,
                TimeCoefficient: Propor.Create
                (
                    part: TotalNumPeople.value * TimeCoefficient - other.TotalNumPeople.value * other.TimeCoefficient,
                    whole: TotalNumPeople.value - other.TotalNumPeople.value
                )!.Value,
                Age: MyMathHelper.Max(TimeSpan.Zero, (TotalNumPeople.value * Age - other.TotalNumPeople.value * other.Age) / (TotalNumPeople.value - other.TotalNumPeople.value)),
                Happiness: Score.WeightedAverageWithPossiblyNegativeWeights
                (
                    (weight: (long)TotalNumPeople.value, score: Happiness),
                    (weight: -(long)other.TotalNumPeople.value, score: other.Happiness)
                ),
                MomentaryHappiness: Score.WeightedAverageWithPossiblyNegativeWeights
                (
                    (weight: (long)TotalNumPeople.value, score: MomentaryHappiness),
                    (weight: -(long)other.TotalNumPeople.value, score: other.MomentaryHappiness)
                )
            );
        }

        public override string ToString()
            => IsEmpty switch
            {
                true => "No people are here",
                false => $"Number of people {TotalNumPeople}\naverage time coefficient {TimeCoefficient}\naverage age {Age}\naverage happiness {Happiness:0.00}\naverage momentary happiness {MomentaryHappiness:0.00}\n"
            };
    }
}
