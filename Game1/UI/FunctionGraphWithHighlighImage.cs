namespace Game1.UI
{
    [Serializable]
    public sealed class FunctionGraphWithHighlighImage<TX, TY> : IImage
        where TX : struct, IScalar<TX>
        where TY : struct, IScalar<TY>
    {
        public interface IHighlightInterval
        {
            public (TX start, TX stop, Color highlightColor) GetHighlightInterval();
        }

        private readonly FunctionGraphImage<TX, TY> functionGraph;
        private readonly IHighlightInterval highlightInterval;

        public UDouble Width
            => functionGraph.Width;

        public UDouble Height
            => functionGraph.Height;

        public FunctionGraphWithHighlighImage(FunctionGraphImage<TX, TY> functionGraph, IHighlightInterval highlightInterval)
        {
            this.functionGraph = functionGraph;
            this.highlightInterval = highlightInterval;
        }

        public void Draw(MyVector2 center)
            => functionGraph.Draw(center: center, highlightInterval: highlightInterval.GetHighlightInterval());
    }

    public static class FunctionGraphWithHighlighImage
    {
        public static IEnumerable<Type> GetKnownTypes()
            => from typeArgs in FunctionGraphImage.GetKnownTypeArgs()
               select typeof(FunctionGraphWithHighlighImage<,>).MakeGenericType(typeArgs);
    }
}
