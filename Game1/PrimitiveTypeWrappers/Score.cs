using Game1.Collections;
using Game1.Industries;
using System.Numerics;

namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly struct Score : IClose<Score>, IComparable<Score>, IComparisonOperators<Score, Score, bool>, IPrimitiveTypeWrapper
    {
        [Serializable]
        public readonly struct ParamsOfChange
        {
            public readonly Score target;
            public readonly TimeSpan elapsed, halvingDifferenceDuration;

            public ParamsOfChange(Score target, TimeSpan elapsed, TimeSpan halvingDifferenceDuration)
            {
                this.target = target;

                if (elapsed < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException();
                this.elapsed = elapsed;

                if (halvingDifferenceDuration <= TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException();
                this.halvingDifferenceDuration = halvingDifferenceDuration;
            }
        }

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

        public static Score? Create(UDouble value)
            => Create(value: (double)value);

        public static Score Create(Propor propor)
            => new(value: (double)propor);

        public static Score CreateOrThrow(double value)
            => Create(value: value) switch
            {
                Score score => score,
                null => throw new ArgumentException()
            };

        public static Score? CreateOrThrow(UDouble value)
            => CreateOrThrow(value: (double)value);

        private readonly UDouble value;

        private Score(double value)
        {
            Debug.Assert(value is >= 0 and <= 1);
            this.value = (UDouble)value;
        }

        public bool IsCloseTo(Score other)
            => MyMathHelper.AreClose(value, other.value);

        public Score Opposite()
            => Create(value: 1 - value)!.Value;

        public static Score FromUnboundedUDouble(UDouble value, UDouble valueGettingAverageScore)
        {
            if (valueGettingAverageScore.IsCloseTo(other: 0))
                throw new ArgumentException();
            return Create(propor: MyMathHelper.Tanh(value / valueGettingAverageScore * MyMathHelper.Atanh((Propor).5)));
        }

        public static Score GenerateRandom()
            => new(value: C.Random(min: (double)0, max: 1));

        public static Score WeightedAverage(params (ulong weight, Score score)[] weightsAndScores)
        {
            ulong weightSum = 0;
            UDouble weightedScoreSum = 0;
            foreach (var (weight, score) in weightsAndScores)
            {
                weightSum += weight;
                weightedScoreSum += weight * score.value;
            }
            if (weightSum is 0)
                return new(value: 0);
            return GetScoreFromNotNull(Create(value: weightedScoreSum / weightSum));
        }

        public static Score WeightedAverageWithPossiblyNegativeWeights(params (long weight, Score score)[] weightsAndScores)
        {
            long weightSum = 0;
            double weightedScoreSum = 0;
            foreach (var (weight, score) in weightsAndScores)
            {
                weightSum += weight;
                weightedScoreSum += weight * score.value;
            }
            return CreateOrThrow(value: weightedScoreSum / weightSum);
        }

        public static Score WeightedAverageOfTwo(Score score1, Score score2, Propor score1Propor)
            => GetScoreFromNotNull
            (
                Create
                (
                    value: (UDouble)score1 * score1Propor + (UDouble)score2 * score1Propor.Opposite()
                )
            );

        private static Score GetScoreFromNotNull(Score? score)
        {
            Debug.Assert(score is not null);
            return score.Value;
        }

        public static Score Average(params Score[] scores)
            => WeightedAverage
            (
                weightsAndScores:
                    (from score in scores
                     select (weight: 1UL, score: score)).ToArray()
            );

        // Assuming target is constant, this method is frame-rate independent
        public static Score BringCloser(Score current, ParamsOfChange paramsOfChange)
        {
            if (paramsOfChange.elapsed == TimeSpan.Zero)
                return current;

            return WeightedAverageOfTwo
            (
                score1: current,
                score2: paramsOfChange.target,
                score1Propor: MyMathHelper.Pow(@base: (Propor).5, exponent: (UDouble)(paramsOfChange.elapsed / paramsOfChange.halvingDifferenceDuration))
            );
        }

        public static EnumDict<IndustryType, Score> ScaleToHaveHighestAndLowest(EnumDict<IndustryType, Score> scores)
        {
            double highestScore = scores.Values.Max().value,
                lowestScore = scores.Values.Min().value;
            if (MyMathHelper.AreClose(highestScore, lowestScore))
            {
                Debug.Fail("Enjoyments shouldn't all be basically the same");
                return scores;
            }
            return new(selector: industryType => CreateOrThrow((scores[industryType].value - lowestScore) / (highestScore - lowestScore)));
        }

        public static explicit operator UDouble(Score score)
            => score.value;

        public static explicit operator double(Score score)
            => score.value;

        public static bool operator >(Score left, Score right)
            => left.value > right.value;

        public static bool operator >=(Score left, Score right)
            => left.value >= right.value;

        public static bool operator <(Score left, Score right)
            => left.value < right.value;

        public static bool operator <=(Score left, Score right)
            => left.value <= right.value;

        public static bool operator ==(Score left, Score right)
            => left.value == right.value;

        public static bool operator !=(Score left, Score right)
            => left.value != right.value;

        int IComparable<Score>.CompareTo(Score other)
            => ((double)this).CompareTo((double)other);

        public string ToString(string? format, IFormatProvider? formatProvider)
            => value.ToString(format, formatProvider);

        public override bool Equals(object? obj)
            => obj is Score score && value == score.value;

        public override int GetHashCode()
            => value.GetHashCode();
    }
}
