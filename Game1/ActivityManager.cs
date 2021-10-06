using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public static class ActivityManager
    {
        private static readonly HashSet<IActivityCenter> activityCenters;

        static ActivityManager()
            => activityCenters = new();

        public static void AddActivityCenter(IActivityCenter activityCenter)
        {
            if (!activityCenters.Add(activityCenter))
                throw new ArgumentException();

            activityCenter.Deleted += () => activityCenters.Remove(activityCenter);
        }

        public static void ManageActivities()
        {
            HashSet<IActivityCenter> availableActivityCenters = new
            (
                collection:
                    from activityCenter in activityCenters
                    where !activityCenter.IsFull()
                    select activityCenter
            );

            Queue<Person> availablePeople = new
            (
                collection: Person.GetActivitySeekingPeople()
                    .OrderBy(person => C.Random(min: 0, max: 1))
            );

            while (availablePeople.Count > 0)
            {
                var person = availablePeople.Dequeue();
                IActivityCenter activityCenter = (IActivityCenter)person.ChooseActivityCenter
                (
                    activityCenters:
                        from possibActivityCenter in availableActivityCenters
                        where possibActivityCenter.IsPersonSuitable(person: person)
                        select possibActivityCenter
                );

                if (!activityCenter.IsPersonQueuedOrHere(person: person))
                    activityCenter.QueuePerson(person: person);

                if (activityCenter.IsFull())
                    availableActivityCenters.Remove(activityCenter);
            }
        }
    }
}
