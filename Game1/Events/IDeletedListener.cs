namespace Game1.Events
{
    public interface IDeletedListener : IListener
    {
        public void DeletedResponse(IDeletable deletable);
    }
}
