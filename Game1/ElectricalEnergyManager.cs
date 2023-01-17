﻿using Game1.Delegates;
using System.Numerics;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class ElectricalEnergyManager : IDeletedListener, IEnergyDistributor
    {
        private readonly MySet<IEnergyProducer> energyProducers;
        private readonly MySet<IEnergyConsumer> energyConsumers;
        
        private ElectricalEnergy totReqEnergy, totProdEnergy, totUsedLocalEnergy, totUsedPowerPlantEnergy;
        private readonly IPile<ElectricalEnergy> energyPile;

        public ElectricalEnergyManager()
        {
            energyProducers = new();
            energyConsumers = new();
            totReqEnergy = ElectricalEnergy.zero;
            totProdEnergy = ElectricalEnergy.zero;
            totUsedLocalEnergy = ElectricalEnergy.zero;
            totUsedPowerPlantEnergy = ElectricalEnergy.zero;
            energyPile = IndividualCounters.CreateEmpty(locationCounters: LocationCounters.CreateEmpty());
        }

        public void AddEnergyProducer(IEnergyProducer energyProducer)
        {
            energyProducers.Add(energyProducer);

            energyProducer.Deleted.Add(listener: this);
        }

        public void AddEnergyConsumer(IEnergyConsumer energyConsumer)
        {
            energyConsumers.Add(energyConsumer);

            (energyConsumer as IDeletable)?.Deleted.Add(listener: this);
        }

        [Serializable]
        private class EnhancedEnergyConsumer<TPile>
            where TPile : IPile<ElectricalEnergy>
        {
            public ElectricalEnergy OwnedEnergy
                => ownedEnergyPile.Amount;
            public readonly ElectricalEnergy reqEnergy;
            public readonly NodeID nodeID;
            public readonly EnergyPriority energyPriority;
            
            private readonly IEnergyConsumer energyConsumer;
            private readonly TPile ownedEnergyPile;

            public EnhancedEnergyConsumer(IEnergyConsumer energyConsumer, TPile ownedEnergyPile)
            {
                this.energyConsumer = energyConsumer;
                nodeID = energyConsumer.NodeID;
                energyPriority = energyConsumer.EnergyPriority;
                reqEnergy = energyConsumer.ReqEnergy();
                this.ownedEnergyPile = ownedEnergyPile;
            }

            public void TransferEnergyFrom<TSource>(TSource source, ElectricalEnergy energy)
                where TSource : ISourcePile<ElectricalEnergy>
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
        // TODO(performance, GC) allocate all intermediate collections on stack/reuse heap collections from previous frame where possible
        public void DistributeEnergy(IEnumerable<NodeID> nodeIDs, Func<NodeID, INodeAsLocalEnergyProducerAndConsumer> nodeIDToNode)
        {
            // TODO(performace): could probably improve performance by separating those requiring no electricity at the start
            // Then only those requiring non-zero electricity would be involved in more costly calculations
            List<EnhancedEnergyConsumer<IndividualCounters>> enhancedConsumers =
               (from energyConsumer in energyConsumers
                select new EnhancedEnergyConsumer<IndividualCounters>
                (
                    energyConsumer: energyConsumer,
                    ownedEnergyPile: IndividualCounters.CreateEmpty(locationCounters: energyPile.LocationCounters)
                )).ToList();

            foreach (var energyProducer in energyProducers)
                energyProducer.ProduceEnergy(destin: energyPile);
            totProdEnergy = energyPile.Amount;
            totReqEnergy = enhancedConsumers.Sum(enhancedConsumer => enhancedConsumer.reqEnergy);
            totUsedLocalEnergy = ElectricalEnergy.zero;
            totUsedPowerPlantEnergy = ElectricalEnergy.zero;

            Dictionary<NodeID, MySet<EnhancedEnergyConsumer<IndividualCounters>>> enhancedConsumersByNode = new();
            foreach (var nodeID in nodeIDs)
                enhancedConsumersByNode[nodeID] = new();
            foreach (var enhancedConsumer in enhancedConsumers)
                enhancedConsumersByNode[enhancedConsumer.nodeID].Add(enhancedConsumer);

            foreach (var (nodeID, sameNodeEnhancedConsumers) in enhancedConsumersByNode)
            {
                var node = nodeIDToNode(nodeID);
                IPile<ElectricalEnergy> energySource = IndividualCounters.CreateEmpty(locationCounters: energyPile.LocationCounters);
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
                    throw new Exception();
                enhancedConsumer.ConsumeEnergy();
            }

            return;

            // remaining energy is left in energySource
            static void DistributePartOfEnergy<T>(IEnumerable<EnhancedEnergyConsumer<IndividualCounters>> enhancedConsumers, T energySource)
                where T : ISourcePile<ElectricalEnergy>
            {
                SortedDictionary<EnergyPriority, List<EnhancedEnergyConsumer<IndividualCounters>>> enhancedConsumersByPriority = new();
                foreach (var enhancedConsumer in enhancedConsumers)
                {
                    EnergyPriority priority = enhancedConsumer.energyPriority;
                    enhancedConsumersByPriority.TryAdd(key: priority, value: new List<EnhancedEnergyConsumer<IndividualCounters>>());
                    enhancedConsumersByPriority[priority].Add(enhancedConsumer);
                }

                foreach (var (priority, samePriorEnhancedConsumers) in enhancedConsumersByPriority)
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
            // If possible, gather all energy meant for one consumer into one place and give it all at once
            // If not possible/convenient enough to do so, change CombinedEnergyConsumer to only distribute energy when get it from all sources
            // Distribute energy
            throw new NotImplementedException();
        }

        public string Summary()
            => $"""
            required energy: {totReqEnergy.ValueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W
            produced energy: {totProdEnergy.ValueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W
            used local energy {totUsedLocalEnergy.ValueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W
            used power plant energy {totUsedPowerPlantEnergy.ValueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W

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