using Game1.Delegates;
using static Game1.WorldManager;

namespace Game1.Inhabitants
{
    [Serializable]
    public sealed class RealPeople : IEnergyConsumer, IDeletable, IWithRealPeopleStats
    {
        public static RealPeople CreateEmpty(ThermalBody thermalBody, IEnergyDistributor energyDistributor, NodeID electricalEnergySourceNodeID, NodeID closestNodeID, bool isInActivityCenter)
            => new
            (
                thermalBody: thermalBody,
                energyDistributor: energyDistributor,
                electricalEnergySourceNodeID: electricalEnergySourceNodeID,
                closestNodeID: closestNodeID,
                isInActivityCenter: isInActivityCenter
            );

        public static RealPeople CreateFromSource(RealPeople realPeopleSource, ThermalBody thermalBody, IEnergyDistributor energyDistributor, NodeID electricalEnergySourceNodeID, NodeID closestNodeID, bool isInActivityCenter)
        {
            RealPeople newRealPeople = new
            (
                thermalBody: thermalBody,
                energyDistributor: energyDistributor,
                electricalEnergySourceNodeID: electricalEnergySourceNodeID,
                closestNodeID: closestNodeID,
                isInActivityCenter: isInActivityCenter
            );
            newRealPeople.TransferAllFrom(realPeopleSource: realPeopleSource);
            return newRealPeople;
        }

        public IEvent<IDeletedListener> Deleted
            => deleted;

        public NumPeople NumPeople
            => Stats.totalNumPeople;

        public RealPeopleStats Stats { get; private set;}

        private LocationCounters LocationCounters
            => thermalBody.locationCounters;
        private readonly Event<IDeletedListener> deleted;
        private readonly ThermalBody thermalBody;
        private readonly Dictionary<VirtualPerson, RealPerson> virtualToRealPeople;
        private readonly HistoricRounder reqEnergyHistoricRounder;
        private readonly EnergyPile<ElectricalEnergy> allocElectricalEnergy;
        private readonly NodeID electricalEnergySourceNodeID, closestNodeID;
        private readonly bool isInActivityCenter;

        private RealPeople(ThermalBody thermalBody, IEnergyDistributor energyDistributor, NodeID electricalEnergySourceNodeID, NodeID closestNodeID, bool isInActivityCenter)
        {
            this.thermalBody = thermalBody;
            deleted = new();
            Stats = RealPeopleStats.empty;
            virtualToRealPeople = new();
            reqEnergyHistoricRounder = new();
            allocElectricalEnergy = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: LocationCounters);
            this.closestNodeID = closestNodeID;
            this.electricalEnergySourceNodeID = electricalEnergySourceNodeID;
            this.isInActivityCenter = isInActivityCenter;

            energyDistributor.AddEnergyConsumer(energyConsumer: this);
        }

        public void AddByMagic(RealPerson realPerson)
            => Add(realPerson: realPerson);

        /// <summary>
        /// Unlike the usual foreach loop, can change the collection with personalAction
        /// </summary>
        public void ForEach(Action<RealPerson> personalAction)
        {
            // ToList is more performant than ToArray according to https://stackoverflow.com/a/16323412
            // TODO(performance) if this is too slow, this method could take a "needCopy" argument and copy the Values only if that's needed
            foreach (var person in virtualToRealPeople.Values.ToList())
                personalAction(person);
        }

        /// <param Name="updatePersonSkillsParams">if null, will use default update</param>
        public void Update(UpdatePersonSkillsParams? updatePersonSkillsParams)
        {
            thermalBody.TransformAllEnergyToHeatAndTransferFrom(source: allocElectricalEnergy);
            foreach (var realPerson in virtualToRealPeople.Values)
                realPerson.Update
                (
                    updateSkillsParams: updatePersonSkillsParams,
                    allocEnergyPropor: Stats.AllocEnergyPropor
                );
            Stats = virtualToRealPeople.Values.CombineRealPeopleStats();
            Debug.Assert(Stats.totalNumPeople.value == (uint)virtualToRealPeople.Count);
        }

        public bool Contains(VirtualPerson person)
            => virtualToRealPeople.ContainsKey(person);

        public void TransferFromIfPossible(RealPeople realPersonSource, VirtualPerson person)
        {
            if (realPersonSource.virtualToRealPeople.TryGetValue(key: person, value: out RealPerson? realPerson))
                TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);
        }

        public void TransferFrom(RealPeople realPersonSource, RealPerson realPerson)
        {
            if (!realPersonSource.Remove(person: realPerson.asVirtual))
                throw new ArgumentException();
            Add(realPerson: realPerson);
        }

        private void TransferAllFrom(RealPeople realPeopleSource)
        {
            foreach (var realPerson in realPeopleSource.virtualToRealPeople.Values)
                TransferFrom(realPersonSource: realPeopleSource, realPerson: realPerson);
        }

        private void Add(RealPerson realPerson)
        {
            realPerson.ChangeLocation(newThermalBody: thermalBody, closestNodeID: closestNodeID);
            virtualToRealPeople.Add(key: realPerson.asVirtual, value: realPerson);
            Stats = Stats.CombineWith(other: realPerson.Stats);
        }

        private bool Remove(VirtualPerson person)
        {
            if (virtualToRealPeople.Remove(key: person, value: out RealPerson? realPerson))
            {
                Stats = Stats.Subtract(realPerson.Stats);
                return true;
            }
            return false;
        }

        public void TransferAllFromAndDeleteSource(RealPeople realPeopleSource)
        {
            TransferAllFrom(realPeopleSource: realPeopleSource);
            realPeopleSource.Delete();
        }

        /// <summary>
        /// Call this ONLY when removed all people from here already
        /// </summary>
        public void Delete()
        {
            if (virtualToRealPeople.Count > 0)
                throw new InvalidOperationException();
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        // No need to take into account the energy priority of the industry as when it is in some industry,
        // it will get energy from CombinedEnergyConsumer, which will take care of different energy priorities
        // by choosing the most important energy priority
        EnergyPriority IEnergyConsumer.EnergyPriority
            => CurWorldConfig.personEnergyPrior;

        NodeID IEnergyConsumer.NodeID
            => electricalEnergySourceNodeID;

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => ReqEnergy();

        private ElectricalEnergy ReqEnergy()
            => ElectricalEnergy.CreateFromJoules
            (
                valueInJ: reqEnergyHistoricRounder.Round
                (
                    value: isInActivityCenter ? Stats.totalReqWatts * (decimal)CurWorldManager.Elapsed.TotalSeconds : 0,
                    curTime: CurWorldManager.CurTime
                )
            );

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
        {
            allocElectricalEnergy.TransferFrom(source: source, amount: electricalEnergy);
            var allocEnergyPropor = MyMathHelper.CreatePropor(part: electricalEnergy, whole: ReqEnergy());
            foreach (var realPerson in virtualToRealPeople.Values)
                realPerson.UpdateAllocEnergyPropor(newAllocEnergyPropor: allocEnergyPropor);
            Stats = Stats with
            {
                AllocEnergyPropor = allocEnergyPropor
            };
        }

#if DEBUG2
        ~RealPeople()
        {
            if (virtualToRealPeople.Count > 0)
                throw new();
        }
#endif
    }
}
