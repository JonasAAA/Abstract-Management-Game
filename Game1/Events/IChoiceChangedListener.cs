namespace Game1.Events
{
    public interface IChoiceChangedListener<TChoice> : IListener
    {
        public void ChoiceChangedResponse(TChoice prevChoice);
    }
}
