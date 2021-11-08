using Game1.Events;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.UI
{
    public class ResDestinArrow : WorldUIElement
    {
        //[DataMember]
        //public Event<IDeletedListener> Deleted { get; private init; }

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

        public Vector2 EndPos
            => shape.endPos;

        [DataMember]
        public Event<INumberChangedListener> ImportanceNumberChanged
            => importanceIncDecrPanel.numberChanged;

        private new readonly Arrow shape;
        private int totalImportance;
        private readonly Color defaultActiveColor, defaultInactiveColor;
        private readonly UIRectPanel<IHUDElement/*<NearRectangle>*/> popup;
        private readonly NumIncDecrPanel importanceIncDecrPanel;
        private readonly TextBox line2;

        public ResDestinArrow(Arrow shape, Color defaultActiveColor, Color defaultInactiveColor, HorizPos popupHorizPos, VertPos popupVertPos, int minImportance, int importance, int resInd)
            : base(shape: shape, activeColor: defaultActiveColor, inactiveColor: defaultInactiveColor, popupHorizPos: popupHorizPos, popupVertPos: popupVertPos)
        {
            //Deleted = new();
            this.shape = shape;
            this.defaultActiveColor = defaultActiveColor;
            this.defaultInactiveColor = defaultInactiveColor;
            if (resInd is < 0 or > (int)MaxRes)
                throw new ArgumentOutOfRangeException();
            popup = new UIRectVertPanel<IHUDElement/*<NearRectangle>*/>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            SetPopup(UIElement: popup, overlay: (Overlay)resInd);

            UIRectHorizPanel<IHUDElement/*<NearRectangle>*/> line1 = new
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

            Button/*<MyRectangle>*/ deleteButton = new
            (
                shape: new MyRectangle
                (
                    width: 70,
                    height: 30
                )
                {
                    Color = Color.Red
                },
                action: () =>
                {
                    OnMouseDownWorldNotMe();
                    Delete();
                    //Deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
                    //Delete?.Invoke();
                },
                text: "delete"
            );
            popup.AddChild(deleteButton);
        }
    }
}
