using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static Game1.WorldManager;

namespace Game1.UI
{
    public class ActiveUIManager
    {
        public static ActiveUIManager CurActiveUIManager { get; private set; }

        public static UIConfig CurUIConfig
            => CurActiveUIManager.UIConfig;

        public static void CreateActiveUIManager(GraphicsDevice graphicsDevice)
        {
            if (CurActiveUIManager is not null)
                throw new InvalidOperationException();
            CurActiveUIManager = new(graphicsDevice: graphicsDevice);

            CurActiveUIManager.explanationTextBox = new();
            CurActiveUIManager.explanationTextBox.Shape.Color = Color.LightPink;

            CurActiveUIManager.HUDCamera = new(graphicsDevice: graphicsDevice);
        }

        public double ScreenHeight
            => UIConfig.standardScreenHeight;

        public bool ArrowDrawingModeOn
        {
            get => arrowDrawingModeOn;
            set
            {
                if (arrowDrawingModeOn == value)
                    return;

                arrowDrawingModeOn = value;
                if (arrowDrawingModeOn)
                {
                    if (CurWorldManager.Overlay > MaxRes)
                        throw new Exception();
                    foreach (var UIElement in activeUIElements)
                        UIElement.HasDisabledAncestor = true;
                    if (activeWorldElement is Node activeNode)
                    {
                        foreach (var node in graph.Nodes)
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

        public Vector2 MouseHUDPos
            => HUDCamera.HUDPos(screenPos: Mouse.GetState().Position.ToVector2());

        public bool MouseAboveHUD { get; private set; }

        public readonly double screenWidth;
        private readonly UIConfig UIConfig;
        
        private bool arrowDrawingModeOn;
        private readonly List<IUIElement> activeUIElements;
        private readonly HashSet<IUIElement> HUDElements;
        private bool leftDown, prevLeftDown;
        private IUIElement halfClicked, contMouse, activeWorldElement;
        private readonly TimeSpan minDurationToGetExplanation;
        private TimeSpan hoverDuration;
        private TextBox explanationTextBox;
        private HUDCamera HUDCamera;
        private Graph graph;
        private readonly HUDPosSetter HUDPosSetter;

        private ActiveUIManager(GraphicsDevice graphicsDevice)
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

            screenWidth = (double)graphicsDevice.Viewport.Width * UIConfig.standardScreenHeight / graphicsDevice.Viewport.Height;
            HUDPosSetter = new();
        }

        /// <summary>
        /// call exatly once after PlayState.InitializeNew()
        /// </summary>
        public void SetWorld(Graph graph, WorldHUD worldHUD)
        {
            this.graph = graph;
            if (activeUIElements.Contains(graph))
                throw new InvalidOperationException();
            activeUIElements.Add(graph);
            activeUIElements.Add(worldHUD);
            if (!HUDElements.Add(worldHUD))
                throw new ArgumentException();
        }

        //public void AddHUDElement(IHUDElement HUDElement, HorizPos horizPos, VertPos vertPos)
        //{
        //    if (HUDElement is null)
        //        return;

        //    HUDPosSetter.AddHUDElement(HUDElement: HUDElement, horizPos: horizPos, vertPos: vertPos);

        //    //sizeOrPosChangedListeners[HUDElement] = new HUDElementSizeOrPosChangedListener(HorizPos: horizPos, VertPos: vertPos);
        //    //sizeOrPosChangedListeners[HUDElement].SizeOrPosChangedResponse(shape: HUDElement.Shape);
        //    //HUDElement.SizeOrPosChanged.Add(listener: sizeOrPosChangedListeners[HUDElement]);

        //    activeUIElements.Add(HUDElement);
        //    if (!HUDElements.Add(HUDElement))
        //        throw new ArgumentException();
        //}

        //public void RemoveHUDElement(IHUDElement HUDElement)
        //{
        //    if (HUDElement is null)
        //        return;
        //    if (!HUDElements.Remove(HUDElement))
        //        throw new ArgumentException();
        //    activeUIElements.Remove(HUDElement);
        //    HUDPosSetter.RemoveHUDElement(HUDElement: HUDElement);
        //    //HUDElement.SizeOrPosChanged.Remove(listener: sizeOrPosChangedListeners[HUDElement]);
        //    //sizeOrPosChangedListeners.Remove(HUDElement);
        //}

        public void Update(TimeSpan elapsed)
        {
            IUIElement prevContMouse = contMouse;

            MouseState mouseState = Mouse.GetState();
            prevLeftDown = leftDown;
            leftDown = mouseState.LeftButton == ButtonState.Pressed;
            Vector2 mouseScreenPos = mouseState.Position.ToVector2(),
                mouseWorldPos = CurWorldManager.MouseWorldPos,
                mouseHUDPos = HUDCamera.HUDPos(screenPos: mouseScreenPos);

            contMouse = null;
            MouseAboveHUD = false;
            foreach (var UIElement in Enumerable.Reverse(activeUIElements))
            {
                Vector2 mousePos = (UIElement == graph) switch
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
                        right: (float)screenWidth,
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

        public void DrawHUD()
        {
            HUDCamera.BeginDraw();
            foreach (var UIElement in HUDElements)
                UIElement.Draw();
            explanationTextBox.Draw();
            HUDCamera.EndDraw();
        }
    }
}
