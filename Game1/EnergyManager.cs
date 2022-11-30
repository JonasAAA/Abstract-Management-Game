using Game1.Delegates;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class EnergyManager : IDeletedListener
    {
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

        public void DistributeEnergy(IEnumerable<NodeID> nodeIDs, Func<NodeID, INodeAsLocalEnergyProducerAndConsumer> nodeIDToNode)
        {
            // TODO(performace): could probably improve performance by separating those requiring no electricity at the start
            // Then only those requiring non-zero electricity would be involved in more costly calculations
            Dictionary<IEnergyConsumer, ElectricalEnergy> reqEnergyByConsumer = energyConsumers.ToDictionary
            (
                keySelector: energyConsumer => energyConsumer,
                elementSelector: energyConsumer => energyConsumer.ReqEnergy()
            );

            Dictionary<NodeID, MySet<IEnergyConsumer>> energyConsumersByNode = new();
            foreach (var nodeID in nodeIDs)
                energyConsumersByNode[nodeID] = new();
            foreach (var energyConsumer in energyConsumers)
                energyConsumersByNode[energyConsumer.NodeID].Add(energyConsumer);

            foreach (var (nodeID, sameNodeEnergyConsumers) in energyConsumersByNode)
            {
                var node = nodeIDToNode(nodeID);
                ElectricalEnergy availableEnergy = DistributePartOfEnergy
                (
                    energyConsumers: sameNodeEnergyConsumers,
                    availableEnergy: node.LocallyProducedEnergy
                );
                ElectricalEnergy usedLocalEnergy = node.LocallyProducedEnergy - availableEnergy;
                node.SetUsedLocalEnergy(usedLocalEnergy: usedLocalEnergy);
                totUsedLocalEnergy += usedLocalEnergy;
            }

            foreach (var energyProducer in energyProducers)
                energyProducer.ProduceEnergy(destin: locationCounters);
            totProdEnergy = locationCounters.ElectricalEnergy;
            totReqEnergy = energyConsumers.Sum(energyConsumer => reqEnergyByConsumer[energyConsumer]);
            totUsedLocalEnergy = ElectricalEnergy.zero;
            totUsedPowerPlantEnergy = ElectricalEnergy.zero;

            ElectricalEnergy totAvailableEnergy = DistributePartOfEnergy
            (
                energyConsumers: energyConsumers,
                availableEnergy: totProdEnergy
            );

            totUsedPowerPlantEnergy = totProdEnergy - totAvailableEnergy;

            foreach (var energyConsumer in energyConsumers)
            {
                ElectricalEnergy curReqEnergy = energyConsumer.ReqEnergy();
                Propor energyPropor = MyMathHelper.IsTiny(value: curReqEnergy) switch
                {
                    true => Propor.full,
                    false => Propor.Create(reqEnergyByConsumer[energyConsumer], curReqEnergy)!.Value.Opposite()
                };
                if (energyConsumer.EnergyPriority == EnergyPriority.minimal && !energyPropor.IsCloseTo(other: Propor.full))
                    throw new Exception();
                energyConsumer.ConsumeEnergy(energyPropor: energyPropor);
            }

            return;

            // remaining energy is left in energySource
            void DistributePartOfEnergy<T>(IEnumerable<IEnergyConsumer> energyConsumers, T energySource)
                where T : IEnergySouce<ElectricalEnergy>
            {
                SortedDictionary<EnergyPriority, MySet<IEnergyConsumer>> energyConsumersByPriority = new();
                foreach (var energyConsumer in energyConsumers)
                {
                    EnergyPriority priority = energyConsumer.EnergyPriority;
                    if (!energyConsumersByPriority.ContainsKey(key: priority))
                        energyConsumersByPriority[priority] = new();
                    energyConsumersByPriority[priority].Add(energyConsumer);
                }

                foreach (var (priority, samePriorEnergyConsumers) in energyConsumersByPriority)
                {
                    ElectricalEnergy reqEnergy = samePriorEnergyConsumers.Sum(energyConsumer => reqEnergyByConsumer[energyConsumer]);
                    Propor energyPropor;

                    if (energySource.AvailableEnergy < reqEnergy)
                    {
                        energyPropor = (Propor)(availableEnergy / reqEnergy);
                        availableEnergy = ElectricalEnergy.zero;
                    }
                    else
                    {
                        energyPropor = Propor.full;
                        availableWatts = (UDouble)(availableWatts - reqWatts);
                        foreach (var energyConsumer in samePriorEnergyConsumers)
                            energyConsumer.ConsumeEnergyFrom(source: energySource, electricalEnergy: energyConsumer.ReqEnergy());
                        // need to decrease the energy required by that consumer
                        throw new NotImplementedException();
                    }

                    foreach (var energyConsumer in samePriorEnergyConsumers)
                        reqEnergyByConsumer[energyConsumer] *= energyPropor.Opposite();
                }

                return availableEnergy;
            }
        }

        //public void DistributeEnergy(IEnumerable<NodeID> nodeIDs, Func<NodeID, INodeAsLocalEnergyProducerAndConsumer> nodeIDToNode)
        //{
        //    // TODO(performace): could probably improve performance by separating those requiring no electricity at the start
        //    // Then only those requiring non-zero electricity would be involved in more costly calculations
        //    Dictionary<IEnergyConsumer, ElectricalEnergy> reqEnergyByConsumer = energyConsumers.ToDictionary
        //    (
        //        keySelector: energyConsumer => energyConsumer,
        //        elementSelector: energyConsumer => energyConsumer.ReqEnergy()
        //    );

        //    Dictionary<NodeID, MySet<IEnergyConsumer>> energyConsumersByNode = new();
        //    foreach (var nodeID in nodeIDs)
        //        energyConsumersByNode[nodeID] = new();
        //    foreach (var energyConsumer in energyConsumers)
        //        energyConsumersByNode[energyConsumer.NodeID].Add(energyConsumer);

        //    foreach (var (nodeID, sameNodeEnergyConsumers) in energyConsumersByNode)
        //    {
        //        var node = nodeIDToNode(nodeID);
        //        ElectricalEnergy availableEnergy = DistributePartOfEnergy
        //        (
        //            energyConsumers: sameNodeEnergyConsumers,
        //            availableEnergy: node.LocallyProducedEnergy
        //        );
        //        ElectricalEnergy usedLocalEnergy = node.LocallyProducedEnergy - availableEnergy;
        //        node.SetUsedLocalEnergy(usedLocalEnergy: usedLocalEnergy);
        //        totUsedLocalEnergy += usedLocalEnergy;
        //    }

        //    foreach (var energyProducer in energyProducers)
        //        energyProducer.ProduceEnergy(destin: locationCounters);
        //    totProdEnergy = locationCounters.ElectricalEnergy;
        //    totReqEnergy = energyConsumers.Sum(energyConsumer => reqEnergyByConsumer[energyConsumer]);
        //    totUsedLocalEnergy = ElectricalEnergy.zero;
        //    totUsedPowerPlantEnergy = ElectricalEnergy.zero;

        //    ElectricalEnergy totAvailableEnergy = DistributePartOfEnergy
        //    (
        //        energyConsumers: energyConsumers,
        //        availableEnergy: totProdEnergy
        //    );

        //    totUsedPowerPlantEnergy = totProdEnergy - totAvailableEnergy;

        //    foreach (var energyConsumer in energyConsumers)
        //    {
        //        ElectricalEnergy curReqEnergy = energyConsumer.ReqEnergy();
        //        Propor energyPropor = MyMathHelper.IsTiny(value: curReqEnergy) switch
        //        {
        //            true => Propor.full,
        //            false => Propor.Create(reqEnergyByConsumer[energyConsumer], curReqEnergy)!.Value.Opposite()
        //        };
        //        if (energyConsumer.EnergyPriority == EnergyPriority.minimal && !energyPropor.IsCloseTo(other: Propor.full))
        //            throw new Exception();
        //        energyConsumer.ConsumeEnergy(energyPropor: energyPropor);
        //    }

        //    return;

        //    // remaining energy is left in energySource
        //    void DistributePartOfEnergy<T>(IEnumerable<IEnergyConsumer> energyConsumers, T energySource)
        //        where T : IEnergySouce<ElectricalEnergy>
        //    {
        //        SortedDictionary<EnergyPriority, MySet<IEnergyConsumer>> energyConsumersByPriority = new();
        //        foreach (var energyConsumer in energyConsumers)
        //        {
        //            EnergyPriority priority = energyConsumer.EnergyPriority;
        //            if (!energyConsumersByPriority.ContainsKey(key: priority))
        //                energyConsumersByPriority[priority] = new();
        //            energyConsumersByPriority[priority].Add(energyConsumer);
        //        }

        //        foreach (var (priority, samePriorEnergyConsumers) in energyConsumersByPriority)
        //        {
        //            ElectricalEnergy reqEnergy = samePriorEnergyConsumers.Sum(energyConsumer => reqEnergyByConsumer[energyConsumer]);
        //            Propor energyPropor;

        //            if (energySource.AvailableEnergy < reqEnergy)
        //            {
        //                energyPropor = (Propor)(availableEnergy / reqEnergy);
        //                availableEnergy = ElectricalEnergy.zero;
        //            }
        //            else
        //            {
        //                foreach (var energyConsumer in samePriorEnergyConsumers)
        //                    energyConsumer.ConsumeEnergyFrom(source: energySource, electricalEnergy: energyConsumer.ReqEnergy());
        //                // need to decrease the energy required by that consumer
        //                throw new NotImplementedException();
        //            }

        //            foreach (var energyConsumer in samePriorEnergyConsumers)
        //                reqEnergyByConsumer[energyConsumer] *= energyPropor.Opposite();
        //        }

        //        return availableEnergy;
        //    }
        //}

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
