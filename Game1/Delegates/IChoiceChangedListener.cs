namespace Game1.Delegates
{
    public interface IChoiceChangedListener<TChoice>
    {
        public void ChoiceChangedResponse(TChoice prevChoice);
    }
}
