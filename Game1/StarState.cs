namespace Game1
{
    [Serializable]
    public class StarState
    {
        // TODO: make prodWatts and color the consequence of mass/radius/material
        public readonly StarId starId;
        public readonly MyVector2 position;
        public readonly UDouble radius;
        public readonly UDouble prodWatts;

        public StarState(StarId starId, MyVector2 position, UDouble radius, UDouble prodWatts)
        {
            this.starId = starId;
            this.position = position;
            this.radius = radius;
            this.prodWatts = prodWatts;
        }
    }
}
