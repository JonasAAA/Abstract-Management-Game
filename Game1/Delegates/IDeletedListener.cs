namespace Game1.Delegates
{
    public interface IDeletedListener : IListener
    {
        /// <summary>
        /// Needs to remove all mentions of deletable in the deletedListener
        /// No need to remove deletedListener from deletable.Deleted
        /// </summary>
        public void DeletedResponse(IDeletable deletable);
    }
}
