using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class ActivityManager
    {
        private readonly HashSet<IActivityCenter> activityCenters;

        public ActivityManager()
            => activityCenters = new();

        public void AddActivityCenter(IActivityCenter activityCenter)
        {
            if (!activityCenters.Add(activityCenter))
                throw new ArgumentException();

            activityCenter.Deleted += () => activityCenters.Remove(activityCenter);
        }

        public void ManageActivities()
        {
            HashSet<IActivityCenter> availableActivityCenters = new
            (
                from activityCenter in activityCenters
                where !activityCenter.IsFull()
                select activityCenter
            );

            Queue<Person> availablePeople = new
            (
                Person.GetActivitySeekingPeople()
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
