namespace Game1.Shapes
{
    [Serializable]
    public class Disk : Shape
    {
        public interface IParams
        {
            public MyVector2 Center { get; }

            public UDouble Radius { get; }
        }

        public UDouble Radius
            => parameters.Radius;

        public MyVector2 Center
            => parameters.Center;

        protected readonly IParams parameters;

        public Disk(IParams parameters)
            => this.parameters = parameters;

        public sealed override bool Contains(MyVector2 position)
            => DiskAlgos.Contains(center: Center, radius: Radius, otherPos: position);

        public sealed override void Draw(Color color)
            => DiskAlgos.Draw(center: Center, radius: Radius, color: color);
    }
}
