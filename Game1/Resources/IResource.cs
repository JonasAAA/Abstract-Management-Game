namespace Game1.Resources
{
    public interface IResource
    {
        public ResInd ResInd { get; }
        public Mass Mass { get; }
        public ResAmounts BasicIngredients { get; }
    }
}
