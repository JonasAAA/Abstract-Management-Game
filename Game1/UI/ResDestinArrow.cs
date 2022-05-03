using Game1.Delegates;
using Game1.Shapes;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;

namespace Game1.UI
{
    [Serializable]
    public class ResDestinArrow : WorldUIElement
    {
        public interface IParamsAndState : NumIncDecrPanel.IParamsAndState, UIRectVertPanel<IHUDElement>.IParams, UIRectHorizPanel<IHUDElement>.IParams
        {
            public UDouble Width { get; }
            public Color DefaultActiveColor { get; }
            public Color DefaultInactiveColor { get; }
            //public HorizPos PopupHorizPos { get; }
            //public VertPos PopupVertPos { get; }
            public ulong MinImportance { get; }
            // TODO: move the color to contants class
            public Color DeleteButtonColor
                => Color.Red;
            public Color DeleteButtonTextColor
                => curUIConfig.defaultTextColor;
            //public ResInd ResInd { get; }
            public ulong TotalImportance { get; }

            public ulong Importance { get; set; }

            int NumIncDecrPanel.IParamsAndState.MinNumber
                => (int)MinImportance;
            int NumIncDecrPanel.IParamsAndState.MaxNumber
                => int.MaxValue;
            int NumIncDecrPanel.IParamsAndState.Number
            {
                get => (int)Importance;
                set => Importance = (ulong)value;
            }
        }

        [Serializable]
        private record ArrowParams : LateInitializer<ResDestinArrow>, Arrow.IParams
        {
            public MyVector2 StartPos
                => CurWorldManager.NodePosition(nodeId: Param.sourceId);

            public MyVector2 EndPos
                => CurWorldManager.NodePosition(nodeId: Param.destinationId);

            public UDouble Width
                => ParamsAndState.Width;

            public Color ActiveColor
                => ImportancePropor * ParamsAndState.DefaultActiveColor;

            public Color InactiveColor
                => ImportancePropor * ParamsAndState.DefaultInactiveColor;

            public bool Active
                => Param.Active;

            private Propor ImportancePropor
                => Propor.Create(part: ParamsAndState.Importance, whole: ParamsAndState.TotalImportance) switch
                {
                    Propor propor => propor,
                    null => throw new Exception("part must not be bigger than whole")
                };

            private IParamsAndState ParamsAndState
                => Param.paramsAndState;

            // TODO: delere
            // This last part of class is an ugly hack to make ArrowParams see current ResDestinArrow instance
            // The straight-forward way doesn't work as you can't use "this" during the call to the base constructor 
            // The class will throw an exception if not properly initialized before use
            //private const string mustInitializeMessage = $"Must initialize {nameof(ArrowParams)} first by calling {nameof(Initialize)}";

            //private ResDestinArrow ResDestinArrow
            //    => resDestinArrow ?? throw new InvalidOperationException(mustInitializeMessage);

            //public static ArrowParams? lastArrowParams;

            //public ArrowParams()
            //{
            //    if (lastArrowParams is not null && lastArrowParams.resDestinArrow is null)
            //        throw new InvalidOperationException(mustInitializeMessage);
            //    lastArrowParams = this;
            //}

            //private ResDestinArrow? resDestinArrow;

            //public void Initialize(ResDestinArrow resDestinArrow)
            //    => this.resDestinArrow = resDestinArrow;
        }

        [Serializable]
        private readonly record struct Line1Params(IParamsAndState ParamsAndState) : TextBox.IParams
        {
            public Color BackgroundColor
                => ParamsAndState.BackgroundColor;

            public Color TextColor
                => ParamsAndState.TextColor;

            public string? Text
                => "importance ";
        }

        [Serializable]
        private readonly record struct Line2Params(IParamsAndState ParamsAndState) : TextBox.IParams
        {
            public Color BackgroundColor
                => ParamsAndState.BackgroundColor;

            public Color TextColor
                => ParamsAndState.TextColor;

            public string? Text
                => $"total importance {ParamsAndState.TotalImportance}";
        }

