using Game1.Delegates;

namespace Game1
{
    /// <summary>
    /// Combiner energy priority is the maximum of the constituents
    /// </summary>
    [Serializable]
    public sealed class CombinedEnergyConsumer : IDeletedListener, IEnergyConsumer, IEnergyDistributor
    {
        private readonly MySet<IEnergyConsumer> energyConsumers;

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
            energyConsumers = new();
            deleted = new();

            energyDistributor.AddEnergyConsumer(this);
        }

        public void Delete()
            => deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => TotalReqEnergy();

        private ElectricalEnergy TotalReqEnergy()
            => energyConsumers.Sum(energyConsumer => energyConsumer.ReqEnergy());

        [Serializable]
        private readonly record struct ConsumerWithEnergy(IEnergyConsumer EnergyConsumer, ElectricalEnergy ElectricalEnergy);

        void IEnergyConsumer.ConsumeEnergyFrom<T>(T source, ElectricalEnergy electricalEnergy)
        {
            if (electricalEnergy.IsZero)
                return;
            var totalReqEnergy = TotalReqEnergy();
            Debug.Assert(!totalReqEnergy.IsZero);
            List<ConsumerWithEnergy> energySplit =
               (from energyConsumer in energyConsumers
                select new ConsumerWithEnergy
                (
                    EnergyConsumer: energyConsumer,
                    ElectricalEnergy: ElectricalEnergy.CreateFromJoules
                    (
                        valueInJ: MyMathHelper.MultThenDivideRoundDown
                        (
                            factor1: energyConsumer.ReqEnergy().ValueInJ,
                            factor2: electricalEnergy.ValueInJ,
                            divisor: totalReqEnergy.ValueInJ
                        )
                    )
                )).ToList();
            energySplit.Sort
            (
                // This compares ElectricalEnergy / left.EnergyConsumer.ReqEnergy() (real number result) between the two
                // and if ReqEnergy is 0, then that energy consumer will be considered big, i.e. at the end of the list
                comparison: (left, right)
                    => (left.EnergyConsumer.ReqEnergy().IsZero, right.EnergyConsumer.ReqEnergy().IsZero) switch
                    {
                        (true, true) => 0,
                        (true, false) => 1,
                        (false, true) => -1,
                        (false, false) => (left.ElectricalEnergy.ValueInJ * right.EnergyConsumer.ReqEnergy().ValueInJ).CompareTo
                            (
                                right.ElectricalEnergy.ValueInJ * left.EnergyConsumer.ReqEnergy().ValueInJ
                            )
                    }
            );
            var remainingEnergy = electricalEnergy - energySplit.Sum(energyConsumer => energyConsumer.ElectricalEnergy);
            // Give the remaining energy to those that got the least of it.
            for (int i = 0; i < (int)remainingEnergy.ValueInJ; i++)
                energySplit[i] = energySplit[i] with
                {
                    ElectricalEnergy = energySplit[i].ElectricalEnergy + ElectricalEnergy.CreateFromJoules(valueInJ: 1)
                };
            Debug.Assert(energySplit.Sum(energyConsumer => energyConsumer.ElectricalEnergy) == electricalEnergy);
            foreach (var consumerWithEnergy in energySplit)
                consumerWithEnergy.EnergyConsumer.ConsumeEnergyFrom(source: source, electricalEnergy: consumerWithEnergy.ElectricalEnergy);
        }

        public void AddEnergyConsumer(IEnergyConsumer energyConsumer)
        {
            energyConsumers.Add(energyConsumer);

            if (!energyConsumer.Deleted.Contains(listener: this))
                energyConsumer.Deleted.Add(listener: this);
        }

        void IDeletedListener.DeletedResponse(IDeletable deletable)
        {
            if (deletable is IEnergyConsumer energyConsumer)
                energyConsumers.Remove(energyConsumer);
        }
    }
}
