using Game1.Delegates;
using System.Numerics;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class EnergyManager : IDeletedListener
    {
        // TODO(performace): could probably improve performance by separating those requiring no electricity at the start
        // Then only those requiring non-zero electricity would be involved in more costly calculations
        [Serializable]
        private class CombinedEnergyConsumer : IComparable<CombinedEnergyConsumer>
        {
            public ElectricalEnergy TotalReqEnergy { get; private set; }

            public ElectricalEnergy CurReqEnergy { get; private set; }

            /// <summary>
            /// node from which consume energy
            /// </summary>
            public readonly NodeID nodeID;

            /// <summary>
            /// the lower, the more important
            /// </summary>
            public readonly EnergyPriority energyPriority;

            private readonly List<IEnergyConsumer> energyConsumers;

            public CombinedEnergyConsumer(NodeID nodeID, EnergyPriority energyPriority)
            {
                this.nodeID = nodeID;
                this.energyPriority = energyPriority;
                energyConsumers = new();
                TotalReqEnergy = ElectricalEnergy.zero;
                CurReqEnergy = ElectricalEnergy.zero;
            }

            public void AddEnergyConsumer(IEnergyConsumer energyConsumer)
            {
                energyConsumers.Add(energyConsumer);
                var reqEnergy = energyConsumer.ReqEnergy();
                TotalReqEnergy += reqEnergy;
                CurReqEnergy += reqEnergy;
            }

            public void ConsumeEnergyFrom<T>(T source, ElectricalEnergy electricalEnergy, Func<NodeID, INodeAsLocalEnergyProducerAndConsumer> nodeIDToNode)
                where T : IEnergySouce<ElectricalEnergy>
            {
                CurReqEnergy -= electricalEnergy;
                nodeIDToNode(nodeID).ConsumeEnergyFrom(source: source, electricalEnergy: electricalEnergy);
            }

            public int CompareTo(CombinedEnergyConsumer? other)
                => energyPriority.CompareTo(other: other.energyPriority);
        }

        private readonly MySet<IEnergyProducer> energyProducers;
        private readonly MySet<IEnergyConsumer> energyConsumers;
        
        private ElectricalEnergy totReqEnergy, totProdEnergy, totUsedLocalEnergy, totUsedPowerPlantEnergy;
        // Used only to transfer electrical energy;
        private readonly LocationCounters locationCounters;

        public EnergyManager()
        {
            energyProducers = new();
            energyConsumers = new();
            totReqEnergy = ElectricalEnergy.zero;
            totProdEnergy = ElectricalEnergy.zero;
            totUsedLocalEnergy = ElectricalEnergy.zero;
            totUsedPowerPlantEnergy = ElectricalEnergy.zero;
            locationCounters = LocationCounters.CreateEmpty();
        }

        public void AddEnergyProducer(IEnergyProducer energyProducer)
        {
            energyProducers.Add(energyProducer);

            if (!energyProducer.Deleted.Contains(listener: this))
                energyProducer.Deleted.Add(listener: this);
        }

        public void AddEnergyConsumer(IEnergyConsumer energyConsumer)
        {
            energyConsumers.Add(energyConsumer);

            if (!energyConsumer.Deleted.Contains(listener: this))
                energyConsumer.Deleted.Add(listener: this);
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
            CombinedEnergyConsumer[] combinedEnergyConsumers = CombineEnergyConsumers();

            foreach (var energyProducer in energyProducers)
                energyProducer.ProduceEnergy(destin: locationCounters);
            totProdEnergy = locationCounters.ElectricalEnergy;
            totReqEnergy = combinedEnergyConsumers.Sum(combinedEnergyConsumer => combinedEnergyConsumer.TotalReqEnergy);
            totUsedLocalEnergy = ElectricalEnergy.zero;
            totUsedPowerPlantEnergy = ElectricalEnergy.zero;

            Dictionary<NodeID, List<CombinedEnergyConsumer>> combinedEnergyConsumersByNode = new();
            foreach (var nodeID in nodeIDs)
                combinedEnergyConsumersByNode[nodeID] = new();
            foreach (var combinedEnergyConsumer in combinedEnergyConsumers)
                combinedEnergyConsumersByNode[combinedEnergyConsumer.nodeID].Add(combinedEnergyConsumer);

            foreach (var (nodeID, sameNodeEnergyConsumers) in combinedEnergyConsumersByNode)
            {
                var node = nodeIDToNode(nodeID);
                var energyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: locationCounters);
                node.ProduceLocalEnergy(destin: energyPile);
                var producedLocalEnergy = energyPile.Energy;
                DistributeLocallyProducedEnergy
                (
                    nodeID: nodeID,
                    combinedEnergyConsumers: sameNodeEnergyConsumers,
                    energySource: energyPile
                );
                node.ConsumeUnusedLocalEnergyFrom(source: energyPile, electricalEnergy: energyPile.Energy);
                totUsedLocalEnergy += producedLocalEnergy - energyPile.Energy;
            }

            

            ElectricalEnergy totAvailableEnergy = DistributePartOfEnergy
            (
                combinedEnergyConsumers: combinedEnergyConsumers,
                availableEnergy: totProdEnergy
            );

            totUsedPowerPlantEnergy = totProdEnergy - totAvailableEnergy;

            foreach (var combinedEnergyConsumer in combinedEnergyConsumers)
            {
                ElectricalEnergy curReqEnergy = combinedEnergyConsumer.ReqEnergy();
                Propor energyPropor = MyMathHelper.IsTiny(value: curReqEnergy) switch
                {
                    true => Propor.full,
                    false => Propor.Create(reqEnergyByCombinedConsumer[combinedEnergyConsumer], curReqEnergy)!.Value.Opposite()
                };
                if (combinedEnergyConsumer.EnergyPriority == EnergyPriority.minimal && !energyPropor.IsCloseTo(other: Propor.full))
                    throw new Exception();
                combinedEnergyConsumer.ConsumeEnergy(energyPropor: energyPropor);
            }

            return;

            CombinedEnergyConsumer[] CombineEnergyConsumers()
            {
                Dictionary<(NodeID nodeID, EnergyPriority energyPriority), CombinedEnergyConsumer> combinedEnergyConsumersDict = new();

                foreach (var energyConsumer in energyConsumers)
                {
                    var nodeIDAndEnergyPriority = (energyConsumer.NodeID, energyConsumer.EnergyPriority);
                    combinedEnergyConsumersDict.TryAdd
                    (
                        nodeIDAndEnergyPriority,
                        new
                        (
                            nodeID: energyConsumer.NodeID,
                            energyPriority: energyConsumer.EnergyPriority
                        )
                    );
                    combinedEnergyConsumersDict[nodeIDAndEnergyPriority].AddEnergyConsumer(energyConsumer: energyConsumer);
                }
                return combinedEnergyConsumersDict.Values.ToArray();
            }

            // remaining energy is left in energySource
            void DistributeLocallyProducedEnergy<T>(NodeID nodeID, List<CombinedEnergyConsumer> combinedEnergyConsumers, T energySource)
                where T : IEnergySouce<ElectricalEnergy>
            {
                Debug.Assert(combinedEnergyConsumers.All(combinedEnergyConsumer => combinedEnergyConsumer.nodeID == nodeID));
                combinedEnergyConsumers.Sort();

                foreach (var combinedEnergyConsumer in combinedEnergyConsumers)
                {
                    combinedEnergyConsumer.ConsumeEnergyFrom
                    (
                        source: energySource,
                        electricalEnergy: MyMathHelper.Min
                        (
                            value1: energySource.Energy,
                            combinedEnergyConsumer.CurReqEnergy
                        ),
                        nodeIDToNode: nodeIDToNode
                    );
                    if (energySource.Energy.IsZero)
                        break;
                }
            }

            // remaining energy is left in energySource
            void DistributeEnergy<T>(CombinedEnergyConsumer[] combinedEnergyConsumers, T energySource)
                where T : IEnergySouce<ElectricalEnergy>
            {
                SortedDictionary<EnergyPriority, List<CombinedEnergyConsumer>> combinedEnergyConsumersByPriority = new();
                foreach (var combinedEnergyConsumer in combinedEnergyConsumers)
                {
                    EnergyPriority priority = combinedEnergyConsumer.energyPriority;
                    if (!combinedEnergyConsumersByPriority.ContainsKey(key: priority))
                        combinedEnergyConsumersByPriority[priority] = new();
                    combinedEnergyConsumersByPriority[priority].Add(combinedEnergyConsumer);
                }

                foreach (var (priority, samePriorCombinedEnergyConsumers) in combinedEnergyConsumersByPriority)
                {
                    ElectricalEnergy reqEnergy = samePriorCombinedEnergyConsumers.Sum(combinedEnergyConsumer => combinedEnergyConsumer.CurReqEnergy);
                    
                    if (energySource.Energy < reqEnergy)
                    {
                        Propor energyPropor = (Propor)(availableEnergy / reqEnergy);
                        break;
                    }
                    else
                    {
                        foreach (var combinedEnergyConsumer in samePriorCombinedEnergyConsumers)
                            combinedEnergyConsumer.ConsumeEnergyFrom
                            (
                                source: energySource,
                                electricalEnergy: combinedEnergyConsumer.CurReqEnergy,
                                nodeIDToNode: nodeIDToNode
                            );
                    }
                }
            }
        }

        public string Summary()
            => $"""
            required energy: {totReqEnergy.valueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W
            produced energy: {totProdEnergy.valueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W
            used local energy {totUsedLocalEnergy.valueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W
            used power plant energy {totUsedPowerPlantEnergy.valueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W

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
