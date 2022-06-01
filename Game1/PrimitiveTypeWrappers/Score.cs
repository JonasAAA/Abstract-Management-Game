namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly struct Score : IClose<Score>, IComparable<Score>, IPrimitiveTypeWrapper
    {
        public static readonly Score highest = new(value: 1);
        public static readonly Score lowest = new(value: 0);

        public static Score? Create(double value)
        {
            if (value < 0 && MyMathHelper.AreClose(value, 0))
                value = 0;
            if (value > 1 && MyMathHelper.AreClose(value, 1))
                value = 1;
            if (value is >= 0 and <= 1)
                return new(value: value);
            return null;
        }

        private readonly UDouble value;

        private Score(double value)
        {
            Debug.Assert(value is >= 0 and <= 1);
            this.value = (UDouble)value;
        }

        public bool IsCloseTo(Score other)
            => MyMathHelper.AreClose(value, other.value);

        public Score Opposite()
            => (Score)(1 - value);

        public static Score FromUnboundedUDouble(UDouble value, UDouble valueGettingAverageScore)
        {
            if (valueGettingAverageScore.IsCloseTo(other: 0))
                throw new ArgumentException();
            return (Score)MyMathHelper.Tanh(value / valueGettingAverageScore * MyMathHelper.Atanh((Propor).5));
        }

        public static Score GenerateRandom()
            => (Score)C.Random(min: (double)0, max: 1);

        public static Score WeightedAverage(params (ulong weight, Score score)[] weightsAndScores)
            => (Score)(weightsAndScores.Sum(weightAndScore => weightAndScore.weight * (UDouble)weightAndScore.score) / weightsAndScores.Sum(weightAndScore => weightAndScore.weight));

        public static Score WightedAverageOfTwo(Score score1, Score score2, Propor score1Propor)
            => (Score)((UDouble)score1 * score1Propor + (UDouble)score2 * score1Propor.Opposite());

        public static Score Average(params Score[] scores)
            => WeightedAverage
            (
                weightsAndScores:
                    (from score in scores
                     select (weight: 1UL, score: score)).ToArray()
            );

        // Assuming target is constant, this method is frame-rate independent
        public static Score BringCloser(Score current, Score target, TimeSpan elapsed, TimeSpan halvingDifferenceDuration)
        {
            if (elapsed < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();
            if (halvingDifferenceDuration <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();

            if (elapsed == TimeSpan.Zero)
                return current;

            return WightedAverageOfTwo
            (
                score1: current,
                score2: target,
                score1Propor: MyMathHelper.Pow(@base: (Propor).5, exponent: (UDouble)(elapsed / halvingDifferenceDuration))
            );
        }

        public static explicit operator Score(double value)
            => Create(value: value) switch
            {
                Score score => score,
                null => throw new InvalidCastException()
            };

        public static explicit operator Score(UDouble value)
            => (Score)(double)value;

        public static explicit operator Score(Propor propor)
            => new(value: (double)propor);

        public static explicit operator UDouble(Score score)
            => score.value;

        public static explicit operator double(Score score)
            => score.value;

        public static bool operator <=(Score score1, Score score2)
            => score1.value <= score2.value;

        public static bool operator >=(Score score1, Score score2)
            => score1.value >= score2.value;

        int IComparable<Score>.CompareTo(Score other)
            => ((double)this).CompareTo((double)other);

        public string ToString(string? format, IFormatProvider? formatProvider)
            => $"score {value.ToString(format, formatProvider)}";
    }
}