        [Serializable]
        private readonly record struct DeleteButtonParams(IParamsAndState ParamsAndState) : Button.IParams, MyRectangle.IParams
        {
            public string? Text
                => "delete";

            public string? Explanation
                => null;

            public Color Color
                => ParamsAndState.DeleteButtonColor;

            public Color BackgroundColor
                => ParamsAndState.DeleteButtonColor;
        }

        // TODO: delete
        //private readonly record struct NumIncrDecrPanelParamsAndState() : NumIncDecrPanel.IParamsAndState
        //{
        //    public int MinNumber => throw new NotImplementedException();

        //    public int MaxNumber => throw new NotImplementedException();

        //    public Color IncrAndDecrButtonColor => throw new NotImplementedException();

        //    public int Number { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //    public Color BackgroundColor => throw new NotImplementedException();

        //    public Color TextColor => throw new NotImplementedException();
        //}

        [Serializable]
        private readonly record struct DeleteButtonClickedListener(ResDestinArrow ResDestinArrow) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                ResDestinArrow.Active = false;
                ResDestinArrow.Delete();
            }
        }

        // TODO: delete
        //public ulong Importance
        //    => (ulong)importanceIncDecrPanel.Number;

        //public int TotalImportance
        //{
        //    set
        //    {
        //        totalImportance = value;
        //        double relImportance = (double)Importance / totalImportance;
        //        InactiveColor = parameters.DefaultInactiveColor * (float)relImportance;
        //        ActiveColor = parameters.DefaultActiveColor * (float)relImportance;
        //        line2.Text = $"total importance {totalImportance}";
        //    }
        //}

        public readonly NodeId sourceId, destinationId;

        //public Event<INumberChangedListener> ImportanceNumberChanged
        //    => importanceIncDecrPanel.numberChanged;

        private readonly IParamsAndState paramsAndState;
        //private int totalImportance;
        private readonly NumIncDecrPanel importanceIncDecrPanel;
        private readonly TextBox line2;

        public ResDestinArrow(IParamsAndState paramsAndState, NodeId sourceId, NodeId destinationId, ResInd resInd, HorizPos popupHorizPos, VertPos popupVertPos)
            : base
            (
                shape: new Arrow(parameters: new ArrowParams()),
                popupHorizPos: popupHorizPos,
                popupVertPos: popupVertPos
            )
        {
            this.paramsAndState = paramsAndState;
            this.sourceId = sourceId;
            this.destinationId = destinationId;
            ArrowParams.InitializeLast(param: this);
            UIRectPanel<IHUDElement> popup = new UIRectVertPanel<IHUDElement>
            (
                parameters: paramsAndState,
                childHorizPos: HorizPos.Left
            );
            SetPopup(HUDElement: popup, overlay: resInd);

            UIRectHorizPanel<IHUDElement> line1 = new
            (
                parameters: paramsAndState,
                childVertPos: VertPos.Middle
            );
            popup.AddChild(child: line1);
            line1.AddChild
            (
                child: new TextBox(parameters: new Line1Params())
            );
            importanceIncDecrPanel = new
            (
                // TODO: put this into constants class
                incrDecrButtonHeight: 20,
                paramsAndState: paramsAndState
                // TODO: delete
                //minNum: paramsAndState.MinImportance,
                //number: paramsAndState.Importance,
                
                //shapeColor: Color.White,
                //incrDecrButtonColor: Color.Blue
            );
            line1.AddChild(child: importanceIncDecrPanel);

            line2 = new(parameters: new Line2Params());
            popup.AddChild(child: line2);

            DeleteButtonParams deleteButtonParams = new(ParamsAndState: paramsAndState);
            Button deleteButton = new
            (
                // TODO: move constants to constants class
                shape: new MyRectangle
                (
                    width: 70,
                    height: 30,
                    parameters: deleteButtonParams
                ),
                parameters: deleteButtonParams
            );
            deleteButton.clicked.Add(listener: new DeleteButtonClickedListener(ResDestinArrow: this));
            popup.AddChild(deleteButton);
        }

        protected override void Delete()
        {
            paramsAndState.Importance = 0;
            base.Delete();
        }
    }
}
