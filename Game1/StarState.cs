namespace Game1
{
    [Serializable]
    public class StarState
    {
        // TODO: make prodWatts and color the consequence of mass/radius/material
        public readonly StarID starID;
        public readonly MyVector2 position;
        public readonly UDouble radius;
        public readonly UDouble prodWatts;

        public StarState(StarID starID, MyVector2 position, UDouble radius, UDouble prodWatts)
        {
            this.starID = starID;
            this.position = position;
            this.radius = radius;
            this.prodWatts = prodWatts;
        }
    }
}
