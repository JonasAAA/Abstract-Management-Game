using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public static class ActiveUI
    {
        private static List<UIElement> activeUIElements;
        private static bool leftDown, prevLeftDown;
        private static UIElement halfClicked;

        static ActiveUI()
        {
            activeUIElements = new();
            leftDown = new();
            prevLeftDown = new();
            halfClicked = null;
        }

        public static void Add(UIElement UIElement)
            => activeUIElements.Add(UIElement);

        public static void Update()
        {
            MouseState mouseState = Mouse.GetState();
            prevLeftDown = leftDown;
            leftDown = mouseState.LeftButton == ButtonState.Pressed;
            Vector2 mouseScreenPos = mouseState.Position.ToVector2();

            if (leftDown && !prevLeftDown)
            {
                foreach (var UIElement in Enumerable.Reverse(activeUIElements))
                    if (UIElement.Contains(mousePos: mouseScreenPos))
                    {
                        halfClicked = UIElement.CatchUIElement(mousePos: mouseScreenPos);
                        break;
                    }
            }

            if (!leftDown && prevLeftDown)
            {
                UIElement otherHalfClicked = null;
                foreach (var UIElement in Enumerable.Reverse(activeUIElements))
                    if (UIElement.Contains(mousePos: mouseScreenPos))
                    {
                        otherHalfClicked = UIElement.CatchUIElement(mousePos: mouseScreenPos);
                        break;
                    }
                if (halfClicked == otherHalfClicked)
                    otherHalfClicked.OnClick();
            }
        }

        public static void DrawHUD()
            => activeUIElements.ForEach(UIElement => UIElement.Draw());
    }
}
