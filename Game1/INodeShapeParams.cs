namespace Game1
{
    public interface INodeShapeParams
    {
        public NodeID NodeID { get; }
        public AreaInt Area { get; }
        public UDouble Radius { get; }
        public UDouble SurfaceLength { get; }
        public MyVector2 Position { get; }
        public UDouble SurfaceGravity { get; }
        public Temperature Temperature { get; }
    }
}
