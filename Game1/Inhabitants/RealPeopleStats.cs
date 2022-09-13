namespace Game1.Inhabitants
{
    [Serializable]
    public readonly record struct RealPeopleStats(Mass Mass, NumPeople NumPeople, Propor AverageTimeCoefficient, TimeSpan AverageAge, Score AverageHappiness, Score AverageMomentaryHappiness)
    {
        public static readonly RealPeopleStats empty;

        static RealPeopleStats()
            => empty = new
            (
                Mass: Mass.zero,
                NumPeople: NumPeople.zero,
                AverageTimeCoefficient: Propor.empty,
                AverageAge: TimeSpan.Zero,
                AverageHappiness: Score.lowest,
                AverageMomentaryHappiness: Score.lowest
            );

        public bool IsEmpty
            => NumPeople.IsZero;

        public RealPeopleStats CombineWith(RealPeopleStats other)
        {
            if (NumPeople + other.NumPeople == NumPeople.zero)
                return empty;
            return new
            (
                Mass: Mass + other.Mass,
                NumPeople: NumPeople + other.NumPeople,
                AverageTimeCoefficient: Propor.Create
                (
                    part: NumPeople.value * AverageTimeCoefficient + other.NumPeople.value * other.AverageTimeCoefficient,
                    whole: NumPeople.value + other.NumPeople.value
                )!.Value,
                AverageAge: (NumPeople.value * AverageAge + other.NumPeople.value * other.AverageAge) / (NumPeople.value + other.NumPeople.value),
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
        }

        public RealPeopleStats Subtract(RealPeopleStats other)
        {
            if (NumPeople == other.NumPeople)
                return empty;
            return new
            (
                Mass: Mass - other.Mass,
                NumPeople: NumPeople - other.NumPeople,
                AverageTimeCoefficient: Propor.Create
                (
                    part: NumPeople.value * AverageTimeCoefficient - other.NumPeople.value * other.AverageTimeCoefficient,
                    whole: NumPeople.value - other.NumPeople.value
                )!.Value,
                AverageAge: MyMathHelper.Max(TimeSpan.Zero, (NumPeople.value * AverageAge - other.NumPeople.value * other.AverageAge) / (NumPeople.value - other.NumPeople.value)),
                AverageHappiness: Score.WeightedAverageWithPossiblyNegativeWeights
                (
                    (weight: (long)NumPeople.value, score: AverageHappiness),
                    (weight: -(long)other.NumPeople.value, score: other.AverageHappiness)
                ),
                AverageMomentaryHappiness: Score.WeightedAverageWithPossiblyNegativeWeights
                (
                    (weight: (long)NumPeople.value, score: AverageMomentaryHappiness),
                    (weight: -(long)other.NumPeople.value, score: other.AverageMomentaryHappiness)
                )
            );
        }

        public string HappinessStats()
            => NumPeople.IsZero switch
            {
                true => "no happiness stats as no\npeople are here\n",
                false => $"average happiness {AverageHappiness:0.00}\naverage momentary happiness {AverageMomentaryHappiness:0.00}\n"
            };

        public override string ToString()
            => $"Number of people {NumPeople}\naverage time coefficient {AverageTimeCoefficient}\naverage age {AverageAge}\n{HappinessStats()}";
    }
}
