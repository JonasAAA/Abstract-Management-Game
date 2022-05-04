using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public abstract class OnOffButton : Button
    {
        public new interface IParams : Button.IParams
        {
            public Color SelectedColor { get; }

            public Color DeselectedColor { get; }
        }

        [Serializable]
        private record ShapeParams : LateInitializer<OnOffButton>, NearRectangle.IState
        {
            public Color Color
                => Param.On switch
                {
                    true => Param.parameters.SelectedColor,
                    false => Param.parameters.DeselectedColor
                };
        }

        public readonly Event<IOnChangedListener> onChanged;

        // TODO: could add get-set property On to IParams instead of having this
        public bool On
        {
            get => on;
            set
            {
                if (on != value)
                {
                    on = value;
                    // TODO: delete
                    //Shape.Color = on switch
                    //{
                    //    true => selectedColor,
                    //    false => deselectedColor
                    //};
                    onChanged.Raise(action: listener => listener.OnChangedResponse());
                }
            }
        }

        private bool on;
        private readonly IParams parameters;

        protected OnOffButton(NearRectangle.Factory shapeFactory, IParams parameters, bool on)
            : base(shape: shapeFactory.CreateNearRectangle(parameters: new ShapeParams()), parameters: parameters)
        {
            onChanged = new();
            this.on = on;
            this.parameters = parameters;
            ShapeParams.InitializeLast(param: this);
            // TODO: delete
            //Shape.Color = on switch
            //{
            //    true => selectedColor,
            //    false => deselectedColor
            //};
        }
    }
}
