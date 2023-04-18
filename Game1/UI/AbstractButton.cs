namespace Game1.UI
{
    [Serializable]
    public class AbstractButton
    {
        public bool Clicked { get; private set; }
        public bool HalfClicked { get; private set; }

        private bool prevDown;

        public AbstractButton()
            => prevDown = false;

        public void Update(bool down)
        {
            Clicked = !down && prevDown;
            HalfClicked = down && !prevDown;
            prevDown = down;
        }
    }
}
