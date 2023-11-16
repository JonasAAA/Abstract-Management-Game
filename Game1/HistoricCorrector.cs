using System.Numerics;

namespace Game1
{
    [Serializable]
    public readonly struct HistoricCorrectorWithTarget<TValue>(HistoricCorrector<TValue> historicCorrector, TValue target)
        where TValue : IAdditiveIdentity<TValue, TValue>, IAdditionOperators<TValue, TValue, TValue>, ISubtractionOperators<TValue, TValue, TValue>
    {
        public readonly TValue suggestion = target + historicCorrector.historicalInaccuracy;

        public HistoricCorrector<TValue> WithValue(TValue value)
            => new(historicCorrectorWithTarget: this, value: value);
    }

    [Serializable]
    public readonly struct HistoricCorrector<TValue>(HistoricCorrectorWithTarget<TValue> historicCorrectorWithTarget, TValue value)
        where TValue : IAdditiveIdentity<TValue, TValue>, IAdditionOperators<TValue, TValue, TValue>, ISubtractionOperators<TValue, TValue, TValue>
    {
        public readonly TValue historicalInaccuracy = historicCorrectorWithTarget.suggestion - value;

        public HistoricCorrectorWithTarget<TValue> WithTarget(TValue target)
            => new(historicCorrector: this, target: target);
    }
}
