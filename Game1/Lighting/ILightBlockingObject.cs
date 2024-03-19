namespace Game1.Lighting
{
    public interface ILightBlockingObject
    {
        /// <summary>
        /// Used for lasers to know where exactly to shine
        /// </summary>
        public MyVector2 Center { get; }

        public AngleArc.Params BlockedAngleArcParams(MyVector2 lightPos);
    }
}
