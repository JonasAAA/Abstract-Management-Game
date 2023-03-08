using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class ResDestinArrow : WorldUIElement, IDeletable
    {
        public IEvent<IDeletedListener> Deleted
            => deleted;

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
                inactiveColor = defaultInactiveColor * (float)relImportance;
                activeColor = defaultActiveColor * (float)relImportance;
                line2.Text = $"total importance {totalImportance}";
            }
        }

        public NodeID DestinationId
            => destinId;

        public Event<INumberChangedListener> ImportanceNumberChanged
            => importanceIncDecrPanel.numberChanged;

        private readonly Event<IDeletedListener> deleted;
        private int totalImportance;
        private readonly NodeID destinId;
        private readonly Color defaultActiveColor, defaultInactiveColor;
        private readonly NumIncDecrPanel importanceIncDecrPanel;
        private readonly TextBox line2;

        public ResDestinArrow(VectorShape.IParams shapeParams, NodeID destinId, Color defaultActiveColor, Color defaultInactiveColor, HorizPos popupHorizPos, VertPos popupVertPos, int minImportance, int startImportance, ResInd resInd)
            : base
            (
                shape: new Arrow(parameters: shapeParams),
                activeColor: defaultActiveColor,
                inactiveColor: defaultInactiveColor,
                popupHorizPos: popupHorizPos,
                popupVertPos: popupVertPos
            )
        {
            this.destinId = destinId;
            this.defaultActiveColor = defaultActiveColor;
            this.defaultInactiveColor = defaultInactiveColor;

            deleted = new();
            UIRectPanel<IHUDElement> popup = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPos.Left);
            SetPopup(HUDElement: popup, overlay: resInd);

            UIRectHorizPanel<IHUDElement> line1 = new(childVertPos: VertPos.Middle);
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
                    height: 30
                ),
                text: "delete",
                tooltip: new ImmutableTextTooltip(text: $"Remove resource {resInd} destination"),
                color: Color.Red
            );
            deleteButton.clicked.Add(listener: new DeleteButtonClickedListener(ResDestinArrow: this));
            popup.AddChild(deleteButton);
        }

        protected override void Delete()
        {
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }
    }
}
