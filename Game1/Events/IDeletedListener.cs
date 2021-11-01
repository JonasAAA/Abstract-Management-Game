namespace Game1.Events
{
    public interface IDeletedListener
    {
        public void DeletedResponse(IDeletable deletable);
    }
}
