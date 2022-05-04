using static Game1.WorldManager;

namespace Game1.Shapes
{
    public abstract class WorldShape : Shape
    {
        public new interface IParams : Shape.IState
        {
            public Color ActiveColor
                => CurWorldConfig.defaultActiveColor;
            public Color InactiveColor
                => CurWorldConfig.defaultInactiveColor;
            public bool Active { get; }

            Color Shape.IState.Color
                => Active switch
                {
                    true => ActiveColor,
                    false => InactiveColor
                };
        }

        protected WorldShape(IParams parameters)
            : base(state: parameters)
        { }
    }
}
