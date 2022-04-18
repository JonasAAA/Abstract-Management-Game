using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class ResDestinArrow : WorldUIElement
    {
        [Serializable]
        private readonly record struct DeleteButtonClickedListener(ResDestinArrow ResDestinArrow) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                ResDestinArrow.Active = false;
                ResDestinArrow.Delete();
            }
        }

        public int Importance
            => importanceIncDecrPanel.Number;

        public int TotalImportance
        {
            set
            {
                totalImportance = value;
                double relImportance = (double)((double)Importance / totalImportance);
                InactiveColor = defaultInactiveColor * (float)relImportance;
                ActiveColor = defaultActiveColor * (float)relImportance;
                line2.Text = $"total importance {totalImportance}";
            }
        }

        public MyVector2 EndPos
            => shape.endPos;

        public Event<INumberChangedListener> ImportanceNumberChanged
            => importanceIncDecrPanel.numberChanged;

        private new readonly Arrow shape;
        private int totalImportance;
        private readonly Color defaultActiveColor, defaultInactiveColor;
        private readonly NumIncDecrPanel importanceIncDecrPanel;
        private readonly TextBox line2;

        public ResDestinArrow(Arrow shape, Color defaultActiveColor, Color defaultInactiveColor, HorizPos popupHorizPos, VertPos popupVertPos, int minImportance, int importance, ResInd resInd)
            : base(shape: shape, activeColor: defaultActiveColor, inactiveColor: defaultInactiveColor, popupHorizPos: popupHorizPos, popupVertPos: popupVertPos)
        {
            this.shape = shape;
            this.defaultActiveColor = defaultActiveColor;
            this.defaultInactiveColor = defaultInactiveColor;
            UIRectPanel<IHUDElement> popup = new UIRectVertPanel<IHUDElement>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            SetPopup(HUDElement: popup, overlay: (IOverlay)resInd);

            UIRectHorizPanel<IHUDElement> line1 = new
            (
                color: Color.White,
                childVertPos: VertPos.Middle
            );
            popup.AddChild(child: line1);
            line1.AddChild
            (
                child: new TextBox()
                {
                    Text = "importance "
                }
            );
            importanceIncDecrPanel = new
            (
                minNum: minImportance,
                number: importance,
                incrDecrButtonHeight: 20,
                shapeColor: Color.White,
                incrDecrButtonColor: Color.Blue
            );
            line1.AddChild(child: importanceIncDecrPanel);

            line2 = new();
            popup.AddChild(child: line2);

            Button deleteButton = new
            (
                shape: new MyRectangle
                (
                    width: 70,
                    height: 30
                )
                {
                    Color = Color.Red
                },
                text: "delete"
            );
            deleteButton.clicked.Add(listener: new DeleteButtonClickedListener(ResDestinArrow: this));
            popup.AddChild(deleteButton);
        }
    }
}
