namespace Game1.Lighting
{
    public interface ILightCatchingObject : IDeletable
    {
        public IEnumerable<double> RelAngles(MyVector2 lightPos);

        public IEnumerable<double> InterPoints(MyVector2 lightPos, MyVector2 lightDir);

        public void SetWatts(StarID starPos, UDouble watts, Propor powerPropor);
    }
}
