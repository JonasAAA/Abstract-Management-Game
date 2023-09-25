using System.Numerics;

namespace Game1
{
    [Serializable]
    public readonly struct HistoricCorrectorWithTarget<TValue>
        where TValue : IAdditiveIdentity<TValue, TValue>, IAdditionOperators<TValue, TValue, TValue>, ISubtractionOperators<TValue, TValue, TValue>
    {
        public readonly TValue suggestion;

        public HistoricCorrectorWithTarget(HistoricCorrector<TValue> historicCorrector, TValue target)
            => suggestion = target + historicCorrector.historicalInaccuracy;

        public HistoricCorrector<TValue> WithValue(TValue value)
            => new(historicCorrectorWithTarget: this, value: value);
    }

    [Serializable]
    public readonly struct HistoricCorrector<TValue>
        where TValue : IAdditiveIdentity<TValue, TValue>, IAdditionOperators<TValue, TValue, TValue>, ISubtractionOperators<TValue, TValue, TValue>
    {
        public readonly TValue historicalInaccuracy;

        public HistoricCorrector()
            => historicalInaccuracy = TValue.AdditiveIdentity;

        public HistoricCorrector(HistoricCorrectorWithTarget<TValue> historicCorrectorWithTarget, TValue value)
            => historicalInaccuracy = historicCorrectorWithTarget.suggestion - value;

        public HistoricCorrectorWithTarget<TValue> WithTarget(TValue target)
            => new(historicCorrector: this, target: target);
    }
}
