using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public static class ActiveUI
    {
        public static int Count
            => activeUIElements.Count;

        private static readonly List<UIElement> activeUIElements;
        private static readonly HashSet<UIElement> worldUIElements, HUDUIElements;
        private static bool leftDown, prevLeftDown;
        private static UIElement halfClicked, contMouse, activeWorldElement;

        static ActiveUI()
        {
            activeUIElements = new();
            worldUIElements = new();
            HUDUIElements = new();
            leftDown = new();
            prevLeftDown = new();
            halfClicked = null;
            contMouse = null;
            activeWorldElement = null;
        }

        public static void Add(UIElement UIElement, bool world)
        {
            activeUIElements.Add(UIElement);
            if (world)
            {
                if (HUDUIElements.Count is not 0)
                    throw new ArgumentException();
                if (!worldUIElements.Add(UIElement))
                    throw new ArgumentException();
            }
            else
            {
                if (!HUDUIElements.Add(UIElement))
                    throw new ArgumentException();
            }
        }

        public static bool Remove(UIElement UIElement)
        {
            worldUIElements.Remove(UIElement);
            HUDUIElements.Remove(UIElement);
            return activeUIElements.Remove(UIElement);
        }

        public static void Update()
        {
            UIElement prevContMouse = contMouse;

            MouseState mouseState = Mouse.GetState();
            prevLeftDown = leftDown;
            leftDown = mouseState.LeftButton == ButtonState.Pressed;
            Vector2 mouseScreenPos = mouseState.Position.ToVector2(),
                mouseWorldPos = C.Camera.Position(screenPos: mouseScreenPos);

            contMouse = null;
            bool? contMouseHUD = null;
            foreach (var UIElement in Enumerable.Reverse(activeUIElements))
            {
                Vector2 mousePos = worldUIElements.Contains(UIElement) switch
                {
                    true => mouseWorldPos,
                    false => mouseScreenPos
                };

                UIElement catchingUIElement = UIElement.CatchUIElement(mousePos: mousePos);

                if (catchingUIElement is not null)
                {
                    contMouse = catchingUIElement;
                    contMouseHUD = HUDUIElements.Contains(UIElement);
                    break;
                }
            }

            if (contMouse != prevContMouse)
            {
                prevContMouse?.OnMouseLeave();
                contMouse?.OnMouseEnter();
            }

            if (leftDown && !prevLeftDown)
            {
                halfClicked = contMouse;
                if (contMouseHUD.HasValue && !contMouseHUD.Value && halfClicked != activeWorldElement)
                {
                    activeWorldElement?.OnMouseDownWorldNotMe();
                    activeWorldElement = null;
                }
            }

            if (!leftDown && prevLeftDown)
            {
                UIElement otherHalfClicked = contMouse;
                if (halfClicked == otherHalfClicked)
                {
                    otherHalfClicked?.OnClick();
                    if (contMouseHUD.HasValue && !contMouseHUD.Value)
                        activeWorldElement = otherHalfClicked;
                }

                halfClicked = null;
            }
        }

        public static void Draw()
        {
            foreach (var UIElement in worldUIElements)
                UIElement.Draw();
        }

        public static void DrawHUD()
        {
            foreach (var UIElement in HUDUIElements)
                UIElement.Draw();
        }
    }
}
