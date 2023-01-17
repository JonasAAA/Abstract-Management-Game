using System.Numerics;

namespace Game1
{
    public static class Algorithms
    {
        // THIS IS A CLASS so that when it's later changed, it the changes are reflected everywhere
        [Serializable]
        private record class ConsumerWithEnergy<T>(T ReqEnergy, T AllocEnergy) : IComparable<ConsumerWithEnergy<T>>
            where T : struct, IUnconstrainedEnergy<T>
        {
            public T AllocEnergy { get; set; } = AllocEnergy;

            // This compares AllocEnergy / ReqWatts (real number result) between the two
            // and if ReqWatts is 0, then that energy consumer will be considered big, i.e. at the end of the list
            public int CompareTo(ConsumerWithEnergy<T>? other)
                => (ReqEnergy.IsZero, other!.ReqEnergy.IsZero) switch
                {
                    (true, true) => 0,
                    (true, false) => 1,
                    (false, true) => -1,
                    (false, false) => ((UInt128)AllocEnergy.ValueInJ() * other.ReqEnergy.ValueInJ()).CompareTo
                        (
                            (UInt128)other.AllocEnergy.ValueInJ() * ReqEnergy.ValueInJ()
                        )
                };
        }

        public static (List<T> allocatedEnergies, T unusedEnergy) SplitEnergyEvenly<T>(List<T> reqEnergies, T availableEnergy)
            where T : struct, IUnconstrainedEnergy<T>, IComparisonOperators<T, T, bool>
        {
            var totalReqEnergy = reqEnergies.Sum();
            if (totalReqEnergy <= availableEnergy)
                return
                (
                    allocatedEnergies: reqEnergies.ToList(),
                    unusedEnergy: availableEnergy - totalReqEnergy
                );
            if (availableEnergy.IsZero)
                return
                (
                    allocatedEnergies: reqEnergies.Select(reqEnergy => T.AdditiveIdentity).ToList(),
                    unusedEnergy: T.AdditiveIdentity
                );

            Debug.Assert(!totalReqEnergy.IsZero);
            List<ConsumerWithEnergy<T>> consumersWithEnergy = reqEnergies.Select
            (
                reqEnergy => new ConsumerWithEnergy<T>
                (
                    ReqEnergy: reqEnergy,
                    AllocEnergy: IUnconstrainedEnergy<T>.CreateFromJoules
                    (
                        valueInJ: (ulong)((UInt128)reqEnergy.ValueInJ() * availableEnergy.ValueInJ() / totalReqEnergy.ValueInJ())
                    )
                )
            ).ToList();
            consumersWithEnergy.Sort();
            var remainingEnergy = availableEnergy - consumersWithEnergy.Sum(energyConsumer => energyConsumer.AllocEnergy);
            // Give the remaining energy to those that got the least of it.
            for (int i = 0; i < (int)remainingEnergy.ValueInJ(); i++)
                consumersWithEnergy[i].AllocEnergy += IUnconstrainedEnergy<T>.CreateFromJoules(valueInJ: 1);
            remainingEnergy = T.AdditiveIdentity;
            return
            (
                allocatedEnergies: consumersWithEnergy.Select(consumer => consumer.AllocEnergy).ToList(),
                unusedEnergy: T.AdditiveIdentity
            );
        }

        private enum Size
        {
            TooBig,
            TooSmall
        }

        // THIS IS A CLASS so that when it's later changed, it the changes are reflected everywhere
        [Serializable]
        private record class ConsumerWithExtraEnergy<T>(T OwnedEnergy, T ReqEnergy, T AllocEnergy) : ConsumerWithEnergy<T>(ReqEnergy: ReqEnergy, AllocEnergy: AllocEnergy), IComparable<ConsumerWithExtraEnergy<T>>
            where T : struct, IUnconstrainedEnergy<T>
        {
            public int CompareTo(ConsumerWithExtraEnergy<T>? other)
                => base.CompareTo(other: other);
        }

