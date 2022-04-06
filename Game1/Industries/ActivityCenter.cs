using Game1.Delegates;
using Game1.PrimitiveTypeWrappers;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public abstract class ActivityCenter : IActivityCenter, IDeletable
    {
        public IEvent<IDeletedListener> Deleted
            => deleted;

        public ActivityType ActivityType { get; }

        public MyVector2 Position
            => state.position;

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

        // must be between 0 and 1 or double.NegativeInfinity
        // should later be changed to graph distance (either time or energy cost)
        protected Score DistanceToHere(Person person)
            // TODO: get rid of hard-coded constant
            => Score.FromUnboundedUDouble(value: MyVector2.Distance(person.ClosestNodePos, Position), valueGettingAverageScore: 100).Opposite();
            //1 - MyMathHelper.Tanh(MyVector2.Distance(person.ClosestNodePos, Position) / 100);
    }
}
