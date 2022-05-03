using static Game1.WorldManager;

namespace Game1.Shapes
{
    public abstract class WorldShape : Shape
    {
        public new interface IParams : Shape.IParams
        {
            public Color ActiveColor
                => CurWorldConfig.defaultActiveColor;
            public Color InactiveColor
                => CurWorldConfig.defaultInactiveColor;
            public bool Active { get; }

            Color Shape.IParams.Color
                => Active switch
                {
                    true => ActiveColor,
                    false => InactiveColor
                };
        }

        // TODO: delete
        //public sealed override Color Color
        //    => parameters.Active switch
        //    {
        //        true => parameters.ActiveColor,
        //        false => parameters.InactiveColor
        //    };

        protected WorldShape(IParams parameters)
            : base(parameters: parameters)
        { }
    }
}
