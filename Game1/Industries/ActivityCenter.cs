using Game1.Delegates;
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
            => state.nodeID;

        public EnergyPriority EnergyPriority { get; private set; }

        public IEnumerable<Person> PeopleHere
            => peopleHere;

        protected readonly MySet<Person> peopleHere, allPeople;
        protected readonly NodeState state;

        private readonly Event<IDeletedListener> deleted;
        private readonly HashSet<Person> peopleInProcessOfRemoving;

        protected ActivityCenter(ActivityType activityType, EnergyPriority energyPriority, NodeState state)
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

        public abstract bool IsPersonSuitable(Person person);

        public abstract Score PersonScoreOfThis(Person person);

        public void QueuePerson(Person person)
            => allPeople.Add(person);

        public virtual void TakePerson(Person person)
        {
            if (!allPeople.Contains(person))
                throw new ArgumentException();
            peopleHere.Add(person);
        }

        public abstract void UpdatePerson(Person person);

        public bool IsPersonHere(Person person)
            => peopleHere.Contains(person);

        public bool IsPersonQueuedOrHere(Person person)
            => allPeople.Contains(person);

        public abstract bool CanPersonLeave(Person person);

        public void RemovePerson(Person person)
        {
            if (peopleInProcessOfRemoving.Contains(person))
                return;
            peopleInProcessOfRemoving.Add(person);

            allPeople.Remove(person);
            person.LetGoFromActivityCenter();
            if (IsPersonHere(person: person))
            {
                peopleHere.Remove(person);
                state.waitingPeople.Add(person);
            }

            peopleInProcessOfRemoving.Remove(person);
        }

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
        protected Score DistanceToHereAsPerson(Person person)
            // TODO: get rid of hard-coded constant
            => Score.FromUnboundedUDouble(value: CurWorldManager.PersonDist(nodeID1: person.ClosestNodeID, nodeID2: NodeID), valueGettingAverageScore: 2).Opposite();

        /// <summary>
        /// Used this to calculate suitability of person
        /// </summary>
        protected Score DistanceToHereAsRes(Person person)
            // TODO: get rid of hard-coded constant
            => Score.FromUnboundedUDouble(value: CurWorldManager.ResDist(nodeID1: person.ClosestNodeID, nodeID2: NodeID), valueGettingAverageScore: 2).Opposite();
    }
}
