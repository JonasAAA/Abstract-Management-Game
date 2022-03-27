namespace Game1.Lighting
{
    public interface ILightCatchingObject : IDeletable
    {
        public IEnumerable<float> RelAngles(Vector2 lightPos);

        public IEnumerable<float> InterPoints(Vector2 lightPos, Vector2 lightDir);

        public void SetWatts(Vector2 starPos, double watts, double powerPropor);
    }
}
