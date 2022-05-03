namespace Game1.Shapes
{
    // TODO: delete if unused
    public interface IShape
    {
        public bool Transparent { get; }

        public Color Color { get; }

        public bool Contains(MyVector2 position);

        public void Draw();

        public void Draw(Color otherColor, Propor otherColorPropor);
    }
}
