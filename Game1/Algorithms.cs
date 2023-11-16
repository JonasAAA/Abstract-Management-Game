using Game1.Collections;
using Priority_Queue;
using System.IO;
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
                (false, false) => MyMathHelper.CompareFractions
                (
                    numeratorA: left.totOwnedEnergy.ValueInJ(),
                    denominatorA: left.reqEnergy.ValueInJ(),
                    numeratorB: right.totOwnedEnergy.ValueInJ(),
                    denominatorB: right.reqEnergy.ValueInJ()
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
            EfficientReadOnlyCollection<ConsumerWithEnergy<T>> consumersWithEnergy = reqEnergies.Select
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
            ).ToEfficientReadOnlyCollection();
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
        private sealed record ConsumerWithExtraEnergy<T>(int Index, T OwnedEnergy, T ReqEnergy, T AllocEnergy) : ConsumerWithEnergy<T>(Index: Index, ReqEnergy: ReqEnergy, AllocEnergy: AllocEnergy), IComparable<ConsumerWithExtraEnergy<T>>
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
                var maxAllocEnergies = energies.Select(energy => energy.reqEnergy - energy.ownedEnergy).ToList();
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
                EfficientReadOnlyCollection<ConsumerWithExtraEnergy<T>> consumersWithExtraEnergy = new
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

        public static EfficientReadOnlyCollection<(TOwner owner, ulong amount)> Split<TOwner>(EfficientReadOnlyDictionary<TOwner, ulong> weights, ulong totalAmount)
            where TOwner : notnull
        {
#warning Test this
            ulong weightSum = weights.Sum(ownerAndWeight => ownerAndWeight.Value);
            var ownersToAmounts = weights.Select
            (
                ownerAndWeight =>
                (
                    owner: ownerAndWeight.Key,
                    amount: (ulong)((UInt128)totalAmount * ownerAndWeight.Value / weightSum)
                )
            ).ToList();
            ulong unusedAmount = totalAmount - ownersToAmounts.Sum(ownerAndAmount => ownerAndAmount.amount);
            Debug.Assert(unusedAmount <= (ulong)ownersToAmounts.Count);
            ownersToAmounts.Sort
            (
                comparison: (ownerAndAmount, otherOwnerAndAmount)
                    => MyMathHelper.CompareFractions
                    (
                        numeratorA: ownerAndAmount.amount,
                        denominatorA: weights[ownerAndAmount.owner],
                        numeratorB: otherOwnerAndAmount.amount,
                        denominatorB: weights[otherOwnerAndAmount.owner]
                    )
            );
            for (int i = 0; i < (int)unusedAmount; i++)
                ownersToAmounts[i] = (owner: ownersToAmounts[i].owner, amount: ownersToAmounts[i].amount + 1);
            Debug.Assert(ownersToAmounts.Sum(ownerAndAmount => ownerAndAmount.amount) == totalAmount);
            return ownersToAmounts.ToEfficientReadOnlyCollection();
        }

        // Inspired by https://en.wikipedia.org/wiki/Lawson_criterion#Energy_balance
        public static RawMatAmounts CosmicBodyNewCompositionFromNuclearFusion(ResConfig curResConfig, RawMatAmounts composition, SurfaceGravity surfaceGravity, UDouble surfaceGravityExponent,
            Temperature temperature, UDouble temperatureExponent, TimeSpan duration, UDouble fusionReactionStrengthCoeff)
        {
            AreaDouble compositionArea = composition.Area().ToDouble();
            Dictionary<RawMaterial, ulong> cosmicBodyNextComposition = new(2 * composition.Count);
            foreach (var (rawMaterial, amount) in composition)
            {
                (ulong nonReactingAmount, ulong fusionProductAmount) = NuclearFusionSingleRawMat
                (
                    amount: amount,
                    compositionArea: compositionArea,
                    surfaceGravity: surfaceGravity,
                    surfaceGravityExponent: surfaceGravityExponent,
                    temperature: temperature,
                    temperatureExponent: temperatureExponent,
                    duration: duration,
                    fusionReactionStrengthCoeff: fusionReactionStrengthCoeff * rawMaterial.FusionReactionStrengthCoeff
                );
                cosmicBodyNextComposition[rawMaterial] = cosmicBodyNextComposition.GetValueOrDefault(key: rawMaterial) + nonReactingAmount;
                if (fusionProductAmount > 0)
                {
                    RawMaterial fusionResult = rawMaterial.GetFusionResult(curResConfig: curResConfig);
                    cosmicBodyNextComposition[fusionResult] = cosmicBodyNextComposition.GetValueOrDefault(key: fusionResult) + fusionProductAmount;
                }
            }
            RawMatAmounts newComposition = new(cosmicBodyNextComposition);
            Debug.Assert(composition.Area() == newComposition.Area());
            return newComposition;
        }

        public static (ulong nonReactingAmount, ulong fusionProductAmount) NuclearFusionSingleRawMat(ulong amount, AreaDouble compositionArea, SurfaceGravity surfaceGravity, UDouble surfaceGravityExponent,
            Temperature temperature, UDouble temperatureExponent, TimeSpan duration, UDouble fusionReactionStrengthCoeff)
        {
            // Somewhat equivalent to https://en.wikipedia.org/wiki/Number_density.
            // Since all raw mats have the same area, this naming makes sense
            double rawMatProporInComposition = (double)amount / compositionArea.valueInMetSq,
                reactionStrength = fusionReactionStrengthCoeff
                    * rawMatProporInComposition * rawMatProporInComposition
                    * MyMathHelper.Pow(@base: surfaceGravity.valueInMetPerSeqSq, exponent: surfaceGravityExponent)
                    * MyMathHelper.Pow(@base: temperature.valueInK, exponent: temperatureExponent);
            ulong reactingAmount = MyMathHelper.Min
            (
                amount,
                MyMathHelper.RoundNonneg((decimal)(amount * reactionStrength * duration.TotalSeconds))
            );
            return
            (
                nonReactingAmount: amount - reactingAmount,
                fusionProductAmount: reactingAmount
            );
        }

        /// <summary>
        /// Implements Stefan-Boltzmann law https://en.wikipedia.org/wiki/Stefan%E2%80%93Boltzmann_law to calculate how much energy in total to dissipate
        /// The splitting into heat energy and radiant energy algorithm is my creation
        /// </summary>
        public static (HeatEnergy heatEnergy, RadiantEnergy radiantEnergy) EnergiesToDissipate(HeatEnergy heatEnergy, Length surfaceLength, Propor emissivity, Temperature temperature,
            TimeSpan duration, UDouble stefanBoltzmannConstant, ulong temperatureExponent,
            Temperature allHeatMaxTemper, Temperature halfHeatTemper, UDouble heatEnergyDropoffExponent)
        {
#warning test this
            ulong energyInJToDissipate = MyMathHelper.Min
            (
                heatEnergy.ValueInJ,
                MyMathHelper.RoundNonneg((decimal)(duration.TotalSeconds * surfaceLength.valueInM * emissivity * stefanBoltzmannConstant * MyMathHelper.Pow(@base: temperature.valueInK, exponent: temperatureExponent)))
            );
            double heatEnergyPropor = (temperature <= allHeatMaxTemper) switch
            {
                true => 1,
                false => 1 / (1 + MyMathHelper.Pow(@base: (temperature.valueInK - allHeatMaxTemper.valueInK) / (halfHeatTemper.valueInK - allHeatMaxTemper.valueInK), exponent: heatEnergyDropoffExponent))
            };

            ulong heatEnergyInJ = MyMathHelper.RoundNonneg(energyInJToDissipate * (decimal)heatEnergyPropor);
            return
            (
                heatEnergy: HeatEnergy.CreateFromJoules(valueInJ: heatEnergyInJ),
                radiantEnergy: RadiantEnergy.CreateFromJoules(valueInJ: energyInJToDissipate - heatEnergyInJ)
            );
        }

        /// <summary>
        /// Checks if streamReader starts with tokens, with potentially whitespace at the start and between tokens
        /// </summary>
        /// <param Name="streamReader"></param>
        /// <param Name="tokens"></param>
        /// <returns></returns>
        public static bool StreamStartsWith(StreamReader streamReader, string[] tokens)
        {
            foreach (string token in tokens)
            {
                // Read all whitespace
                while (true)
                {
                    int nextChar = streamReader.Peek();
                    if (nextChar == -1)
                        return false;
                    if (char.IsWhiteSpace((char)nextChar))
                        streamReader.Read();
                    else
                        break;
                }
                foreach (char symbol in token)
                    if (symbol != streamReader.Read())
                        return false;
            }

            return true;
        }
        
        public static TAmount EnergyPropor<TAmount>(TAmount wholeAmount, Propor propor)
            where TAmount : struct, IUnconstrainedEnergy<TAmount>
            => IUnconstrainedEnergy<TAmount>.CreateFromJoules
            (
                valueInJ: MyMathHelper.RoundNonneg(wholeAmount.ValueInJ() * (decimal)propor)
            );

        public static string GanerateNewName(string prefix, EfficientReadOnlyHashSet<string> usedNames)
        {
            for (int i = 0; ; i++)
            {
                string newName = $"{prefix} {i}";
                if (!usedNames.Contains(newName))
                    return newName;
            }
        }

        [Serializable]
        public readonly record struct Vertex<T>(T ResOwner, bool IsSource);

        [Serializable]
        public sealed class VertexInfo<T>
        {
            public readonly List<T> directedNeighbours;
            public ulong amount;

            public VertexInfo(List<T> directedNeighbours, ulong amount)
            {
                this.directedNeighbours = directedNeighbours;
                this.amount = amount;
            }

            public ulong GiveAmountRoundUp()
                => MyMathHelper.DivideThenTakeCeiling(dividend: amount, divisor: (ulong)directedNeighbours.Count);


            public double Priority()
               => (double)amount / directedNeighbours.Count;
        }

        [Serializable]
        public readonly record struct ResPacket<T>(T Source, T Destin, ulong Amount);

        // Dictionary is from Vertex<T> rather than T directly, as the same Industry may be a source and a destination of the same resource, e.g. storage.
        public static EfficientReadOnlyCollection<ResPacket<T>> DistributeRes<T>(EfficientReadOnlyDictionary<Vertex<T>, VertexInfo<T>> graph)
        {
            SimplePriorityQueue<Vertex<T>, double> vertsByPriority = new();
            foreach (var (vertex, vertexInfo) in graph)
                EnqueueVertexIfNeeded(vertex: vertex, vertexInfo: vertexInfo);
            
            List<ResPacket<T>> resPackets = [];
            while (vertsByPriority.Count > 0)
            {
                var (resOwnerA, isASource) = vertsByPriority.Dequeue();
                var vertexAInfo = graph[new(ResOwner: resOwnerA, IsSource: isASource)];
                // ToList is needed as in this process neighbours are removed. Without it, may get error about modifying collection
                // and iterating it at the same time
                var orderedNeighbours = vertexAInfo.directedNeighbours.OrderByDescending
                (
                    resOwnerB => graph[new(ResOwner: resOwnerB, IsSource: !isASource)].Priority()
                ).ToList();
                foreach (var resOwnerB in orderedNeighbours)
                {
                    ulong amount = vertexAInfo.GiveAmountRoundUp();
                    resPackets.Add
                    (
                        new
                        (
                            Source: isASource ? resOwnerA : resOwnerB,
                            Destin: isASource ? resOwnerB : resOwnerA,
                            Amount: amount
                        )
                    );
                    Vertex<T> vertexB = new(ResOwner: resOwnerB, IsSource: !isASource);
                    var vertexBInfo = graph[vertexB];

                    vertexAInfo.directedNeighbours.Remove(resOwnerB);
                    vertexBInfo.directedNeighbours.Remove(resOwnerA);
                    vertexAInfo.amount -= amount;
                    vertexBInfo.amount -= amount;
                    
                    vertsByPriority.Remove(vertexB);
                    EnqueueVertexIfNeeded(vertex: vertexB, vertexInfo: vertexBInfo);
                }
                Debug.Assert(vertexAInfo.amount is 0);
            }

            return resPackets.ToEfficientReadOnlyCollection();

            void EnqueueVertexIfNeeded(Vertex<T> vertex, VertexInfo<T> vertexInfo)
            {
                var priority = vertexInfo.Priority();
                if (priority is not double.PositiveInfinity)
                    vertsByPriority.Enqueue
                    (
                        item: vertex,
                        priority: priority
                    );
            }
        }

        /// <summary>
        /// Assumptions:
        /// * minValue <= maxValue
        /// * isValueOk(minValue) is true
        /// * there exists maxOkValue s.t.:
        ///   * for value <= maxOkValue, isValueOk(value) is true
        ///   * for value > maxOkValue, isValueOk(value) is false
        /// </summary>
        public static ulong FindMaxOkValue(ulong minValue, ulong maxValue, Func<ulong, bool> isValueOk)
        {
            if (minValue > maxValue)
                throw new ArgumentException();
            if (!isValueOk(minValue))
                throw new ArgumentException();
#warning Test this
            while (minValue < maxValue)
            {
                ulong midValue = (minValue + maxValue + 1) / 2;
                if (isValueOk(midValue))
                    minValue = midValue;
                else
                    maxValue = midValue - 1;
            }
            var value = minValue;
            Debug.Assert(minValue <= value && value <= maxValue && isValueOk(value));
            Debug.Assert(minValue + 1 > maxValue || !isValueOk(value + 1));
            return value;
        }

        public static UDouble WeightedAverage((UDouble weight, UDouble value) a, (UDouble weight, UDouble value) b)
            => (a.weight * a.value + b.weight * b.value) / (a.weight + b.weight);

        public static double WeightedAverage((double weight, double value) a, (double weight, double value) b)
            => (a.weight * a.value + b.weight * b.value) / (a.weight + b.weight);

        public static T Interpolate<T>(Propor normalized, T start, T stop)
            where T : struct, IScalar<T>
            => T.Interpolate(normalized: normalized, start: start, stop: stop);

        public static Propor Normalize<T>(T value, T start, T stop)
            where T : struct, IScalar<T>
            => T.Normalize(value: value, start: start, stop: stop);

        public static double Interpolate(Propor normalized, double start, double stop)
            => start + (stop - start) * normalized;

        /// <summary>
        /// This is in order to not crash if value is not between min and max
        /// Such is needed, e.g. when gravity or temperature is bigger than the graph can represent
        /// </summary>
        public static Propor Normalize(double value, double start, double stop)
            => Propor.CreateByClamp(value: (value - start) / (stop - start));

        //public static UDouble PowerMean(ReadOnlySpan<(UDouble weight, UDouble value)> args, double exponent)
        //{
        //    UDouble totalWeight = args.Sum(weightAndFunc => weightAndFunc.weight);
        //    return exponent switch
        //    {
        //        double.NegativeInfinity => args.Min(args => args.value),
        //        // geometric mean
        //        0 => MyMathHelper.Exp(args.Sum(args => args.weight / totalWeight * MyMathHelper.Log(args.value))),
        //        double.PositiveInfinity => args.Max(args => args.value),
        //        double pow => MyMathHelper.Pow
        //        (
        //            @base: args.Sum(args => args.weight / totalWeight * MyMathHelper.Pow(@base: args.value, exponent: pow)),
        //            exponent: 1 / pow
        //        )
        //    };
        //}


        // TODO: could replace this with ReadOnlySpan to do this without allocations
        /// <summary>
        /// Weights must sum to 1
        /// </summary>
        public static UDouble PowerMean(IEnumerable<(Propor weight, UDouble value)> args, double exponent)
            => exponent switch
            {
                double.NegativeInfinity => args.Min(args => args.value),
                // geometric mean
                0 => MyMathHelper.Exp(args.Sum(args => args.weight * MyMathHelper.Log(args.value))),
                double.PositiveInfinity => args.Max(args => args.value),
                double pow => MyMathHelper.Pow
                (
                    @base: args.Sum(args => args.weight * MyMathHelper.Pow(@base: args.value, exponent: pow)),
                    exponent: 1 / pow
                )
            };

        //public static Func<T, UDouble> PowerMean<T>(List<(UDouble weight, Func<T, UDouble> func)> args, double exponent)
        //{
        //    UDouble totalWeight = args.Sum(weightAndFunc => weightAndFunc.weight);
        //    return exponent switch
        //    {
        //        double.NegativeInfinity => value => args.Min(args => args.func(value)),
        //        // geometric mean
        //        0 => value => MyMathHelper.Exp(args.Sum(args => args.weight / totalWeight * MyMathHelper.Log(args.func(value)))),
        //        double.PositiveInfinity => value => args.Max(args => args.func(value)),
        //        double pow => value => MyMathHelper.Pow
        //        (
        //            @base: args.Sum(args => args.weight / totalWeight * MyMathHelper.Pow(@base: args.func(value), exponent: pow)),
        //            exponent: 1 / pow
        //        )
        //    };
        //}
    }
}
