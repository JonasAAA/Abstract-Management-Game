namespace Game1.Lighting
{
    public interface ILightCatchingObject : ILightBlockingObject, IDeletable
    {
        public void BeginSetWatts()
        { }

        public void SetWatts(StarID starPos, UDouble watts, Propor powerPropor);
    }
}
