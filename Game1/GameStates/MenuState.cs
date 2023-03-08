using Game1.Shapes;
using Game1.UI;

namespace Game1.GameStates
{
    [Serializable]
    public sealed class MenuState : GameState
    {
        private readonly ActiveUIManager activeUIManager;
        private readonly UIRectVertPanel<ActionButton> UIPanel;

        public MenuState(List<ActionButton> actionButtons)
        {
            UIPanel = new(childHorizPos: HorizPos.Middle);
            foreach (var actionButton in actionButtons)
                UIPanel.AddChild(child: actionButton);

            activeUIManager = new(worldCamera: null);
            activeUIManager.AddHUDElement(HUDElement: UIPanel, horizPos: HorizPos.Middle, vertPos: VertPos.Middle);
        }

        public override void Update(TimeSpan elapsed)
            => activeUIManager.Update(elapsed: elapsed);

        public override void Draw()
        {
            C.GraphicsDevice.Clear(color: Color.Black);
            activeUIManager.DrawHUD();
        }
    }
}
