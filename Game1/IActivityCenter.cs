using Game1.Inhabitants;

namespace Game1
{
    /// <summary>
    /// MUST call ActivityManager.AddActivityCenter() for each instance
    /// </summary>
    public interface IActivityCenter : IPersonFacingActivityCenter, IDeletable
    {
        public static void UpdatePersonDefault(RealPerson person)
        {
            // TODO calculate happiness
            // may decrease person's skills
        }

        public bool IsFull();

        public bool IsPersonSuitable(VirtualPerson person);

        public bool IsPersonQueuedOrHere(VirtualPerson person);

        public void QueuePerson(VirtualPerson person);
    }
}
