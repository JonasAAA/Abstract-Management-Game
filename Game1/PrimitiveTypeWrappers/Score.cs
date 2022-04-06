namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly struct Score : IClose<Score>, IComparable<Score>
    {
        // TODO: should probably rename those to highest and lowest
        public static readonly Score best = new(value: 1);
        public static readonly Score worst = new(value: 0);

        private readonly UDouble value;

        private Score(double value)
        {
            Debug.Assert(value is >= 0 and <= 1);
            this.value = (UDouble)value;
        }

        public bool IsCloseTo(Score other)
            => MyMathHelper.AreClose(value, other.value);

        public Score Opposite()
            => new(value: 1 - (double)value);

        public static Score FromUnboundedUDouble(UDouble value, UDouble valueGettingAverageScore)
        {
            if (valueGettingAverageScore.IsCloseTo(other: 0))
                throw new ArgumentException();
            return (Score)MyMathHelper.Tanh(value / valueGettingAverageScore * MyMathHelper.Atanh((Propor).5));
        }

        public static Score GenerateRandom()
            => new(value: C.Random(min: (double)0, max: 1));

        // TODO: could rename this to WeightedAverage
        public static Score Combine(params (ulong weight, Score score)[] weightsAndScores)
            => (Score)(weightsAndScores.Sum(weightAndScore => weightAndScore.weight * (UDouble)weightAndScore.score) / weightsAndScores.Sum(weightAndScore => weightAndScore.weight));

        public static Score CombineTwo(Score score1, Score score2, Propor score1Propor)
            => (Score)((UDouble)score1 * score1Propor + (UDouble)score2 * score1Propor.Opposite());

        public static Score Average(params Score[] scores)
            => Combine
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

            return CombineTwo
            (
                score1: current,
                score2: target,
                score1Propor: MyMathHelper.Pow(@base: (Propor).5, exponent: (UDouble)(elapsed / halvingDifferenceDuration))
            );
        }

        // TODO: give the checks some leeway : accept slightly negative and slightly above 1 values
        public static explicit operator Score(double value)
            => value switch
            {
                >= 0 and <= 1 => new(value: value),
                _ => throw new InvalidCastException()
            };

        public static explicit operator Score(UDouble value)
            => (Score)(double)value;

        public static explicit operator Score(Propor propor)
            => new(value: (double)propor);

        public static explicit operator UDouble(Score score)
            => score.value;

        public static explicit operator double(Score score)
            => (double)score.value;

        public static bool operator <=(Score score1, Score score2)
            => score1.value <= score2.value;

        public static bool operator >=(Score score1, Score score2)
            => score1.value >= score2.value;

        int IComparable<Score>.CompareTo(Score other)
            => ((double)this).CompareTo((double)other);
    }
}
