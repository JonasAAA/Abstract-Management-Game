namespace Game1.Delegates
{
    public interface IChoiceChangedListener<TChoice> : IListener
    {
        public void ChoiceChangedResponse(TChoice prevChoice);
    }
}
