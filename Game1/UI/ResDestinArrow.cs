using Game1.Events;
using Game1.Shapes;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.UI
{
    [DataContract]
    public class ResDestinArrow : WorldUIElement
    {
        [DataContract]
        private record DeleteButtonClickedListener([property:DataMember] ResDestinArrow ResDestinArrow) : IClickedListener
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
                float relImportance = (float)((double)Importance / totalImportance);
                InactiveColor = defaultInactiveColor * relImportance;
                ActiveColor = defaultActiveColor * relImportance;
                line2.Text = $"total importance {totalImportance}";
            }
        }

        public Vector2 EndPos
            => shape.endPos;

        public Event<INumberChangedListener> ImportanceNumberChanged
            => importanceIncDecrPanel.numberChanged;

        [DataMember] private new readonly Arrow shape;
        [DataMember] private int totalImportance;
        [DataMember] private readonly Color defaultActiveColor, defaultInactiveColor;
        [DataMember] private readonly NumIncDecrPanel importanceIncDecrPanel;
        [DataMember] private readonly TextBox line2;

        public ResDestinArrow(Arrow shape, Color defaultActiveColor, Color defaultInactiveColor, HorizPos popupHorizPos, VertPos popupVertPos, int minImportance, int importance, int resInd)
            : base(shape: shape, activeColor: defaultActiveColor, inactiveColor: defaultInactiveColor, popupHorizPos: popupHorizPos, popupVertPos: popupVertPos)
        {
            this.shape = shape;
            this.defaultActiveColor = defaultActiveColor;
            this.defaultInactiveColor = defaultInactiveColor;
            if (resInd is < 0 or > (int)MaxRes)
                throw new ArgumentOutOfRangeException();
            UIRectPanel<IHUDElement> popup = new UIRectVertPanel<IHUDElement>
            (
                color: Color.White,
                childHorizPos: HorizPos.Left
            );
            SetPopup(HUDElement: popup, overlay: (Overlay)resInd);

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
