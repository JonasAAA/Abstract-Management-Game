using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game1
{
    public abstract class ActivityCenter : IActivityCenter
    {
        public Vector2 Position { get; }

        public ulong ElectrPriority { get; }

        public IEnumerable<Person> PeopleHere
            => peopleHere;

        public event Action Deleted;

        protected readonly MyHashSet<Person> peopleHere, allPeople;

        private readonly Action<Person> personLeft;

        protected ActivityCenter(Vector2 position, ulong electrPriority, Action<Person> personLeft)
        {
            Position = position;
            ElectrPriority = electrPriority;
            if (personLeft is null)
                throw new ArgumentNullException();
            this.personLeft = personLeft;
            peopleHere = new();
            allPeople = new();

            ActivityManager.AddActivityCenter(activityCenter: this);
        }

        public abstract bool IsFull();

        public abstract bool IsPersonSuitable(Person person);

        public abstract double PersonScoreOfThis(Person person);

        public void QueuePerson(Person person)
            => allPeople.Add(person);

        public void TakePerson(Person person)
        {
            if (!allPeople.Contains(person))
                throw new ArgumentException();
            peopleHere.Add(person);
        }

        public abstract void UpdatePerson(Person person, TimeSpan elapsed);

        public bool IsPersonHere(Person person)
            => peopleHere.Contains(person);

        public bool IsPersonQueuedOrHere(Person person)
            => allPeople.Contains(person);

        private readonly HashSet<Person> peopleInProcessOfRemoving = new();
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
                personLeft(person);
            }

            peopleInProcessOfRemoving.Remove(person);
        }

        public void Delete()
        {
            foreach (var person in allPeople)
                RemovePerson(person: person);
            Debug.Assert(allPeople.Count is 0 && peopleHere.Count is 0);
            Deleted?.Invoke();
        }

        // must be between 0 and 1 or double.NegativeInfinity
        // should later be changed to graph distance (either time or electricity cost)
        protected double DistanceToHere(Person person)
            => 1 - Math.Tanh(Vector2.Distance(person.ClosestNodePos, Position) / 100);
    }
}
