namespace Game1.Shapes
{
    [Serializable]
    public abstract class WorldShape : Shape
    {
        private readonly WorldCamera worldCamera;

        protected WorldShape(WorldCamera worldCamera)
            => this.worldCamera = worldCamera;

        public sealed override bool Contains(Vector2Bare screenPos)
            => Contains(position: worldCamera.ScreenPosToWorldPos(screenPos));

        public abstract bool Contains(MyVector2 position);
    }
}
