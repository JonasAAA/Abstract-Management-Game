using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class FunctionGraphImage<TX, TY> : IImage
        where TX : struct, IScalar<TX>
        where TY : struct, IScalar<TY>
    {
        public UDouble Width { get; }
        public UDouble Height { get; }

        private readonly Color lineColor, backgroundColor;
        private readonly UDouble lineWidth;
        private readonly TX minX, maxX;
        private readonly TY minY, maxY;
        private readonly ulong numXSamples;
        private bool drawPoints;
        private readonly Vector2Bare[] points;

        public FunctionGraphImage(UDouble width, UDouble height, Color lineColor, Color backgroundColor, UDouble lineWidth, TX minX, TX maxX, TY minY, TY maxY, ulong numXSamples, Func<TX, TY>? func)
        {
            Width = width;
            Height = height;
            this.lineColor = lineColor;
            this.backgroundColor = backgroundColor;
            this.lineWidth = lineWidth;
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            this.numXSamples = numXSamples;
            points = new Vector2Bare[numXSamples];
            SetFunction(func: func);
        }

        public void SetFunction(Func<TX, TY>? func)
        {
            if (func is null)
                drawPoints = false;
            else
            {
                drawPoints = true;
                for (ulong ind = 0; ind < numXSamples; ind++)
                {
                    // between 0 and 1
                    var normalizedX = Algorithms.Normalize(value: ind, start: 0, stop: numXSamples - 1);

                    var x = TX.Interpolate(normalized: normalizedX, start: minX, stop: maxX);
                    var y = func(x);

                    var normalizedY = TY.Normalize(value: y, start: minY, stop: maxY);
                    points[ind] = new Vector2Bare
                    (
                        x: ImageXFromNormalized(normalizedX: normalizedX),
                        y: -Algorithms.Interpolate
                        (
                            normalized: normalizedY,
                            start: 0,
                            stop: Height
                        )
                    );
                }
            }
        }

        private UDouble ImageXFromNormalized(Propor normalizedX)
            => Algorithms.Interpolate(normalized: normalizedX, start: UDouble.zero, stop: Width);

        public void Draw(Vector2Bare center, (TX start, TX stop, Color highlightColor)? highlightInterval)
        {
            var shape = new MyRectangle(width: Width, height: Height)
            {
                Center = center
            };

            shape.Draw(color: backgroundColor);

            if (drawPoints)
                for (int ind = 0; ind < points.Length - 1; ind++)
                    SegmentAlgos.Draw
                    (
                        startPos: shape.BottomLeftCorner + points[ind],
                        endPos: shape.BottomLeftCorner + points[ind + 1],
                        width: lineWidth,
                        color: lineColor
                    );
            if (highlightInterval is (TX start, TX stop, Color highlightColor))
            {
                UDouble funcXToImageX(TX x)
                    => ImageXFromNormalized
                    (
                        normalizedX: TX.Normalize(value: x, start: minX, stop: maxX)
                    );
                var imageXStart = funcXToImageX(x: start);
                var imageXStop = funcXToImageX(x: stop);
                // This is needed in case start > stop
                var highlightMin = MyMathHelper.Min(imageXStart, imageXStop);
                var highlightMax = MyMathHelper.Max(imageXStart, imageXStop);
                new MyRectangle(width: highlightMax - highlightMin, height: Height)
                {
                    BottomLeftCorner = shape.BottomLeftCorner + new Vector2Bare(x: highlightMin, y: 0)
                }.Draw(color: highlightColor);
            }
        }

        void IImage.Draw(Vector2Bare center)
            => Draw(center: center, highlightInterval: null);
    }

    public static class FunctionGraphImage
    {
        public static List<Type[]> GetKnownTypeArgs()
            => new()
            {
                new[] { typeof(Temperature), typeof(Propor) },
                new[] { typeof(UDouble), typeof(Propor)}
            };

        public static IEnumerable<Type> GetKnownTypes()
            => from typeArgs in GetKnownTypeArgs()
               select typeof(FunctionGraphImage<,>).MakeGenericType(typeArgs);
    }

    //[Serializable]
    //public sealed class FunctionGraph : HUDElement
    //{
    //    protected sealed override Color Color { get; }

    //    private readonly Color lineColor;
    //    private readonly UDouble lineWidth;
    //    private readonly double minX, maxX, minY, maxY;
    //    private readonly ulong numXSamples;
    //    private bool drawPoints;
    //    private readonly Vector2Bare[] points;

    //    public FunctionGraph(UDouble width, UDouble height, Color lineColor, Color backgroundColor, UDouble lineWidth, double minX, double maxX, double minY, double maxY, ulong numXSamples, Func<double, double>? func)
    //        : base(shape: new MyRectangle(width: width, height: height))
    //    {
    //        Color = backgroundColor;
    //        this.lineColor = lineColor;
    //        this.lineWidth = lineWidth;
    //        this.minX = minX;
    //        this.maxX = maxX;
    //        this.minY = minY;
    //        this.maxY = maxY;
    //        this.numXSamples = numXSamples;
    //        points = new Vector2Bare[numXSamples];
    //        SetFunction(func: func);
    //    }

    //    public void SetFunction(Func<double, double>? func)
    //    {
    //        if (func is null)
    //            drawPoints = false;
    //        else
    //        {
    //            drawPoints = true;
    //            for (ulong ind = 0; ind < numXSamples; ind++)
    //            {
    //                // between 0 and 1
    //                var normalizedX = Algorithms.Normalize(value: ind, start: 0, stop: numXSamples - 1);

    //                double x = Algorithms.Interpolate(normalized: normalizedX, start: minX, stop: maxX);
    //                double y = func(x);

    //                var normalizedY = Algorithms.Normalize(value: y, start: minY, stop: maxY);
    //                points[ind] = new Vector2Bare
    //                (
    //                    x: Algorithms.Interpolate(normalized: normalizedX, start: 0, stop: Shape.Width),
    //                    y: -Algorithms.Interpolate
    //                    (
    //                        normalized: normalizedY,
    //                        start: 0,
    //                        stop: Shape.Height
    //                    )
    //                );
    //            }
    //        }
    //    }

    //    protected override void DrawChildren()
    //    {
    //        base.DrawChildren();
    //        if (drawPoints)
    //            for (int ind = 0; ind < points.Length - 1; ind++)
    //                SegmentAlgos.Draw
    //                (
    //                    startPos: Shape.BottomLeftCorner + points[ind],
    //                    endPos: Shape.BottomLeftCorner + points[ind + 1],
    //                    width: lineWidth,
    //                    color: lineColor
    //                );
    //    }
    //}
}
