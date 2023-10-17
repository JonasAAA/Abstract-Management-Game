namespace Game1
{
    public interface INodeShapeParams
    {
        public NodeID NodeID { get; }
        public AreaInt Area { get; }
        public Length Radius { get; }
        public Length SurfaceLength { get; }
        public MyVector2 Position { get; }
        public SurfaceGravity SurfaceGravity { get; }
        public Temperature Temperature { get; }
    }
}
