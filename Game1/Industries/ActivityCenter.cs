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

        protected readonly RealPeople peopleHere;
        protected readonly VirtualPeople allPeople;
        protected readonly IIndustryFacingNodeState state;

        private readonly Event<IDeletedListener> deleted;
        private readonly VirtualPeople peopleInProcessOfRemoving;

        protected ActivityCenter(ActivityType activityType, EnergyPriority energyPriority, IIndustryFacingNodeState state)
        {
            ActivityType = activityType;
            EnergyPriority = energyPriority;
            this.state = state;
            peopleHere = new();
            allPeople = new();

            deleted = new();
            peopleInProcessOfRemoving = new();

            CurWorldManager.AddActivityCenter(activityCenter: this);
        }

        public abstract bool IsFull();

        public abstract bool IsPersonSuitable(VirtualPerson person);

        public abstract Score PersonScoreOfThis(VirtualPerson person);

        public void QueuePerson(VirtualPerson person)
            => allPeople.Add(person);

        public virtual void TakePersonFrom(RealPeople personSource, RealPerson person)
        {
            if (!allPeople.Contains(person.asVirtual))
                throw new ArgumentException();
            peopleHere.TransferFrom(personSource: personSource, realPerson: person);
        }

        public void UpdatePeople(RealPerson.UpdateParams updateParams)
            => peopleHere.Update
            (
                updateParams: updateParams,
                personalUpdate: realPerson => UpdatePerson(person: realPerson)
            );

        protected abstract void UpdatePerson(RealPerson person);

        public bool IsPersonHere(VirtualPerson person)
            => peopleHere.Contains(person);

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
            state.WaitingPeople.TransferFromIfPossible(personSource: peopleHere, virtualPerson: person);

            peopleInProcessOfRemoving.Remove(person);
        }

        protected virtual void RemovePersonInternal(VirtualPerson person, bool force)
        { }

        public void Delete()
        {
            foreach (var person in allPeople)
                RemovePerson(person: person);
            Debug.Assert(allPeople.Count is 0 && peopleHere.Count is 0);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        /// <summary>
        /// Used this to calculate personal score
        /// </summary>
        protected Score DistanceToHereAsPerson(VirtualPerson person)
            // TODO: get rid of hard-coded constant
            => Score.FromUnboundedUDouble(value: CurWorldManager.PersonDist(nodeID1: person.ClosestNodeID, nodeID2: NodeID), valueGettingAverageScore: 2).Opposite();

        /// <summary>
        /// Used this to calculate suitability of person
        /// </summary>
        protected Score DistanceToHereAsRes(VirtualPerson person)
            // TODO: get rid of hard-coded constant
            => Score.FromUnboundedUDouble(value: CurWorldManager.ResDist(nodeID1: person.ClosestNodeID, nodeID2: NodeID), valueGettingAverageScore: 2).Opposite();
    }
}
