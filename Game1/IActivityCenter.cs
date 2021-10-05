using System;

namespace Game1
{
    /// <summary>
    /// MUST call ActivityManager.AddActivityCenter() for each instance
    /// </summary>
    public interface IActivityCenter : IPersonFacingActivityCenter
    {
        public static void UpdatePersonDefault(Person person, TimeSpan elapsed)
        {
            // TODO calculate happiness
            // may decrease person's skills
        }

        public event Action Deleted;

        public bool IsFull();

        public bool IsPersonSuitable(Person person);

        public bool IsPersonQueuedOrHere(Person person);

        public void QueuePerson(Person person);
    }
}
