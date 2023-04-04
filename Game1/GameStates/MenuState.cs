using Game1.Shapes;
using Game1.UI;

namespace Game1.GameStates
{
    [Serializable]
    public sealed class MenuState : GameState
    {
        private readonly ActiveUIManager activeUIManager;
        private UIRectVertPanel<ActionButton>? UIPanel;
        private Func<List<ActionButton>>? getActionButtons;

        public MenuState()
        {
            activeUIManager = new(worldCamera: null);
            UIPanel = null;
            getActionButtons = null;
        }

        public void Initialize(Func<List<ActionButton>> getActionButtons)
        {
            if (this.getActionButtons is not null)
                throw new InvalidOperationException();
            this.getActionButtons = getActionButtons;
        }

        public override void OnEnter()
        {
            if (getActionButtons is null)
                throw new ArgumentException();

            base.OnEnter();

            activeUIManager.RemoveHUDElement(HUDElement: UIPanel);
            UIPanel = new(childHorizPos: HorizPos.Middle);
            foreach (var actionButton in getActionButtons())
                UIPanel.AddChild(child: actionButton);
            activeUIManager.AddHUDElement(HUDElement: UIPanel, horizPos: HorizPos.Middle, vertPos: VertPos.Middle);
        }

        public override void Update(TimeSpan elapsed)
            => activeUIManager.Update(elapsed: elapsed);

        public override void Draw()
        {
            if (getActionButtons is null)
                throw new InvalidOperationException();
            C.GraphicsDevice.Clear(color: Color.Black);
            activeUIManager.DrawHUD();
        }
    }
}
