namespace Game1.Shapes
{
    [Serializable]
    public abstract class Shape
    {
        public interface IState
        {
            public Color Color { get; }
        }

        public bool Transparent
           => state.Color.Transparent();
        //public abstract Color Color { get; }
        // TODO: delete
        //public virtual MyVector2 Center { get; set; }

        private readonly IState state;

        protected Shape(IState state)
            => this.state = state;
        // TODO: delete
            //=> Color = Color.Transparent;

        public abstract bool Contains(MyVector2 position);

        protected abstract void Draw(Color color);

        public void Draw()
        {
            if (!Transparent)
                Draw(color: state.Color);
        }

        public void Draw(Color otherColor, Propor otherColorPropor)
        {
            Color color = Color.Lerp(state.Color, otherColor, amount: (float)otherColorPropor);
            color.A = state.Color.A;
            if (!color.Transparent())
                Draw(color: color);
        }
    }
}
