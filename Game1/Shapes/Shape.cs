namespace Game1.Shapes
{
    [Serializable]
    public abstract class Shape
    {
        public bool Transparent
           => Color.Transparent();
        public Color Color { get; set; }
        public virtual MyVector2 Center { get; set; }

        protected Shape()
            => Color = Color.Transparent;

        public abstract bool Contains(MyVector2 position);

        protected abstract void Draw(Color color);

        public void Draw()
        {
            if (!Color.Transparent())
                Draw(color: Color);
        }

        public void Draw(Color otherColor, Propor otherColorPropor)
        {
            Color color = Color.Lerp(Color, otherColor, amount: (float)otherColorPropor);
            color.A = Color.A;
            if (!color.Transparent())
                Draw(color: color);
        }
    }
}
