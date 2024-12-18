﻿using Game1.Collections;
using Game1.Delegates;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class ElectricalEnergyManager : IDeletedListener, IEnergyDistributor
    {
        private readonly ThrowingSet<IEnergyProducer> energyProducers;
        private readonly ThrowingSet<IEnergyConsumer> energyConsumers;
        
        private ElectricalEnergy totReqEnergy, totProdEnergy, totUsedLocalEnergy, totUsedPowerPlantEnergy;

        public ElectricalEnergyManager()
        {
            energyProducers = [];
            energyConsumers = [];
            totReqEnergy = ElectricalEnergy.zero;
            totProdEnergy = ElectricalEnergy.zero;
            totUsedLocalEnergy = ElectricalEnergy.zero;
            totUsedPowerPlantEnergy = ElectricalEnergy.zero;
        }

        public void AddEnergyProducer(IEnergyProducer energyProducer)
        {
            energyProducers.Add(energyProducer);

            (energyProducer as IDeletable)?.Deleted.Add(listener: this);
        }

        public void RemoveEnergyProducer(IEnergyProducer energyProducer)
        {
            if (energyProducer is IDeletable)
                throw new ArgumentException("Will be removed anyway, no need to do so manually");
            energyProducers.Remove(energyProducer);
        }

        public void AddEnergyConsumer(IEnergyConsumer energyConsumer)
        {
            energyConsumers.Add(energyConsumer);

            (energyConsumer as IDeletable)?.Deleted.Add(listener: this);
        }

        [Serializable]
        private readonly struct EnhancedEnergyConsumer
        {
            public ElectricalEnergy OwnedEnergy
                => ownedEnergyPile.Amount;
            public readonly ElectricalEnergy reqEnergy;
            public readonly NodeID nodeID;
            public readonly EnergyPriority energyPriority;
            
            private readonly IEnergyConsumer energyConsumer;
            private readonly Pile<ElectricalEnergy> ownedEnergyPile;

            public EnhancedEnergyConsumer(IEnergyConsumer energyConsumer, LocationCounters locationCounters)
            {
                this.energyConsumer = energyConsumer;
                nodeID = energyConsumer.NodeID;
                energyPriority = energyConsumer.EnergyPriority;
                reqEnergy = energyConsumer.ReqEnergy();
                ownedEnergyPile = Pile<ElectricalEnergy>.CreateEmpty(locationCounters: locationCounters);
            }

            public void TransferEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy energy)
                => source.TransferTo(destin: ownedEnergyPile, amount: energy);

            public void ConsumeEnergy()
                => energyConsumer.ConsumeEnergyFrom(source: ownedEnergyPile, electricalEnergy: OwnedEnergy);
        }

        // The energy is distributed as follows:
        // 1. Each planet local energy is distributed evenly (i.e. to cover the same proportion of required energy) to priority 0 consumers,
        //    then if have enough, evenly to priority 1 consumers, etc.
        // 2. Energy produced in power plants is distributed evenly to priority 0 consumers, then if have enough, evenly to priority 1 consumers, etc.
        // Note that in step 2. all proportions are taken with respect to still needed energy, not the total required energy.
        // E.g. have person A on planet P, person B on planet Q. They each require 10 J. But if planet P produces more local energy,
        // chances are that person A gets more energy afterall
        // TODO(performance, GC) allocate all intermediate collections on stack/reuse heap collections from the previous frame where possible
        public void DistributeEnergy(IEnumerable<NodeID> nodeIDs, Func<NodeID, INodeAsLocalEnergyProducerAndConsumer> nodeIDToNode)
        {
            var energyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: LocationCounters.CreateEmpty());
            // TODO(performace): could probably improve performance by separating those requiring no electricity at the start
            // Then only those requiring non-zero electricity would be involved in more costly calculations
            var enhancedConsumers =
               (from energyConsumer in energyConsumers
                select new EnhancedEnergyConsumer
                (
                    energyConsumer: energyConsumer,
                    locationCounters: energyPile.LocationCounters
                )).ToList();

            var producedEnergies = energyProducers.ToEfficientReadOnlyDict
            (
                elementSelector: energyProducer =>
                {
                    var prevEnergy = energyPile.Amount;
                    energyProducer.ProduceEnergy(destin: energyPile);
                    return energyPile.Amount - prevEnergy;
                }
            );

            //foreach (var energyProducer in energyProducers)
            //    energyProducer.ProduceEnergy(destin: energyPile);
            totProdEnergy = energyPile.Amount;
            totReqEnergy = enhancedConsumers.Sum(enhancedConsumer => enhancedConsumer.reqEnergy);
            totUsedLocalEnergy = ElectricalEnergy.zero;
            totUsedPowerPlantEnergy = ElectricalEnergy.zero;

            Dictionary<NodeID, ThrowingSet<EnhancedEnergyConsumer>> enhancedConsumersByNode = [];
            foreach (var nodeID in nodeIDs)
                enhancedConsumersByNode[nodeID] = [];
            foreach (var enhancedConsumer in enhancedConsumers)
                enhancedConsumersByNode[enhancedConsumer.nodeID].Add(enhancedConsumer);

            foreach (var (nodeID, sameNodeEnhancedConsumers) in enhancedConsumersByNode)
            {
                var node = nodeIDToNode(nodeID);
                var energySource = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: energyPile.LocationCounters);
                node.ProduceLocalEnergy(destin: energySource);
                var locallyProducedEnergy = energySource.Amount;
                DistributePartOfEnergy
                (
                    enhancedConsumers: sameNodeEnhancedConsumers,
                    energySource: energySource
                );
                totUsedLocalEnergy += locallyProducedEnergy - energySource.Amount;
                node.ConsumeUnusedLocalEnergyFrom(source: energySource, electricalEnergy: energySource.Amount);
            }

            DistributePartOfEnergy
            (
                enhancedConsumers: enhancedConsumers,
                energySource: energyPile 
            );

            totUsedPowerPlantEnergy = totProdEnergy - energyPile.Amount;

            foreach (var enhancedConsumer in enhancedConsumers)
            {
                Debug.Assert(enhancedConsumer.OwnedEnergy <= enhancedConsumer.reqEnergy);
                if (enhancedConsumer.energyPriority == EnergyPriority.mostImportant && enhancedConsumer.OwnedEnergy < enhancedConsumer.reqEnergy)
                    throw new InvalidStateException();
                enhancedConsumer.ConsumeEnergy();
            }

            var (allocatedEnergies, unusedEnergy) = Algorithms.SplitEnergyEvenly
            (
                reqEnergies: producedEnergies.Values.ToList(),
                availableEnergy: energyPile.Amount
            );
            Debug.Assert(unusedEnergy.IsZero);
            foreach (var (energyProducer, allocatedEnergy) in producedEnergies.Keys.Zip(allocatedEnergies))
                energyProducer.TakeBackUnusedEnergy(source: energyPile, amount: allocatedEnergy);
            Debug.Assert(energyPile.Amount.IsZero);
            return;

            // remaining energy is left in energySource
            static void DistributePartOfEnergy(IEnumerable<EnhancedEnergyConsumer> enhancedConsumers, Pile<ElectricalEnergy> energySource)
            {
                SortedDictionary<EnergyPriority, List<EnhancedEnergyConsumer>> enhancedConsumersByPriority = [];
                foreach (var enhancedConsumer in enhancedConsumers)
                {
                    EnergyPriority priority = enhancedConsumer.energyPriority;
                    enhancedConsumersByPriority.TryAdd(key: priority, value: []);
                    enhancedConsumersByPriority[priority].Add(enhancedConsumer);
                }

                // Reverse to give energy to the highest priority consumers first
                foreach (var samePriorEnhancedConsumers in enhancedConsumersByPriority.Values.Reverse())
                {
                    var energies =
                       (from enhancedConsumer in samePriorEnhancedConsumers
                        select (ownedEnergy: enhancedConsumer.OwnedEnergy, enhancedConsumer.reqEnergy)).ToList();
                    var (allocatedEnergies, _) = Algorithms.SplitExtraEnergyEvenly
                    (
                        energies: energies,
                        availableEnergy: energySource.Amount
                    );
                    foreach (var (enhancedConsumer, allocatedEnergy) in samePriorEnhancedConsumers.Zip(allocatedEnergies))
                        enhancedConsumer.TransferEnergyFrom(source: energySource, energy: allocatedEnergy);
                }
            }
        }

        public string Summary()
            => $"""
            required electricity: {totReqEnergy.ValueInJ / (1000 * CurWorldManager.Elapsed.TotalSeconds):#,0.} kW
            produced electricity: {totProdEnergy.ValueInJ / (1000 * CurWorldManager.Elapsed.TotalSeconds):#,0.} kW
            used electricity: {totUsedPowerPlantEnergy.ValueInJ / (1000 * CurWorldManager.Elapsed.TotalSeconds):#,0.} kW
            """;

        void IDeletedListener.DeletedResponse(IDeletable deletable)
        {
            if (deletable is IEnergyProducer energyProducer)
                energyProducers.Remove(energyProducer);
            if (deletable is IEnergyConsumer energyConsumer)
                energyConsumers.Remove(energyConsumer);
        }
    }
}
