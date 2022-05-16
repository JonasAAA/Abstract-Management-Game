namespace Game1.Lighting
{
    public interface ILightBlockingObject
    {
        public IEnumerable<double> RelAngles(MyVector2 lightPos);

        public IEnumerable<double> InterPoints(MyVector2 lightPos, MyVector2 lightDir);
    }
}
