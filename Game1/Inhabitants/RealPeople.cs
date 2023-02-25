using static Game1.WorldManager;

namespace Game1.Inhabitants
{
    [Serializable]
    public class RealPeople : IEnergyConsumer, IWithRealPeopleStats
    {
        public static RealPeople CreateEmpty(ThermalBody thermalBody, IEnergyDistributor energyDistributor)
            => new(thermalBody: thermalBody, energyDistributor: energyDistributor);

        public static RealPeople CreateFromSource(RealPeople realPeopleSource)
            => CreateFromSource
            (
                realPeopleSource: realPeopleSource,
                thermalBody: realPeopleSource.thermalBody,
                energyDistributor: realPeopleSource.energyDistributor
            );

        public static RealPeople CreateFromSource(RealPeople realPeopleSource, ThermalBody thermalBody, IEnergyDistributor energyDistributor)
        {
            RealPeople newRealPeople = new(thermalBody: thermalBody, energyDistributor: energyDistributor);
            newRealPeople.TransferAllFrom(realPeopleSource: realPeopleSource);
            return newRealPeople;
        }

        public NumPeople NumPeople
            => Stats.totalNumPeople;

        public RealPeopleStats Stats { get; private set;}

        private LocationCounters LocationCounters
            => thermalBody.locationCounters;
        private readonly ThermalBody thermalBody;
        private readonly IEnergyDistributor energyDistributor;
        private readonly Dictionary<VirtualPerson, RealPerson> virtualToRealPeople;
        private readonly HistoricRounder reqEnergyHistoricRounder;
        private readonly EnergyPile<ElectricalEnergy> allocElectricalEnergy;

        private RealPeople(ThermalBody thermalBody, IEnergyDistributor energyDistributor)
        {
            this.thermalBody = thermalBody;
            Stats = RealPeopleStats.empty;
            virtualToRealPeople = new();
            reqEnergyHistoricRounder = new();
            allocElectricalEnergy = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: LocationCounters);
            this.energyDistributor = energyDistributor;

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

        /// <param name="updatePersonSkillsParams">if null, will use default update</param>
        public void Update(RealPerson.UpdateLocationParams updateLocationParams, UpdatePersonSkillsParams? updatePersonSkillsParams)
        {
            thermalBody.TransformAllEnergyToHeatAndTransferFrom(source: allocElectricalEnergy);
            foreach (var realPerson in virtualToRealPeople.Values)
                realPerson.Update
                (
                    updateLocationParams: updateLocationParams,
                    updateSkillsParams: updatePersonSkillsParams,
                    allocEnergyPropor: Stats.AllocEnergyPropor
                );
            Stats = virtualToRealPeople.Values.CombineRealPeopleStats();
            Debug.Assert(Stats.totalNumPeople.value == (ulong)virtualToRealPeople.Count);
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

        public void TransferAllFrom(RealPeople realPeopleSource)
        {
            foreach (var realPerson in realPeopleSource.virtualToRealPeople.Values)
                TransferFrom(realPersonSource: realPeopleSource, realPerson: realPerson);
        }

        private void Add(RealPerson realPerson)
        {
            realPerson.ChangeLocation(newThermalBody: thermalBody);
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

        // No need to take into account the energy priority of the industry as when it is in some industry,
        // it will get energy from CombinedEnergyConsumer, which will take care of different energy priorities
        // by choosing the most important energy priority
        EnergyPriority IEnergyConsumer.EnergyPriority
            => CurWorldConfig.personEnergyPrior;
            // OLD implmementation follows
            //=> IsInActivityCenter switch
            //{
            //    // if person has higher priority then activityCenter,
            //    // then activityCenter most likely can't work at full capacity
            //    // so will not use all the available energy
            //    true => MyMathHelper.Min(CurWorldConfig.personEnergyPrior, activityCenter.EnergyPriority),
            //    false => CurWorldConfig.personEnergyPrior
            //};

        NodeID IEnergyConsumer.NodeID
            => throw new NotImplementedException();
        //=> lastNodeID;

        protected bool IsInActivityCenter
            => throw new NotImplementedException();

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => ReqEnergy();

        private ElectricalEnergy ReqEnergy()
            => ElectricalEnergy.CreateFromJoules
            (
                valueInJ: reqEnergyHistoricRounder.Round
                (
                    value: IsInActivityCenter ? Stats.totalReqWatts * (decimal)CurWorldManager.Elapsed.TotalSeconds : 0,
                    curTime: CurWorldManager.CurTime
                )
            );

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
        {
            allocElectricalEnergy.TransferFrom(source: source, amount: electricalEnergy);
            Stats = Stats with
            {
                AllocEnergyPropor = MyMathHelper.CreatePropor(part: electricalEnergy, whole: ReqEnergy())
            };
        }

#if DEBUG2
        ~RealPeople()
        {
            if (virtualToRealPeople.Count != 0)
                throw new();
        }
#endif
    }
}
