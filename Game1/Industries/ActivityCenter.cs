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

        protected abstract RealPeople NewPeopleDestin { get; }
        protected readonly VirtualPeople peopleHere, allPeople;
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
            peopleHere.Add(person: person.asVirtual);
            NewPeopleDestin.TransferFrom(personSource: personSource, realPerson: person);
        }

        public abstract void UpdatePeople(RealPerson.UpdateParams updateParams);

        // TODO: delete
        //public void UpdatePeople(RealPerson.UpdateParams updateParams)
        //{
        //    peopleHere.Update
        //    (
        //        updateParams: updateParams,
        //        personalUpdate: realPerson => UpdatePerson(person: realPerson)
        //    );
        //}

        //public abstract void UpdatePerson(RealPerson person);

        public bool IsPersonHere(VirtualPerson person)
            => peopleHere.Contains(person);

        public bool IsPersonQueuedOrHere(VirtualPerson person)
            => allPeople.Contains(person);

        public abstract bool CanPersonLeave(VirtualPerson person);

        public void RemovePerson(VirtualPerson person)
        {
            if (!CanPersonLeave(person: person))
                throw new ArgumentException();
            if (peopleInProcessOfRemoving.Contains(person))
                return;
            peopleInProcessOfRemoving.Add(person);

            allPeople.Remove(person);
            if (peopleHere.Contains(person: person))
                peopleHere.Remove(person);
            person.LetGoFromActivityCenter();
            RemovePersonInternal(person: person);

            peopleInProcessOfRemoving.Remove(person);
        }

        protected abstract void RemovePersonInternal(VirtualPerson person);

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
