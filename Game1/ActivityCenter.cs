using Game1.Events;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public abstract class ActivityCenter : IActivityCenter, IDeletable
    {
        public IEvent<IDeletedListener> Deleted
            => deleted;

        [DataMember]
        public ActivityType ActivityType { get; private init; }

        public Vector2 Position
            => state.position;

        [DataMember]
        public ulong EnergyPriority { get; private set; }

        public IEnumerable<Person> PeopleHere
            => peopleHere;

        [DataMember]
        protected readonly MyHashSet<Person> peopleHere, allPeople;
        [DataMember]
        protected readonly NodeState state;

        [DataMember]
        private readonly Event<IDeletedListener> deleted;
        [DataMember]
        private readonly HashSet<Person> peopleInProcessOfRemoving;

        protected ActivityCenter(ActivityType activityType, ulong energyPriority, NodeState state)
        {
            ActivityType = activityType;
            EnergyPriority = energyPriority;
            this.state = state;
            peopleHere = new();
            allPeople = new();

            deleted = new();
            peopleInProcessOfRemoving = new();

            AddActivityCenter(activityCenter: this);
        }

        public abstract bool IsFull();

        public abstract bool IsPersonSuitable(Person person);

        public abstract double PersonScoreOfThis(Person person);

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
        protected double DistanceToHere(Person person)
            => 1 - Math.Tanh(Vector2.Distance(person.ClosestNodePos, Position) / 100);
    }
}
