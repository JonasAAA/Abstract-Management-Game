namespace Game1.UI
{
    public class ResDestinArrow : UIElement<Arrow>
    {
        public override bool CanBeClicked
            => true;

        public ResDestinArrow(Arrow shape)
            : base(shape: shape)
        { }
    }
}
