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
            public Color DefaultActiveColor { get; }
            public Color DefaultInactiveColor { get; }
            public HorizPos PopupHorizPos { get; }
            public VertPos PopupVertPos { get; }
            public int MinImportance { get; }
            public int Importance { get; }
            public ResInd ResInd { get; }
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
                InactiveColor = parameters.DefaultInactiveColor * (float)relImportance;
                ActiveColor = parameters.DefaultActiveColor * (float)relImportance;
                line2.Text = $"total importance {totalImportance}";
            }
        }

        public NodeId DestinationId
            => parameters.DestinationId;

        public Event<INumberChangedListener> ImportanceNumberChanged
            => importanceIncDecrPanel.numberChanged;

        private readonly IParams parameters;
        private int totalImportance;
        private readonly NumIncDecrPanel importanceIncDecrPanel;
        private readonly TextBox line2;

        public ResDestinArrow(IParams parameters)
            : base
            (
                shape: new Arrow(parameters: parameters),
                activeColor: parameters.DefaultActiveColor,
                inactiveColor: parameters.DefaultInactiveColor,
                popupHorizPos: parameters.PopupHorizPos,
                popupVertPos: parameters.PopupVertPos
            )
        {
            this.parameters = parameters;
            UIRectPanel<IHUDElement> popup = new UIRectVertPanel<IHUDElement>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            SetPopup(HUDElement: popup, overlay: parameters.ResInd);

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
                minNum: parameters.MinImportance,
                number: parameters.Importance,
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
