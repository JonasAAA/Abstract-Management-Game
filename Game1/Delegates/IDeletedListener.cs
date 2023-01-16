namespace Game1.Delegates
{
    public interface IDeletedListener : IListener
    {
        /// <summary>
        /// Needs to remove all mentions of deletable in the deletedListener
        /// </summary>
        /// <param name="deletable"></param>
        public void DeletedResponse(IDeletable deletable);
    }
}
