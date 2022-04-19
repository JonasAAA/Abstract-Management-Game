using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class ResDestinArrow : WorldUIElement
    {
        public interface IParams : VectorShape.IParams
        {
            public NodeId SourceId { get; }
            public NodeId DestinationId { get; }
            public Color defaultActiveColor { get; }
            public Color defaultInactiveColor { get; }
            public HorizPos popupHorizPos { get; }
            public VertPos popupVertPos { get; }
            public int minImportance { get; }
            public int importance { get; }
            public ResInd resInd { get; }
        }

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
                InactiveColor = parameters.defaultInactiveColor * (float)relImportance;
                ActiveColor = parameters.defaultActiveColor * (float)relImportance;
                line2.Text = $"total importance {totalImportance}";
            }
        }

        public NodeId DestinationId
            => parameters.DestinationId;

        public Event<INumberChangedListener> ImportanceNumberChanged
            => importanceIncDecrPanel.numberChanged;

        private readonly IParams parameters;
        private new readonly Arrow shape;
        private int totalImportance;
        private readonly NumIncDecrPanel importanceIncDecrPanel;
        private readonly TextBox line2;

        public ResDestinArrow(IParams parameters)
            : base
            (
                shape: new Arrow(parameters: parameters),
                activeColor: parameters.defaultActiveColor,
                inactiveColor: parameters.defaultInactiveColor,
                popupHorizPos: parameters.popupHorizPos,
                popupVertPos: parameters.popupVertPos
            )
        {
            this.parameters = parameters;
            shape = (Arrow)base.shape;
            UIRectPanel<IHUDElement> popup = new UIRectVertPanel<IHUDElement>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            SetPopup(HUDElement: popup, overlay: parameters.resInd);

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
                minNum: parameters.minImportance,
                number: parameters.importance,
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
