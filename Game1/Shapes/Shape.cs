namespace Game1.Shapes
{
    [Serializable]
    public abstract class Shape
    {
        public bool Transparent
           => Color.Transparent();
        public Color Color { get; set; }

        protected Shape(Color color)
            => Color = color;

        public abstract bool Contains(MyVector2 position);

        protected abstract void Draw(Color color);

        public void Draw()
        {
            if (!Transparent)
                Draw(color: Color);
        }

        public void Draw(Color otherColor, Propor otherColorPropor)
        {
            Color color = Color.Lerp(Color, otherColor, amount: (float)otherColorPropor);
            if (!color.Transparent())
                Draw(color: color);
        }
    }
}
