namespace Game1.Shapes
{
    [Serializable]
    public abstract class Shape
    {
        public interface IParams
        {
            public Color Color { get; }
        }

        public bool Transparent
           => parameters.Color.Transparent();
        //public abstract Color Color { get; }
        // TODO: delete
        //public virtual MyVector2 Center { get; set; }

        private readonly IParams parameters;

        protected Shape(IParams parameters)
            => this.parameters = parameters;
        // TODO: delete
            //=> Color = Color.Transparent;

        public abstract bool Contains(MyVector2 position);

        protected abstract void Draw(Color color);

        public void Draw()
        {
            if (!Transparent)
                Draw(color: parameters.Color);
        }

        public void Draw(Color otherColor, Propor otherColorPropor)
        {
            Color color = Color.Lerp(parameters.Color, otherColor, amount: (float)otherColorPropor);
            color.A = parameters.Color.A;
            if (!color.Transparent())
                Draw(color: color);
        }
    }
}
