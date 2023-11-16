using Game1.Delegates;
using Game1.Inhabitants;

namespace Game1
{
    [Serializable]
    public sealed class ActivityManager : IDeletedListener
    {
        private readonly HashSet<IActivityCenter> activityCenters;

        public ActivityManager()
            => activityCenters = [];

        public void AddActivityCenter(IActivityCenter activityCenter)
        {
            if (!activityCenters.Add(activityCenter))
                throw new ArgumentException();

            activityCenter.Deleted.Add(listener: this);
        }

        public void ManageActivities(VirtualPeople people)
        {
            HashSet<IActivityCenter> availableActivityCenters = new
            (
                from activityCenter in activityCenters
                where !activityCenter.IsFull()
                select activityCenter
            );

            Queue<VirtualPerson> availablePeople = new
            (
                (from person in people
                 where person.IfSeeksNewActivity()
                 select person)
                .OrderBy(person => C.Random(min: 0.0, max: 1))
            );

            while (availablePeople.Count > 0)
            {
                var person = availablePeople.Dequeue();
                var activityCenter = (IActivityCenter)person.ChooseActivityCenter
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
