namespace Game1.Resources
{
    [Serializable]
    public sealed class MaterialPalette<TProductClass> : IMaterialPalette
        where TProductClass : struct, IProductClass
    {
    }

    public interface IMaterialPalette
    {

    }
}