        public static (List<T> allocatedEnergies, T unusedEnergy) SplitExtraEnergyEvenly<T>(List<(T ownedEnergy, T reqEnergy)> energies, T availableEnergy)
            where T : struct, IUnconstrainedEnergy<T>, IComparisonOperators<T, T, bool>
        {
            // TEST this method
            throw new NotImplementedException();
            if (energies.All(energy => energy.ownedEnergy.IsZero))
                return SplitEnergyEvenly
                (
                    reqEnergies: energies.Select(energy => energy.reqEnergy).ToList(),
                    availableEnergy: availableEnergy
                );

            List<T> allocatedEnergies = SplitExtraEnergyEvenlyInternal();
            return (allocatedEnergies, unusedEnergy: availableEnergy - allocatedEnergies.Sum());

            List<T> SplitExtraEnergyEvenlyInternal()
            {
                List<T> maxAllocEnergies = energies.Select(energy => energy.reqEnergy - energy.ownedEnergy).ToList();
                if (maxAllocEnergies.Sum() <= availableEnergy)
                    return maxAllocEnergies;

                if (availableEnergy.IsZero)
                    return energies.Select(reqEnergy => T.AdditiveIdentity).ToList();

                decimal minAllocPropor = 0, maxAllocPropor = 1;
                while (true)
                {
                    decimal mediumAllocPropor = (minAllocPropor + maxAllocPropor) / 2;
                    var allocEnergies = TryAllocPropor(allocPropor: mediumAllocPropor).SwitchExpression<List<T>?>
                    (
                        case1: allocEnergies => allocEnergies,
                        case2: size =>
                        {
                            switch (size)
                            {
                                case Size.TooBig:
                                    maxAllocPropor = mediumAllocPropor;
                                    break;
                                case Size.TooSmall:
                                    minAllocPropor = mediumAllocPropor;
                                    break;
                                default:
                                    Debug.Assert(false);
                                    break;
                            }
                            return null;
                        }
                    );
                    if (allocEnergies is not null)
                        return allocEnergies;
                }
            }

            // If allocPropor is good enough, returns the list of allocated energies.
            // Otherwise returns
            GeneralEnum<List<T>, Size> TryAllocPropor(decimal allocPropor)
            {
                List<ConsumerWithExtraEnergy<T>> consumersWithExtraEnergy = energies.Select
                (
                    energy => new ConsumerWithExtraEnergy<T>
                    (
                        OwnedEnergy: energy.ownedEnergy,
                        ReqEnergy: energy.reqEnergy,
                        AllocEnergy: IUnconstrainedEnergy<T>.CreateFromJoules
                        (
                            valueInJ: (ulong)MyMathHelper.Max
                            (
                                0,
                                MyMathHelper.Ceiling
                                (
                                    energy.reqEnergy.ValueInJ() * allocPropor - energy.ownedEnergy.ValueInJ()
                                )
                            )
                        )
                    )
                ).ToList();
                var totAllocEnergies = consumersWithExtraEnergy.Sum(consumer => consumer.AllocEnergy);
                if (totAllocEnergies > availableEnergy)
                    return new(value2: Size.TooBig);
                var remainingEnergyInJ = availableEnergy.ValueInJ() - totAllocEnergies.ValueInJ();
                if (remainingEnergyInJ > (ulong)energies.Count)
                    return new(value2: Size.TooSmall);
                SortedSet<ConsumerWithExtraEnergy<T>> sortedConsumersWithExtraEnergy = new();
                while (remainingEnergyInJ > 0)
                {
                    var consumer = sortedConsumersWithExtraEnergy.Min!;
                    // Remove, then change, then put the element back in, as otherwise the collection won't be resorted
                    sortedConsumersWithExtraEnergy.Remove(consumer);
                    consumer.AllocEnergy += IUnconstrainedEnergy<T>.CreateFromJoules(valueInJ: 1);
                    remainingEnergyInJ--;
                    sortedConsumersWithExtraEnergy.Add(consumer);
                }
                return new(value1: consumersWithExtraEnergy.Select(consumer => consumer.AllocEnergy).ToList());
            }
        }
    }
}
