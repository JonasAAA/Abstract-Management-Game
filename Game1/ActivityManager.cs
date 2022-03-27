using Game1.Delegates;

namespace Game1
{
    [Serializable]
    public class ActivityManager : IDeletedListener
    {
        private readonly HashSet<IActivityCenter> activityCenters;

        public ActivityManager()
            => activityCenters = new();

        public void AddActivityCenter(IActivityCenter activityCenter)
        {
            if (!activityCenters.Add(activityCenter))
                throw new ArgumentException();

            activityCenter.Deleted.Add(listener: this);
        }

        public void ManageActivities(IEnumerable<Person> people)
        {
            HashSet<IActivityCenter> availableActivityCenters = new
            (
                from activityCenter in activityCenters
                where !activityCenter.IsFull()
                select activityCenter
            );

            Queue<Person> availablePeople = new
            (
                (from person in people
                 where person.IfSeeksNewActivity()
                 select person)
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

        void IDeletedListener.DeletedResponse(IDeletable deletable)
        {
            if (deletable is IActivityCenter activityCenter)
                activityCenters.Remove(activityCenter);
            else
                throw new ArgumentException();
        }
    }
}
