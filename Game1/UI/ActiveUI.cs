using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1.UI
{
    public static class ActiveUI
    {
        public static bool MouseAboveHUD { get; private set; }
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
            MouseAboveHUD = true;
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
                mouseWorldPos = C.WorldCamera.WorldPos(screenPos: mouseScreenPos),
                mouseHUDPos = C.HUDCamera.HUDPos(screenPos: mouseScreenPos);

            contMouse = null;
            MouseAboveHUD = false;
            foreach (var UIElement in Enumerable.Reverse(activeUIElements))
            {
                Vector2 mousePos = worldUIElements.Contains(UIElement) switch
                {
                    true => mouseWorldPos,
                    false => mouseHUDPos
                };

                UIElement catchingUIElement = UIElement.CatchUIElement(mousePos: mousePos);

                if (catchingUIElement is not null)
                {
                    contMouse = catchingUIElement;
                    MouseAboveHUD = HUDUIElements.Contains(UIElement);
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
                if (!MouseAboveHUD && halfClicked != activeWorldElement)
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
                    if (!MouseAboveHUD)
                        activeWorldElement = otherHalfClicked;
                }

                halfClicked = null;
            }
        }

        public static void Draw()
        {
            C.WorldCamera.BeginDraw();
            foreach (var UIElement in worldUIElements)
                UIElement.Draw();
            C.WorldCamera.EndDraw();

            C.HUDCamera.BeginDraw();
            foreach (var UIElement in HUDUIElements)
                UIElement.Draw();
            C.HUDCamera.EndDraw();
        }
    }
}
