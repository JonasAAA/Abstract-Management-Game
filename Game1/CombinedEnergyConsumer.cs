﻿using Game1.Collections;
using Game1.Delegates;

namespace Game1
{
    /// <summary>
    /// Combiner energy priority is the maximum of the constituents
    /// </summary>
    [Serializable]
    public sealed class CombinedEnergyConsumer : IEnergyConsumer, IEnergyDistributor, IDeletedListener, IDeletable
    {
        private readonly ThrowingSet<IEnergyConsumer> energyConsumers;

        EnergyPriority IEnergyConsumer.EnergyPriority
            => energyConsumers.MaxOrDefault(selector: energyConsumer => energyConsumer.EnergyPriority);

        /// <summary>
        /// node from which consume energy
        /// </summary>
        NodeID IEnergyConsumer.NodeID
            => nodeID;

        IEvent<IDeletedListener> IDeletable.Deleted
            => deleted;

        private readonly NodeID nodeID;
        private readonly Event<IDeletedListener> deleted;

        public CombinedEnergyConsumer(NodeID nodeID, IEnergyDistributor energyDistributor)
        {
            this.nodeID = nodeID;
            energyConsumers = [];
            deleted = new();

            energyDistributor.AddEnergyConsumer(this);
        }

        public void Delete()
        {
            foreach (var energyConsumer in energyConsumers)
                (energyConsumer as IDeletable)?.Deleted.Remove(listener: this);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        public ElectricalEnergy ReqEnergy()
            => energyConsumers.Sum(energyConsumer => energyConsumer.ReqEnergy());

        [Serializable]
        private readonly record struct ConsumerWithEnergy(IEnergyConsumer EnergyConsumer, ElectricalEnergy ElectricalEnergy);

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
        {
            // Create this list of energyConsumers to ensure that looping through the list of them twice preserves ordering
            // HashSet foreach ordering is very unlikely to change if the collection is not mutated
            // https://stackoverflow.com/questions/27065754/relying-on-the-iteration-order-of-an-unmodified-hashset
            // Though that is technically an implementation detail and doesn't have to be true in future versions
            var localEnergyConsumers = energyConsumers.ToList();
            var (allocEnergies, unusedEnergy) = Algorithms.SplitEnergyEvenly
            (
                reqEnergies: localEnergyConsumers.Select(energyConsumer => energyConsumer.ReqEnergy()).ToList(),
                availableEnergy: electricalEnergy
            );
            Debug.Assert(unusedEnergy.IsZero);
            foreach (var (energyConsumer, allocEnergy) in localEnergyConsumers.Zip(allocEnergies))
                energyConsumer.ConsumeEnergyFrom(source: source, electricalEnergy: allocEnergy);
        }

        public void AddEnergyConsumer(IEnergyConsumer energyConsumer)
        {
            energyConsumers.Add(energyConsumer);

            (energyConsumer as IDeletable)?.Deleted.Add(listener: this);
        }

        void IDeletedListener.DeletedResponse(IDeletable deletable)
        {
            if (deletable is IEnergyConsumer energyConsumer)
                energyConsumers.Remove(energyConsumer);
        }
    }
}
