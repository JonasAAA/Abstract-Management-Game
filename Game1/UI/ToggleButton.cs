using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class ToggleButton : OnOffButton
    {
        public new interface IParams : OnOffButton.IParams
        { }

        public ToggleButton(NearRectangle.Factory shapeFactory, IParams parameters, bool on)
            : base(shapeFactory: shapeFactory, parameters: parameters, on: on)
        { }

        public override void OnClick()
            => On = !On;
    }
}
