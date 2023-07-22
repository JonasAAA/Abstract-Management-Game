using Game1.Shapes;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public sealed class UIStepsPanel : HUDElement
    {
        protected override Color Color
            => colorConfig.UIBackgroundColor;

        private readonly List<IHUDElement> stepPanels;

        private IHUDElement CurStepPanel
            => stepPanels[^1];

        public UIStepsPanel(IHUDElement firstStepPanel)
            : base(shape: new MyRectangle())
        {
            stepPanels = new() { firstStepPanel };
            AddChild(child: firstStepPanel);
        }

        protected override void PartOfRecalcSizeAndPos()
        {
            base.PartOfRecalcSizeAndPos();
            UDouble width = stepPanels.Max(stepPanel => stepPanel.Shape.Width),
                height = stepPanels.Max(stepPanel => stepPanel.Shape.Height);
            CurStepPanel.Shape.MinWidth = width;
            CurStepPanel.Shape.MinHeight = height;
        }

        public void TransitionToNewStep(IHUDElement newStepPanel)
        {
            stepPanels.Add(newStepPanel);
            AddChild(child: newStepPanel);
        }

        public void MoveBackIfCan()
        {
            if (stepPanels.Count > 1)
            {
                RemoveChild(child: stepPanels[^1]);
                stepPanels.RemoveAt(stepPanels.Count - 1);
            }
        }

        protected override void DrawChildren()
        {
            CurStepPanel.Draw();
        }
    }
}
