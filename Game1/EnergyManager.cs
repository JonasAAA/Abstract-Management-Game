﻿using Game1.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Game1
{
    [DataContract]
    public class EnergyManager : IDeletedListener
    {
        [DataMember]
        private readonly MyHashSet<IEnergyProducer> energyProducers;
        [DataMember]
        private readonly MyHashSet<IEnergyConsumer> energyConsumers;
        [DataMember]
        private double totReqWatts, totProdWatts, totUsedLocalWatts, totUsedPowerPlantWatts;

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

        public void DistributeEnergy(IEnumerable<Vector2> nodePositions, IReadOnlyDictionary<Vector2, Node> posToNode)
        {
            Dictionary<IEnergyConsumer, double> reqWattsByConsumer = energyConsumers.ToDictionary
            (
                keySelector: energyConsumer => energyConsumer,
                elementSelector: energyConsumer =>
                {
                    double curReqWatts = energyConsumer.ReqWatts();
                    if (curReqWatts < 0)
                        throw new Exception();
                    return curReqWatts;
                }
            );

            totProdWatts = energyProducers.Sum(energyProducer => energyProducer.ProdWatts());
            totReqWatts = energyConsumers.Sum(energyConsumer => reqWattsByConsumer[energyConsumer]);
            totUsedLocalWatts = 0;
            totUsedPowerPlantWatts = 0;

            Dictionary<Vector2, MyHashSet<IEnergyConsumer>> energyConsumersByNode = new();
            foreach (var nodePos in nodePositions)
                energyConsumersByNode[nodePos] = new();
            foreach (var energyConsumer in energyConsumers)
                energyConsumersByNode[energyConsumer.NodePos].Add(energyConsumer);
            
            foreach (var (nodePos, sameNodeEnergyConsumers) in energyConsumersByNode)
            {
                var node = posToNode[nodePos];
                double availableWatts = DistributePartOfEnergy
                (
                    energyConsumers: sameNodeEnergyConsumers,
                    availableWatts: node.LocallyProducedWatts
                );
                node.SetRemainingLocalWatts(remainingLocalWatts: availableWatts);
                totUsedLocalWatts += node.LocallyProducedWatts - availableWatts;
            }

            double totAvailableWatts = DistributePartOfEnergy
            (
                energyConsumers: energyConsumers,
                availableWatts: totProdWatts
            );

            totUsedPowerPlantWatts = totProdWatts - totAvailableWatts;

            foreach (var energyConsumer in energyConsumers)
            {
                double curReqWatts = energyConsumer.ReqWatts();
                if (C.IsTiny(value: curReqWatts))
                    curReqWatts = 0;
                var aaa = reqWattsByConsumer[energyConsumer];
                double energyPropor = curReqWatts switch
                {
                    0 => 1,
                    not 0 => 1 - reqWattsByConsumer[energyConsumer] / curReqWatts
                };
                if (C.IsTiny(value: energyPropor))
                    energyPropor = 0;
                if (C.IsTiny(value: energyPropor - 1))
                    energyPropor = 1;
                Debug.Assert(C.IsInSuitableRange(value: energyPropor));
                    
                energyConsumer.ConsumeEnergy(energyPropor: energyPropor);
            }

            // returns remaining watts
            double DistributePartOfEnergy(IEnumerable<IEnergyConsumer> energyConsumers, double availableWatts)
            {
                SortedDictionary<ulong, MyHashSet<IEnergyConsumer>> energyConsumersByPriority = new();
                foreach (var energyConsumer in energyConsumers)
                {
                    ulong priority = energyConsumer.EnergyPriority;
                    if (!energyConsumersByPriority.ContainsKey(key: priority))
                        energyConsumersByPriority[priority] = new();
                    energyConsumersByPriority[priority].Add(energyConsumer);
                }

                foreach (var (priority, samePriorEnergyConsumers) in energyConsumersByPriority)
                {
                    double reqWatts = samePriorEnergyConsumers.Sum(energyConsumer => reqWattsByConsumer[energyConsumer]),
                        energyPropor;

                    if (availableWatts < reqWatts)
                    {
                        energyPropor = availableWatts / reqWatts;
                        availableWatts = 0;
                        if (priority is 0)
                            throw new Exception();
                    }
                    else
                    {
                        energyPropor = 1;
                        availableWatts -= reqWatts;
                    }
                    Debug.Assert(C.IsInSuitableRange(value: energyPropor));

                    foreach (var energyConsumer in samePriorEnergyConsumers)
                    {
                        reqWattsByConsumer[energyConsumer] *= 1 - energyPropor;
                        Debug.Assert(reqWattsByConsumer[energyConsumer] >= 0);
                    }
                }

                return availableWatts;
            }
        }

        public string Summary()
            => $"required energy: {totReqWatts:0.##} W\nproduced energy: {totProdWatts:0.##} W\nused local energy {totUsedLocalWatts:0.##} W\nused power plant energy {totUsedPowerPlantWatts:0.##} W\n";

        void IDeletedListener.Deleted(object deletable)
        {
            if (deletable is IEnergyProducer energyProducer)
                energyProducers.Remove(energyProducer);
            if (deletable is IEnergyConsumer energyConsumer)
                energyConsumers.Remove(energyConsumer);
        }
    }
}
