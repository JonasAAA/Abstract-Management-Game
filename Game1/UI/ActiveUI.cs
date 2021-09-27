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

        public static bool ArrowDrawingModeOn
        {
            get => arrowDrawingModeOn;
            set
            {
                if (arrowDrawingModeOn == value)
                    return;

                arrowDrawingModeOn = value;
                if (arrowDrawingModeOn)
                {
                    if (Graph.Overlay > C.MaxRes)
                        throw new Exception();
                    foreach (var UIElement in activeUIElements)
                        UIElement.HasDisabledAncestor = true;
                    if (activeWorldElement is Node activeNode)
                    {
                        foreach (var node in Graph.World.Nodes)
                            if (activeNode.CanHaveDestin(destination: node.Position))
                                node.HasDisabledAncestor = false;
                    }
                    else
                        throw new Exception();
                }
                else
                {
                    foreach (var UIElement in activeUIElements)
                        UIElement.HasDisabledAncestor = false;
                }
            }
        }

        private static bool arrowDrawingModeOn;
        private static readonly List<IUIElement> activeUIElements;
        private static readonly HashSet<IUIElement> worldUIElements, HUDUIElements;
        private static bool leftDown, prevLeftDown;
        private static IUIElement halfClicked, contMouse, activeWorldElement;

        static ActiveUI()
        {
            arrowDrawingModeOn = false;
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

        public static void AddWorldElement(IUIElement UIElement)
        {
            if (UIElement is null)
                return;
            activeUIElements.Add(UIElement);
            if (HUDUIElements.Count is not 0)
                throw new ArgumentException();
            if (!worldUIElements.Add(UIElement))
                throw new ArgumentException();
        }

        public static void AddHUDElement(IUIElement<NearRectangle> UIElement, HorizPos horizPos, VertPos vertPos)
        {
            if (UIElement is null)
                return;
            Vector2 HUDCenter = new((float)(C.ScreenWidth * .5), (float)(C.ScreenHeight * .5));
            void SetUIElementPosition()
                => UIElement.Shape.SetPosition
                (
                    position: HUDCenter + new Vector2((int)horizPos * HUDCenter.X, (int)vertPos * HUDCenter.Y),
                    horizOrigin: horizPos,
                    vertOrigin: vertPos
                );

            SetUIElementPosition();
            UIElement.Shape.SizeOrPosChanged += SetUIElementPosition;

            activeUIElements.Add(UIElement);
            if (!HUDUIElements.Add(UIElement))
                throw new ArgumentException();
        }

        public static bool RemoveUIElement(IUIElement UIElement)
        {
            if (UIElement is null)
                return true;
            worldUIElements.Remove(UIElement);
            HUDUIElements.Remove(UIElement);
            return activeUIElements.Remove(UIElement);
        }

        public static void Update()
        {
            IUIElement prevContMouse = contMouse;

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

                IUIElement catchingUIElement = UIElement.CatchUIElement(mousePos: mousePos);

                if (catchingUIElement is not null)
                {
                    contMouse = catchingUIElement;
                    MouseAboveHUD = HUDUIElements.Contains(UIElement);
                    break;
                }
            }

            if (contMouse != prevContMouse)
            {
                if (prevContMouse is not null && prevContMouse.Enabled)
                    prevContMouse.MouseOn = false;
                if (contMouse is not null && contMouse.Enabled)
                    contMouse.MouseOn = true;
            }

            if (leftDown && !prevLeftDown)
                halfClicked = contMouse;

            if (!leftDown && prevLeftDown)
            {
                IUIElement otherHalfClicked = contMouse;
                if (ArrowDrawingModeOn)
                {
                    if (halfClicked == otherHalfClicked && otherHalfClicked != activeWorldElement && otherHalfClicked.Enabled && otherHalfClicked is Node destinationNode)
                        ((Node)activeWorldElement).AddResDestin(destination: destinationNode.Position);
                    ArrowDrawingModeOn = false;
                }
                else
                {
                    if (!MouseAboveHUD && (halfClicked != activeWorldElement || otherHalfClicked != activeWorldElement))
                    {
                        activeWorldElement?.OnMouseDownWorldNotMe();
                        activeWorldElement = null;
                    }
                    if (halfClicked == otherHalfClicked)
                    {
                        if (otherHalfClicked is not null && otherHalfClicked.Enabled && otherHalfClicked.CanBeClicked)
                            otherHalfClicked.OnClick();

                        if (!MouseAboveHUD)
                            activeWorldElement = otherHalfClicked;
                    }
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
