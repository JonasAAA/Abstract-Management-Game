using Game1.Shapes;
using Game1.UI;

namespace Game1.GameStates
{
    [Serializable]
    public sealed class MenuState : GameState
    {
        private readonly ActiveUIManager activeUIManager;
        private UIRectVertPanel<IHUDElement>? UIPanel;
        private Func<IEnumerable<IHUDElement>>? getHUDElements;

        public MenuState()
        {
            activeUIManager = new(worldCamera: null);
            UIPanel = null;
            getHUDElements = null;
        }

        public void Initialize(Func<IEnumerable<IHUDElement>> getHUDElements)
        {
            if (this.getHUDElements is not null)
                throw new InvalidOperationException();
            this.getHUDElements = getHUDElements;
        }

        public sealed override void OnEnter()
        {
            if (getHUDElements is null)
                throw new ArgumentException();

            base.OnEnter();

            activeUIManager.RemoveHUDElement(HUDElement: UIPanel);
            UIPanel = new
            (
                childHorizPos: HorizPosEnum.Middle,
                children: getHUDElements()
            );
            activeUIManager.AddHUDElement(HUDElement: UIPanel, position: new(HorizPosEnum.Middle, VertPosEnum.Middle));
        }

        public sealed override void Update(TimeSpan elapsed)
            => activeUIManager.Update(elapsed: elapsed);

        public sealed override void Draw()
        {
            if (getHUDElements is null)
                throw new InvalidOperationException();
            C.GraphicsDevice.Clear(color: Color.Black);
            activeUIManager.DrawHUD();
        }
    }
}
