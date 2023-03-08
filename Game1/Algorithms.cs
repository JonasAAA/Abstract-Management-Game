using System.Numerics;

namespace Game1
{
    public static class Algorithms
    {
        // THIS IS A CLASS so that when it's later changed, it the changes are reflected everywhere
        // Index is here only to resolve the ties when comparing elements.
        // That is needed because SortedSet doesn't allow duplicates (i.e. elements which return 0 when compared)
        [Serializable]
        private record class ConsumerWithEnergy<T>(int Index, T ReqEnergy, T AllocEnergy) : IComparable<ConsumerWithEnergy<T>>
            where T : struct, IUnconstrainedEnergy<T>
        {
            public T AllocEnergy { get; set; } = AllocEnergy;

            // TODO: could randomise this a little so that the same things don't get consistently more energy
            // The benefit would be miniscule, probabbly unnoticeable
            int IComparable<ConsumerWithEnergy<T>>.CompareTo(ConsumerWithEnergy<T>? other)
                => Compare
                (
                    left: (index: Index, totOwnedEnergy: AllocEnergy, reqEnergy: ReqEnergy),
                    right: (index: other!.Index, totOwnedEnergy: other.AllocEnergy, reqEnergy: other.ReqEnergy)
                );
        }

        // This compares totOwnedEnergy / reqEnergy (real number result) between the two
        // and if reqEnergy is 0, then that energy consumer will be considered big, i.e. at the end of the list
        private static int Compare<T>((int index, T totOwnedEnergy, T reqEnergy) left, (int index, T totOwnedEnergy, T reqEnergy) right)
            where T : struct, IUnconstrainedEnergy<T>
        {
            int compareValues = (left.reqEnergy.IsZero, right.reqEnergy.IsZero) switch
            {
                (true, true) => 0,
                (true, false) => 1,
                (false, true) => -1,
                (false, false) => ((UInt128)left.totOwnedEnergy.ValueInJ() * right.reqEnergy.ValueInJ()).CompareTo
                    (
                        (UInt128)right.totOwnedEnergy.ValueInJ() * left.reqEnergy.ValueInJ()
                    )
            };
            return compareValues is 0 ? left.index.CompareTo(right.index) : compareValues;
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
            ReadOnlyCollection<ConsumerWithEnergy<T>> consumersWithEnergy = new
            (
                reqEnergies.Select
                (
                    (reqEnergy, index) => new ConsumerWithEnergy<T>
                    (
                        Index: index,
                        ReqEnergy: reqEnergy,
                        AllocEnergy: IUnconstrainedEnergy<T>.CreateFromJoules
                        (
                            valueInJ: (ulong)((UInt128)reqEnergy.ValueInJ() * availableEnergy.ValueInJ() / totalReqEnergy.ValueInJ())
                        )
                    )
                ).ToList()
            );
            var sortedConsumersWithEnergy = consumersWithEnergy.Order().ToList();
            var remainingEnergy = availableEnergy - sortedConsumersWithEnergy.Sum(energyConsumer => energyConsumer.AllocEnergy);
            // Give the remaining energy to those that got the least of it.
            for (int i = 0; i < (int)remainingEnergy.ValueInJ(); i++)
                sortedConsumersWithEnergy[i].AllocEnergy += IUnconstrainedEnergy<T>.CreateFromJoules(valueInJ: 1);
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
        private record class ConsumerWithExtraEnergy<T>(int Index, T OwnedEnergy, T ReqEnergy, T AllocEnergy) : ConsumerWithEnergy<T>(Index: Index, ReqEnergy: ReqEnergy, AllocEnergy: AllocEnergy), IComparable<ConsumerWithExtraEnergy<T>>
            where T : struct, IUnconstrainedEnergy<T>
        {
            int IComparable<ConsumerWithExtraEnergy<T>>.CompareTo(ConsumerWithExtraEnergy<T>? other)
                => Compare
                (
                    left: (index: Index, totOwnedEnergy: OwnedEnergy + AllocEnergy, reqEnergy: ReqEnergy),
                    right: (index: other!.Index, totOwnedEnergy: other.OwnedEnergy + other.AllocEnergy, reqEnergy: other.ReqEnergy)
                );
        }

        public static (List<T> allocatedEnergies, T unusedEnergy) SplitExtraEnergyEvenly<T>(List<(T ownedEnergy, T reqEnergy)> energies, T availableEnergy)
            where T : struct, IUnconstrainedEnergy<T>, IComparisonOperators<T, T, bool>
        {
            if (energies.All(energy => energy.ownedEnergy.IsZero))
                return SplitEnergyEvenly
                (
                    reqEnergies: energies.Select(energy => energy.reqEnergy).ToList(),
                    availableEnergy: availableEnergy
                );

            List<T> allocatedEnergies = SplitExtraEnergyEvenlyInternal();
            Debug.Assert(energies.Zip(allocatedEnergies).All(energy => energy.First.ownedEnergy + energy.Second <= energy.First.reqEnergy));
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
                ReadOnlyCollection<ConsumerWithExtraEnergy<T>> consumersWithExtraEnergy = new
                (
                    energies.Select
                    (
                        (energy, index) => new ConsumerWithExtraEnergy<T>
                        (
                            Index: index,
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
                    ).ToList()
                );
                var totAllocEnergies = consumersWithExtraEnergy.Sum(consumer => consumer.AllocEnergy);
                if (totAllocEnergies > availableEnergy)
                    return new(value2: Size.TooBig);
                var remainingEnergyInJ = availableEnergy.ValueInJ() - totAllocEnergies.ValueInJ();
                if (remainingEnergyInJ > (ulong)energies.Count)
                    return new(value2: Size.TooSmall);
                SortedSet<ConsumerWithExtraEnergy<T>> sortedConsumersWithExtraEnergy = new(consumersWithExtraEnergy);
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

        public static ulong MatterToConvertToEnergy(BasicRes basicRes, ulong resAmount, UDouble temperatureInK, UDouble surfaceGravity, TimeSpan duration,
            Func<decimal, ulong> massInKgRoundFunc, UDouble reactionStrengthCoeff, Propor nonConvertedMassForUnitReactionStrengthUnitTime)
        {
#warning test this
            double density = (double)basicRes.Area / basicRes.Mass.valueInKg,
                reactionStrength = reactionStrengthCoeff * density * surfaceGravity * temperatureInK;
            var nonConvertedMassPropor = (decimal)MyMathHelper.Pow(@base: (UDouble)nonConvertedMassForUnitReactionStrengthUnitTime, exponent: reactionStrength * duration.TotalSeconds);
            ulong matterToConvert = resAmount - massInKgRoundFunc(nonConvertedMassPropor * resAmount);
            return matterToConvert;
        }

        /// <summary>
        /// Implements Stefan-Boltzmann law https://en.wikipedia.org/wiki/Stefan%E2%80%93Boltzmann_law to calculate how much energy in total to dissipate
        /// The splitting into heat energy and radiant energy algorithm is my creation
        /// </summary>
        public static (HeatEnergy heatEnergy, RadiantEnergy radiantEnergy) EnergiesToDissipate(HeatEnergy heatEnergy, UDouble surfaceLength, Propor emissivity, UDouble temperatureInK,
            Func<decimal, ulong> energyInJToDissipateRoundFunc, UDouble stefanBoltzmannConstant, ulong temperatureExponent, Func<decimal, ulong> heatEnergyInJRoundFunc,
            UDouble allHeatMaxTemper, UDouble halfHeatTemper, UDouble heatEnergyDropoffExponent)
        {
#warning test this
            ulong energyInJToDissipate = MyMathHelper.Min
            (
                heatEnergy.ValueInJ,
                energyInJToDissipateRoundFunc((decimal)(surfaceLength * emissivity * stefanBoltzmannConstant * MyMathHelper.Pow(@base: temperatureInK, exponent: temperatureExponent)))
            );
            double heatEnergyPropor = (temperatureInK <= allHeatMaxTemper) switch
            {
                true => 1,
                false => 1 / (1 + MyMathHelper.Pow(@base: (temperatureInK - allHeatMaxTemper) / (halfHeatTemper - allHeatMaxTemper), exponent: heatEnergyDropoffExponent))
            };

            ulong heatEnergyInJ = heatEnergyInJRoundFunc(energyInJToDissipate * (decimal)heatEnergyPropor);
            return
            (
                heatEnergy: HeatEnergy.CreateFromJoules(valueInJ: heatEnergyInJ),
                radiantEnergy: RadiantEnergy.CreateFromJoules(valueInJ: energyInJToDissipate - heatEnergyInJ)
            );
        }
    }
}
