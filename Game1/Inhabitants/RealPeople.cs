using Game1.Delegates;
using Game1.Industries;
using static Game1.WorldManager;

namespace Game1.Inhabitants
{
    [Serializable]
    public class RealPeople : IEnergyConsumer, IWithRealPeopleStats
    {
        public static RealPeople CreateEmpty(LocationCounters locationCounters, IEnergyDistributor energyDistributor)
            => new(locationCounters: locationCounters, energyDistributor: energyDistributor);

        public static RealPeople CreateFromSource(RealPeople realPeopleSource)
            => CreateFromSource
            (
                realPeopleSource: realPeopleSource,
                locationCounters: realPeopleSource.locationCounters,
                energyDistributor: realPeopleSource.energyDistributor
            );

        public static RealPeople CreateFromSource(RealPeople realPeopleSource, LocationCounters locationCounters, IEnergyDistributor energyDistributor)
        {
            RealPeople newRealPeople = new(locationCounters: locationCounters, energyDistributor: energyDistributor);
            newRealPeople.TransferAllFrom(realPeopleSource: realPeopleSource);
            return newRealPeople;
        }

        public NumPeople NumPeople
            => RealPeopleStats.totalNumPeople;

        public RealPeopleStats RealPeopleStats { get; private set;}

        IEvent<IDeletedListener> IDeletable.Deleted => throw new NotImplementedException();

        private readonly LocationCounters locationCounters;
        private readonly IEnergyDistributor energyDistributor;
        private readonly Dictionary<VirtualPerson, RealPerson> virtualToRealPeople;

        private RealPeople(LocationCounters locationCounters, IEnergyDistributor energyDistributor)
        {
            RealPeopleStats = RealPeopleStats.empty;
            virtualToRealPeople = new();
            this.locationCounters = locationCounters;
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

        /// <param name="personalUpdateSkillsParams">if null, will use default update</param>
        public void Update(RealPerson.UpdateLocationParams updateLocationParams, Func<RealPerson, UpdatePersonSkillsParams?>? personalUpdateSkillsParams)
        {
            personalUpdateSkillsParams ??= realPerson => null;
            Propor energyPropor = Propor.empty;
            // Calculate energyPropor properly.
            // KEEP in mind that people get slightly more energy than they use for work, thus not all energy can be used for skill improvement
            throw new NotImplementedException();
            foreach (var realPerson in virtualToRealPeople.Values)
                realPerson.Update
                (
                    updateLocationParams: updateLocationParams,
                    updateSkillsParams: personalUpdateSkillsParams(realPerson),
                    energyPropor: energyPropor
                );
            RealPeopleStats = virtualToRealPeople.Values.CombineRealPeopleStats();
            Debug.Assert(RealPeopleStats.totalNumPeople.value == (ulong)virtualToRealPeople.Count);
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
            realPerson.SetLocationCounters(locationCounters: locationCounters);
            virtualToRealPeople.Add(key: realPerson.asVirtual, value: realPerson);
            RealPeopleStats = RealPeopleStats.CombineWith(other: realPerson.RealPeopleStats);
        }

        private bool Remove(VirtualPerson person)
        {
            if (virtualToRealPeople.Remove(key: person, value: out RealPerson? realPerson))
            {
                RealPeopleStats = RealPeopleStats.Subtract(realPerson.RealPeopleStats);
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

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => throw new NotImplementedException();
        //=> IsInActivityCenter ? totReqWatts * CurWorldManager.Elapsed * worldSecondsPerRealSecond : 0;

        void IEnergyConsumer.ConsumeEnergyFrom<T>(T source, ElectricalEnergy electricalEnergy)
        {
            throw new NotImplementedException();
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
