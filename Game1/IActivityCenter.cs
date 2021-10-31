namespace Game1
{
    /// <summary>
    /// MUST call ActivityManager.AddActivityCenter() for each instance
    /// </summary>
    public interface IActivityCenter : IPersonFacingActivityCenter, IDeletable
    {
        public static void UpdatePersonDefault(Person person)
        {
            // TODO calculate happiness
            // may decrease person's skills
        }

        public bool IsFull();

        public bool IsPersonSuitable(Person person);

        public bool IsPersonQueuedOrHere(Person person);

        public void QueuePerson(Person person);
    }
}
