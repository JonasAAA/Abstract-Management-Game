using System.Numerics;

namespace Game1
{
    public static class Algorithms
    {
        [Serializable]
        private readonly record struct ConsumerWithEnergy<T>(int EnergyConsumerID, T ReqEnergy, T Energy);

        public static List<T> SplitEnergyEvenly<T>(List<T> reqEnergies, T availableEnergy)
            where T : struct, IUnconstrainedEnergy<T>, IComparisonOperators<T, T, bool>
        {
            var totalReqEnergy = reqEnergies.Sum();
            if (totalReqEnergy < availableEnergy)
                throw new ArgumentException();
            if (availableEnergy.IsZero)
                return reqEnergies.Select(reqEnergy => T.AdditiveIdentity).ToList();

            Debug.Assert(!totalReqEnergy.IsZero);
            List<ConsumerWithEnergy<T>> energySplit = reqEnergies.Select
            (
                (reqEnergy, energyConsumerID) => new ConsumerWithEnergy<T>
                (
                    EnergyConsumerID: energyConsumerID,
                    ReqEnergy: reqEnergy,
                    Energy: IUnconstrainedEnergy<T>.CreateFromJoules
                    (
                        valueInJ: (ulong)((UInt128)reqEnergy.ValueInJ() * availableEnergy.ValueInJ() / totalReqEnergy.ValueInJ())
                    )
                )
            ).ToList();
            energySplit.Sort
            (
                // This compares Energy / ReqEnergy (real number result) between the two
                // and if ReqEnergy is 0, then that energy consumer will be considered big, i.e. at the end of the list
                comparison: (left, right)
                    => (left.ReqEnergy.IsZero, right.ReqEnergy.IsZero) switch
                    {
                        (true, true) => 0,
                        (true, false) => 1,
                        (false, true) => -1,
                        (false, false) => ((UInt128)left.Energy.ValueInJ() * right.ReqEnergy.ValueInJ()).CompareTo
                            (
                                (UInt128)right.Energy.ValueInJ() * left.ReqEnergy.ValueInJ()
                            )
                    }
            );
            var remainingEnergy = availableEnergy - energySplit.Sum(energyConsumer => energyConsumer.Energy);
            // Give the remaining energy to those that got the least of it.
            for (int i = 0; i < (int)remainingEnergy.ValueInJ(); i++)
                energySplit[i] = energySplit[i] with
                {
                    Energy = energySplit[i].Energy + IUnconstrainedEnergy<T>.CreateFromJoules(valueInJ: 1)
                };
            List<T> result = reqEnergies.Select(reqEnergy => default(T)).ToList();
            foreach (var (consumerID, _, energy) in energySplit)
                result[consumerID] = energy;
            Debug.Assert(result.Count == reqEnergies.Count);
            Debug.Assert(result.Sum() == availableEnergy);
            return result;
        }
    }
}
