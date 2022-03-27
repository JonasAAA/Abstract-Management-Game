namespace Game1.Delegates
{
    public interface IDeletedListener : IListener
    {
        public void DeletedResponse(IDeletable deletable);
    }
}
