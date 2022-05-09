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
                double relImportance = (double)Importance / totalImportance;
                InactiveColor = defaultInactiveColor * (float)relImportance;
                ActiveColor = defaultActiveColor * (float)relImportance;
                line2.Text = $"total importance {totalImportance}";
            }
        }

        public NodeID DestinationId
            => destinId;

        public Event<INumberChangedListener> ImportanceNumberChanged
            => importanceIncDecrPanel.numberChanged;

        private int totalImportance;
        private readonly NodeID destinId;
        private readonly Color defaultActiveColor, defaultInactiveColor;
        private readonly NumIncDecrPanel importanceIncDecrPanel;
        private readonly TextBox line2;

        public ResDestinArrow(VectorShape.IParams shapeParams, NodeID destinId, Color defaultActiveColor, Color defaultInactiveColor, HorizPos popupHorizPos, VertPos popupVertPos, int minImportance, int startImportance, ResInd resInd)
            : base
            (
                shape: new Arrow(parameters: shapeParams, color: Color.White),
                activeColor: defaultActiveColor,
                inactiveColor: defaultInactiveColor,
                popupHorizPos: popupHorizPos,
                popupVertPos: popupVertPos
            )
        {
            this.destinId = destinId;
            this.defaultActiveColor = defaultActiveColor;
            this.defaultInactiveColor = defaultInactiveColor;

            UIRectPanel<IHUDElement> popup = new UIRectVertPanel<IHUDElement>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            SetPopup(HUDElement: popup, overlay: resInd);

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
                number: startImportance,
                shapeColor: Color.White,
                incrDecrButtonHeight: 20,
                incrButtonTooltip: new ImmutableTextTooltip(text: "Increase importance of this route"),
                decrButtonTooltip: new ImmutableTextTooltip(text: "Decrease importance of this route"),
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
                    height: 30,
                    color: Color.Red
                ),
                text: "delete",
                tooltip: new ImmutableTextTooltip(text: $"Remove resource {resInd} destination")
            );
            deleteButton.clicked.Add(listener: new DeleteButtonClickedListener(ResDestinArrow: this));
            popup.AddChild(deleteButton);
        }
    }
}
