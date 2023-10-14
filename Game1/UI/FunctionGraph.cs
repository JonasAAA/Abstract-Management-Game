using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class FunctionGraph<TX, TY> : HUDElement
        where TX : struct, IScalar<TX>
        where TY : struct, IScalar<TY>
    {
        protected sealed override Color Color { get; }

        private readonly Color lineColor;
        private readonly UDouble lineWidth;
        private readonly TX minX, maxX;
        private readonly TY minY, maxY;
        private readonly ulong numXSamples;
        private bool drawPoints;
        private readonly MyVector2[] points;

        public FunctionGraph(UDouble width, UDouble height, Color lineColor, Color backgroundColor, UDouble lineWidth, TX minX, TX maxX, TY minY, TY maxY, ulong numXSamples, Func<TX, TY>? func)
            : base(shape: new MyRectangle(width: width, height: height))
        {
            Color = backgroundColor;
            this.lineColor = lineColor;
            this.lineWidth = lineWidth;
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            this.numXSamples = numXSamples;
            points = new MyVector2[numXSamples];
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
                    points[ind] = new MyVector2
                    (
                        x: Algorithms.Interpolate(normalized: normalizedX, start: 0, stop: Shape.Width),
                        y: -Algorithms.Interpolate
                        (
                            normalized: normalizedY,
                            start: 0,
                            stop: Shape.Height
                        )
                    );
                }
            }
        }

        protected override void DrawChildren()
        {
            base.DrawChildren();
            if (drawPoints)
                for (int ind = 0; ind < points.Length - 1; ind++)
                    SegmentAlgos.Draw
                    (
                        startPos: Shape.BottomLeftCorner + points[ind],
                        endPos: Shape.BottomLeftCorner + points[ind + 1],
                        width: lineWidth,
                        color: lineColor
                    );
        }
    }

    public static class FunctionGraph
    {
        public static IEnumerable<Type> GetKnownTypes()
            => new List<Type>() { typeof(FunctionGraph<Temperature, Propor>), typeof(FunctionGraph<UDouble, Propor>) };
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
    //    private readonly MyVector2[] points;

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
    //        points = new MyVector2[numXSamples];
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
    //                points[ind] = new MyVector2
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
