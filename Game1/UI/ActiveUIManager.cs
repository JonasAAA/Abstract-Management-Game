using Game1.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.UI
{
    public static class ActiveUIManager
    {
        public static readonly UIConfig UIConfig;
        public static double ScreenWidth { get; private set; }
        public static double ScreenHeight
            => UIConfig.standardScreenHeight;
        public static bool MouseAboveHUD { get; private set; }
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
                    if (CurOverlay > MaxRes)
                        throw new Exception();
                    foreach (var UIElement in activeUIElements)
                        UIElement.HasDisabledAncestor = true;
                    if (activeWorldElement is Node activeNode)
                    {
                        foreach (var node in curGraph.Nodes)
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
        public static Vector2 HUDPos
            => HUDCamera.HUDPos(screenPos: Mouse.GetState().Position.ToVector2());

        private static bool arrowDrawingModeOn;
        private static readonly List<IUIElement> activeUIElements;
        private static readonly HashSet<IUIElement> HUDElements;
        private static bool leftDown, prevLeftDown;
        private static IUIElement halfClicked, contMouse, activeWorldElement;
        private static readonly TimeSpan minDurationToGetExplanation;
        private static TimeSpan hoverDuration;
        private static readonly TextBox explanationTextBox;
        private static HUDCamera HUDCamera;
        private static Graph curGraph;
        private static readonly Dictionary<IHUDElement<NearRectangle>, SizeOrPosChangedListener> sizeOrPosChangedListenersByHUDElement;

        static ActiveUIManager()
        {
            UIConfig = new();

            arrowDrawingModeOn = false;
            activeUIElements = new();
            HUDElements = new();
            leftDown = new();
            prevLeftDown = new();
            halfClicked = null;
            contMouse = null;
            activeWorldElement = null;
            MouseAboveHUD = true;
            minDurationToGetExplanation = TimeSpan.FromSeconds(.5);
            hoverDuration = TimeSpan.Zero;
            explanationTextBox = new();
            explanationTextBox.Shape.Color = Color.LightPink;

            sizeOrPosChangedListenersByHUDElement = new();
        }

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            HUDCamera = new(graphicsDevice: graphicsDevice);
            ScreenWidth = (double)graphicsDevice.Viewport.Width * UIConfig.standardScreenHeight / graphicsDevice.Viewport.Height;
        }

        /// <summary>
        /// call exatly once after PlayState.InitializeNew()
        /// </summary>
        public static void SetCurGraph(Graph curGraph)
        {
            ActiveUIManager.curGraph = curGraph;
            if (activeUIElements.Contains(curGraph))
                throw new InvalidOperationException();
            activeUIElements.Add(curGraph);
        }

        public static void AddHUDElement(IHUDElement<NearRectangle> HUDElement, HorizPos horizPos, VertPos vertPos)
        {
            if (HUDElement is null)
                return;
            
            //void SetUIElementPosition()
            //{
            //    Vector2 HUDCenter = new((float)(ScreenWidth * .5), (float)(ScreenHeight * .5));
            //    HUDElement.Shape.SetPosition
            //    (
            //        position: HUDCenter + new Vector2((int)horizPos * HUDCenter.X, (int)vertPos * HUDCenter.Y),
            //        horizOrigin: horizPos,
            //        vertOrigin: vertPos
            //    );
            //}

            //SetUIElementPosition();
            sizeOrPosChangedListenersByHUDElement[HUDElement] = new SizeOrPosChangedListener(HorizPos: horizPos, VertPos: vertPos);
            sizeOrPosChangedListenersByHUDElement[HUDElement].SizeOrPosChangedResponse(shape: HUDElement.Shape);
            HUDElement.SizeOrPosChanged.Add(listener: sizeOrPosChangedListenersByHUDElement[HUDElement]);
            //HUDElement.Shape.SizeOrPosChanged += SetUIElementPosition;

            activeUIElements.Add(HUDElement);
            if (!HUDElements.Add(HUDElement))
                throw new ArgumentException();
        }

        public static void RemoveHUDElement(IHUDElement<NearRectangle> HUDElement)
        {
            if (HUDElement is null)
                return;
            if (!HUDElements.Remove(HUDElement))
                throw new ArgumentException();
            activeUIElements.Remove(HUDElement);
            HUDElement.SizeOrPosChanged.Remove(listener: sizeOrPosChangedListenersByHUDElement[HUDElement]);
            sizeOrPosChangedListenersByHUDElement.Remove(HUDElement);
            //HUDElement.Shape.SizeOrPosChanged -= SetUIElementPosition;
        }

        [DataContract]
        private record SizeOrPosChangedListener([property:DataMember] HorizPos HorizPos, [property: DataMember] VertPos VertPos) : ISizeOrPosChangedListener
        {
            public void SizeOrPosChangedResponse(Shape shape)
            {
                if (shape is NearRectangle nearRectangle)
                {
                    Vector2 HUDCenter = new((float)(ScreenWidth * .5), (float)(ScreenHeight * .5));
                    nearRectangle.SetPosition
                    (
                        position: HUDCenter + new Vector2((int)HorizPos * HUDCenter.X, (int)VertPos * HUDCenter.Y),
                        horizOrigin: HorizPos,
                        vertOrigin: VertPos
                    );
                }
                else
                    throw new ArgumentException();
            }
        }

        public static void Update(TimeSpan elapsed)
        {
            IUIElement prevContMouse = contMouse;

            MouseState mouseState = Mouse.GetState();
            prevLeftDown = leftDown;
            leftDown = mouseState.LeftButton == ButtonState.Pressed;
            Vector2 mouseScreenPos = mouseState.Position.ToVector2(),
                mouseWorldPos = MouseWorldPos,
                mouseHUDPos = HUDCamera.HUDPos(screenPos: mouseScreenPos);

            contMouse = null;
            MouseAboveHUD = false;
            foreach (var UIElement in Enumerable.Reverse(activeUIElements))
            {
                Vector2 mousePos = (UIElement == curGraph) switch
                {
                    true => mouseWorldPos,
                    false => mouseHUDPos
                };

                IUIElement catchingUIElement = UIElement.CatchUIElement(mousePos: mousePos);

                if (catchingUIElement is not null)
                {
                    contMouse = catchingUIElement;
                    MouseAboveHUD = HUDElements.Contains(UIElement);
                    break;
                }
            }

            if (contMouse == prevContMouse)
            {
                hoverDuration += elapsed;
                if (contMouse is not null && contMouse.Enabled && hoverDuration >= minDurationToGetExplanation && explanationTextBox.Text is null)
                {
                    explanationTextBox.Text = contMouse.Explanation;
                    explanationTextBox.Shape.TopLeftCorner = mouseHUDPos;
                    explanationTextBox.Shape.ClampPosition
                    (
                        left: 0,
                        right: (float)ScreenWidth,
                        top: 0,
                        bottom: (float)ScreenHeight
                    );
                }
            }
            else
            {
                hoverDuration = TimeSpan.Zero;
                explanationTextBox.Text = null;
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
                    if (otherHalfClicked is not null && halfClicked == otherHalfClicked && otherHalfClicked != activeWorldElement && otherHalfClicked.Enabled && otherHalfClicked is Node destinationNode)
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

        public static void DrawHUD()
        {
            HUDCamera.BeginDraw();
            foreach (var UIElement in HUDElements)
                UIElement.Draw();
            explanationTextBox.Draw();
            HUDCamera.EndDraw();
        }
    }
}
