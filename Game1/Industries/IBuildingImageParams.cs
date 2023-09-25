namespace Game1.Industries
{
    public interface IBuildingImageParams<T>
        where T : IBuildingImage, IIncompleteBuildingImage
    {
        public T CreateImage(INodeShapeParams nodeShapeParams);
    }
}
