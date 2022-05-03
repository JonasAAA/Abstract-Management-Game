using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class SelectButton : OnOffButton
    {
        public new interface IParams : OnOffButton.IParams
        { }

        public override bool CanBeClicked
            => !On;

        public SelectButton(NearRectangle.Factory shapeFactory, IParams parameters, bool on)
            : base(shapeFactory: shapeFactory, parameters: parameters, on: on)
        { }

        public override void OnClick()
            => On = true;
    }
}
