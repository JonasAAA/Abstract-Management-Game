﻿using Game1.Collections;
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
            EfficientReadOnlyCollection<ConsumerWithEnergy<T>> consumersWithEnergy = new
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

        // Inspired by https://en.wikipedia.org/wiki/Lawson_criterion#Energy_balance
        public static RawMatAmounts CosmicBodyNewCompositionFromNuclearFusion(ResConfig curResConfig, RawMatAmounts composition, UDouble gravity, Temperature temperature, TimeSpan duration,
            Func<RawMaterial, decimal, ulong> reactionNumberRounder, Propor nonReactingProporForUnitReactionStrengthUnitTime)
        {
            AreaDouble compositionArea = composition.Area().ToDouble();
            Dictionary<RawMaterial, ulong> cosmicBodyNextComposition = new(2 * composition.Count);
            foreach (var (rawMaterial, amount) in composition)
            {
                (ulong nonReactingAmount, ulong fusionProductAmount) = NuclearFusionSingleRawMat
                (
                    amount: amount,
                    compositionArea: compositionArea,
                    gravity: gravity,
                    temperature: temperature,
                    duration: duration,
                    reactionNumberRounder: reactionNum => reactionNumberRounder(rawMaterial, reactionNum),
                    nonReactingProporForUnitReactionStrengthUnitTime: nonReactingProporForUnitReactionStrengthUnitTime,
                    fusionReactionStrengthCoeff: rawMaterial.FusionReactionStrengthCoeff
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

        public static (ulong nonReactingAmount, ulong fusionProductAmount) NuclearFusionSingleRawMat(ulong amount, AreaDouble compositionArea, UDouble gravity, Temperature temperature,
            TimeSpan duration, Func<decimal, ulong> reactionNumberRounder, Propor nonReactingProporForUnitReactionStrengthUnitTime, UDouble fusionReactionStrengthCoeff)
        {
            // Number density is from https://en.wikipedia.org/wiki/Number_density
            // Volume number density is the number of specified objects per unit volume
            double numberDensity = amount / compositionArea.valueInMetSq,
                reactionStrength = fusionReactionStrengthCoeff * numberDensity * numberDensity * gravity * gravity * temperature.valueInK * temperature.valueInK;
            decimal nonReactingPropor = (decimal)MyMathHelper.Pow(@base: (UDouble)nonReactingProporForUnitReactionStrengthUnitTime, exponent: reactionStrength * duration.TotalSeconds);
            ulong maxReactions = amount / 2,
                numberOfReactions = maxReactions - reactionNumberRounder(nonReactingPropor * maxReactions);
            return
            (
                nonReactingAmount: amount - 2 * numberOfReactions,
                fusionProductAmount: numberOfReactions
            );
        }

        /// <summary>
        /// Implements Stefan-Boltzmann law https://en.wikipedia.org/wiki/Stefan%E2%80%93Boltzmann_law to calculate how much energy in total to dissipate
        /// The splitting into heat energy and radiant energy algorithm is my creation
        /// </summary>
        public static (HeatEnergy heatEnergy, RadiantEnergy radiantEnergy) EnergiesToDissipate(HeatEnergy heatEnergy, UDouble surfaceLength, Propor emissivity, Temperature temperature,
            TimeSpan duration, Func<decimal, ulong> energyInJToDissipateRoundFunc, UDouble stefanBoltzmannConstant, ulong temperatureExponent, Func<decimal, ulong> heatEnergyInJRoundFunc,
            Temperature allHeatMaxTemper, Temperature halfHeatTemper, UDouble heatEnergyDropoffExponent)
        {
#warning test this
            ulong energyInJToDissipate = MyMathHelper.Min
            (
                heatEnergy.ValueInJ,
                energyInJToDissipateRoundFunc((decimal)(duration.TotalSeconds * surfaceLength * emissivity * stefanBoltzmannConstant * MyMathHelper.Pow(@base: temperature.valueInK, exponent: temperatureExponent)))
            );
            double heatEnergyPropor = (temperature <= allHeatMaxTemper) switch
            {
                true => 1,
                false => 1 / (1 + MyMathHelper.Pow(@base: (temperature.valueInK - allHeatMaxTemper.valueInK) / (halfHeatTemper.valueInK - allHeatMaxTemper.valueInK), exponent: heatEnergyDropoffExponent))
            };

            ulong heatEnergyInJ = heatEnergyInJRoundFunc(energyInJToDissipate * (decimal)heatEnergyPropor);
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
        
        public static TAmount EnergyPropor<TAmount>(TAmount wholeAmount, Propor propor, Func<decimal, ulong> roundFunc)
            where TAmount : struct, IUnconstrainedEnergy<TAmount>
            => IUnconstrainedEnergy<TAmount>.CreateFromJoules
            (
                valueInJ: roundFunc(wholeAmount.ValueInJ() * (decimal)propor)
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

        [Serializable]
        private readonly record struct InternalResPacket<T>(T VertexA, T VertexB, bool IsASource, ulong Amount);

        // Dictionary is from Vertex<T> rather than T directly, as the same Industry may be a source and a destination of the same resource, e.g. storage.
        public static EfficientReadOnlyCollection<ResPacket<T>> DistributeRes<T>(EfficientReadOnlyDictionary<Vertex<T>, VertexInfo<T>> graph)
        {
            SimplePriorityQueue<Vertex<T>, double> vertsByPriority = new();
            foreach (var (vertex, vertexInfo) in graph)
                EnqueueVertexIfNeeded(vertex: vertex, vertexInfo: vertexInfo);
            
            List<ResPacket<T>> resPackets = new();
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
    }
}
