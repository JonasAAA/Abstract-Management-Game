using Game1.Delegates;
using Game1.Inhabitants;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public abstract class ActivityCenter : IActivityCenter, IDeletable
    {
        public IEvent<IDeletedListener> Deleted
            => deleted;

        public ActivityType ActivityType { get; }

        public NodeID NodeID
            => state.NodeID;

        public EnergyPriority EnergyPriority { get; private set; }

        public RealPeopleStats PeopleHereStats
            => realPeopleHere.RealPeopleStats;

        protected readonly RealPeople realPeopleHere;
        protected readonly VirtualPeople allPeople;
        protected readonly IIndustryFacingNodeState state;

        private readonly Event<IDeletedListener> deleted;
        private readonly VirtualPeople peopleInProcessOfRemoving;

        protected ActivityCenter(IEnergyDistributor energyDistributor, ActivityType activityType, EnergyPriority energyPriority, IIndustryFacingNodeState state)
        {
            ActivityType = activityType;
            EnergyPriority = energyPriority;
            this.state = state;
            realPeopleHere = RealPeople.CreateEmpty(thermalBody: state.ThermalBody, energyDistributor: energyDistributor);
            allPeople = new();

            deleted = new();
            peopleInProcessOfRemoving = new();

            CurWorldManager.AddActivityCenter(activityCenter: this);
        }

        public abstract bool IsFull();

        public abstract bool IsPersonSuitable(VirtualPerson person);

        public abstract Score PersonEnjoymentOfThis(VirtualPerson person);

        public void QueuePerson(VirtualPerson person)
            => allPeople.Add(person);

        public virtual void TakePersonFrom(RealPeople realPersonSource, RealPerson realPerson)
        {
            if (!allPeople.Contains(realPerson.asVirtual))
                throw new ArgumentException();
            realPeopleHere.TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);
        }

        public void UpdatePeople(RealPerson.UpdateLocationParams updateLocationParams)
            => realPeopleHere.Update
            (
                updateLocationParams: updateLocationParams,
                personalUpdateSkillsParams: PersonUpdateParams
            );

        protected abstract UpdatePersonSkillsParams? PersonUpdateParams(RealPerson realPerson);

        public bool IsPersonHere(VirtualPerson person)
            => realPeopleHere.Contains(person);

        public bool IsPersonQueuedOrHere(VirtualPerson person)
            => allPeople.Contains(person);

        public abstract bool CanPersonLeave(VirtualPerson person);

        public void RemovePerson(VirtualPerson person, bool force = false)
        {
            if (peopleInProcessOfRemoving.Contains(person))
                return;
            peopleInProcessOfRemoving.Add(person);
            
            if (!force && !CanPersonLeave(person: person))
                throw new ArgumentException();

            RemovePersonInternal(person: person, force: force);

            allPeople.Remove(person);
            person.LetGoFromActivityCenter();
            state.WaitingPeople.TransferFromIfPossible(realPersonSource: realPeopleHere, person: person);

            peopleInProcessOfRemoving.Remove(person);
        }

        protected virtual void RemovePersonInternal(VirtualPerson person, bool force)
        { }

        public void Delete()
        {
            foreach (var person in allPeople)
                RemovePerson(person: person);
            Debug.Assert(allPeople.Count.IsZero && PeopleHereStats.totalNumPeople.IsZero);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }
    }
}
