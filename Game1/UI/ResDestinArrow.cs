using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public class ResDestinArrow : WorldUIElement
    {
        public int Importance
            => importanceIncDecrPanel.Number;

        public int TotalImportance
        {
            set
            {
                totalImportance = value;
                float relImportance = (float)((double)Importance / totalImportance);
                InactiveColor = defaultInactiveColor * relImportance;
                ActiveColor = defaultActiveColor * relImportance;
                line2.Text = $"total importance {totalImportance}";
            }
        }

        public event Action ImportanceChanged
        {
            add => importanceIncDecrPanel.NumberChanged += value;
            remove => importanceIncDecrPanel.NumberChanged -= value;
        }

        private int totalImportance;
        private readonly Color defaultActiveColor, defaultInactiveColor;
        private readonly UIRectPanel<IUIElement<NearRectangle>> popup;
        private readonly NumIncDecrPanel importanceIncDecrPanel;
        private readonly TextBox line2;

        public ResDestinArrow(Arrow shape, bool active, Color defaultActiveColor, Color defaultInactiveColor, HorizPos popupHorizPos, VertPos popupVertPos, int minImportance, int importance, float letterHeight, int resInd)
            : base(shape: shape, active: active, activeColor: defaultActiveColor, inactiveColor: defaultInactiveColor, popupHorizPos: popupHorizPos, popupVertPos: popupVertPos)
        {
            this.defaultActiveColor = defaultActiveColor;
            this.defaultInactiveColor = defaultInactiveColor;
            if (resInd is < 0 or > (int)C.MaxRes)
                throw new ArgumentOutOfRangeException();
            popup = new UIRectVertPanel<IUIElement<NearRectangle>>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            UIRectHorizPanel<IUIElement<NearRectangle>> line1 = new
            (
                color: Color.White,
                childVertPos: VertPos.Middle
            );
            popup.AddChild(child: line1);
            line1.AddChild
            (
                child: new TextBox(letterHeight: letterHeight)
                {
                    Text = "importance "
                }
            );
            importanceIncDecrPanel = new
            (
                minNum: minImportance,
                number: importance,
                letterHeight: letterHeight,
                incrDecrButtonHeight: 20,
                shapeColor: Color.White,
                incrDecrButtonColor: Color.Blue
            );
            line1.AddChild(child: importanceIncDecrPanel);

            line2 = new(letterHeight: letterHeight);
            popup.AddChild(child: line2);

            SetPopup(UIElement: popup, overlay: (Overlay)resInd);
        }
    }
}
