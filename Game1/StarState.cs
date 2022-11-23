namespace Game1
{
    [Serializable]
    public sealed class StarState
    {
        // TODO: make prodWatts and color the consequence of Mass/radius/material
        public readonly StarID starID;
        public readonly MyVector2 position;
        public readonly UDouble radius;
        public readonly UDouble prodWatts;

        public StarState(MyVector2 position, UDouble radius, UDouble prodWatts)
        {
            starID = StarID.Create();
            this.position = position;
            this.radius = radius;
            this.prodWatts = prodWatts;
        }
    }
}
