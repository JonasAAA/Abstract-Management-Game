using Game1.Delegates;

namespace Game1
{
    [Serializable]
    public class EnergyManager : IDeletedListener
    {
        private readonly MySet<IEnergyProducer> energyProducers;
        private readonly MySet<IEnergyConsumer> energyConsumers;
        private UDouble totReqWatts, totProdWatts, totUsedLocalWatts, totUsedPowerPlantWatts;

        public EnergyManager()
        {
            energyProducers = new();
            energyConsumers = new();
            totReqWatts = 0;
            totProdWatts = 0;
            totUsedLocalWatts = 0;
            totUsedPowerPlantWatts = 0;
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

        public void DistributeEnergy(IEnumerable<NodeID> nodeIDs, Func<NodeID, INodeAsLocalEnergyProducer> nodeIDToNode)
        {
            Dictionary<IEnergyConsumer, UDouble> reqWattsByConsumer = energyConsumers.ToDictionary
            (
                keySelector: energyConsumer => energyConsumer,
                elementSelector: energyConsumer => energyConsumer.ReqWatts()
            );

            totProdWatts = energyProducers.Sum(energyProducer => energyProducer.ProdWatts());
            totReqWatts = energyConsumers.Sum(energyConsumer => reqWattsByConsumer[energyConsumer]);
            totUsedLocalWatts = 0;
            totUsedPowerPlantWatts = 0;

            Dictionary<NodeID, MySet<IEnergyConsumer>> energyConsumersByNode = new();
            foreach (var nodeID in nodeIDs)
                energyConsumersByNode[nodeID] = new();
            foreach (var energyConsumer in energyConsumers)
                energyConsumersByNode[energyConsumer.NodeID].Add(energyConsumer);

            foreach (var (nodeID, sameNodeEnergyConsumers) in energyConsumersByNode)
            {
                var node = nodeIDToNode(nodeID);
                UDouble availableWatts = DistributePartOfEnergy
                (
                    energyConsumers: sameNodeEnergyConsumers,
                    availableWatts: node.LocallyProducedWatts
                );
                node.SetRemainingLocalWatts(remainingLocalWatts: availableWatts);
                totUsedLocalWatts += (UDouble)(node.LocallyProducedWatts - availableWatts);
            }

            UDouble totAvailableWatts = DistributePartOfEnergy
            (
                energyConsumers: energyConsumers,
                availableWatts: totProdWatts
            );

            totUsedPowerPlantWatts = (UDouble)(totProdWatts - totAvailableWatts);

            foreach (var energyConsumer in energyConsumers)
            {
                UDouble curReqWatts = energyConsumer.ReqWatts();
                Propor energyPropor = MyMathHelper.IsTiny(value: curReqWatts) switch
                {
                    true => Propor.full,
                    false => Propor.Create(reqWattsByConsumer[energyConsumer], curReqWatts)!.Value.Opposite()
                };
                if (energyConsumer.EnergyPriority == EnergyPriority.minimal && !energyPropor.IsCloseTo(other: Propor.full))
                    throw new Exception();
                energyConsumer.ConsumeEnergy(energyPropor: energyPropor);
            }

            return;

            // returns remaining watts
            UDouble DistributePartOfEnergy(IEnumerable<IEnergyConsumer> energyConsumers, UDouble availableWatts)
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
                    UDouble reqWatts = samePriorEnergyConsumers.Sum(energyConsumer => reqWattsByConsumer[energyConsumer]);
                    Propor energyPropor;

                    if (availableWatts < reqWatts)
                    {
                        energyPropor = (Propor)(availableWatts / reqWatts);
                        availableWatts = 0;
                    }
                    else
                    {
                        energyPropor = Propor.full;
                        availableWatts = (UDouble)(availableWatts - reqWatts);
                    }

                    foreach (var energyConsumer in samePriorEnergyConsumers)
                        reqWattsByConsumer[energyConsumer] *= energyPropor.Opposite();
                }

                return availableWatts;
            }
        }

        public string Summary()
            => $"required energy: {totReqWatts:0.##} W\nproduced energy: {totProdWatts:0.##} W\nused local energy {totUsedLocalWatts:0.##} W\nused power plant energy {totUsedPowerPlantWatts:0.##} W\n";

        void IDeletedListener.DeletedResponse(IDeletable deletable)
        {
            if (deletable is IEnergyProducer energyProducer)
                energyProducers.Remove(energyProducer);
            if (deletable is IEnergyConsumer energyConsumer)
                energyConsumers.Remove(energyConsumer);
        }
    }
}
