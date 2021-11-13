using Game1.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
//using static Game1.WorldManager;

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

        public Event<IClickedNowhereListener> clickedNowhere;

        public double ScreenHeight
            => UIConfig.standardScreenHeight;

        public Vector2 MouseHUDPos
            => HUDCamera.HUDPos(screenPos: Mouse.GetState().Position.ToVector2());

        public bool MouseAboveHUD { get; private set; }

        public readonly double screenWidth;

        private readonly UIConfig UIConfig;
        private readonly List<IUIElement> activeUIElements;
        private readonly HashSet<IHUDElement> HUDElements;
        private readonly Dictionary<IUIElement, Func<Vector2, Vector2>> nonHUDElementsToTransform;
        private bool leftDown, prevLeftDown;
        private IUIElement halfClicked, contMouse;
        private readonly TimeSpan minDurationToGetExplanation;
        private TimeSpan hoverDuration;
        private TextBox explanationTextBox;
        private HUDCamera HUDCamera;
        private readonly HUDPosSetter HUDPosSetter;

        private ActiveUIManager(GraphicsDevice graphicsDevice)
        {
            clickedNowhere = new();

            UIConfig = new();
            activeUIElements = new();
            HUDElements = new();
            leftDown = new();
            prevLeftDown = new();
            halfClicked = null;
            contMouse = null;
            MouseAboveHUD = true;
            minDurationToGetExplanation = TimeSpan.FromSeconds(.5);
            hoverDuration = TimeSpan.Zero;

            screenWidth = (double)graphicsDevice.Viewport.Width * UIConfig.standardScreenHeight / graphicsDevice.Viewport.Height;
            HUDPosSetter = new();
            nonHUDElementsToTransform = new();
        }

        public void AddNonHUDElement(IUIElement UIElement, Func<Vector2, Vector2> screenToPos)
        {
            activeUIElements.Add(UIElement);
            nonHUDElementsToTransform.Add(UIElement, screenToPos);
        }

        public void RemoveNonHUDElement(IUIElement UIElement)
        {
            activeUIElements.Remove(UIElement);
            nonHUDElementsToTransform.Remove(UIElement);
        }

        public void AddHUDElement(IHUDElement HUDElement, HorizPos horizPos, VertPos vertPos)
        {
            if (HUDElement is null)
                return;

            HUDPosSetter.AddHUDElement(HUDElement: HUDElement, horizPos: horizPos, vertPos: vertPos);

            activeUIElements.Add(HUDElement);
            if (!HUDElements.Add(HUDElement))
                throw new ArgumentException();
        }

        public void RemoveHUDElement(IHUDElement HUDElement)
        {
            if (HUDElement is null)
                return;
            if (!HUDElements.Remove(HUDElement))
                throw new ArgumentException();
            activeUIElements.Remove(HUDElement);
            HUDPosSetter.RemoveHUDElement(HUDElement: HUDElement);
        }

        public void Update(TimeSpan elapsed)
        {
            IUIElement prevContMouse = contMouse;

            MouseState mouseState = Mouse.GetState();
            prevLeftDown = leftDown;
            leftDown = mouseState.LeftButton == ButtonState.Pressed;
            Vector2 mouseScreenPos = mouseState.Position.ToVector2(),
                //mouseWorldPos = CurWorldManager.MouseWorldPos,
                mouseHUDPos = HUDCamera.HUDPos(screenPos: mouseScreenPos);

            contMouse = null;
            MouseAboveHUD = false;
            foreach (var UIElement in Enumerable.Reverse(activeUIElements))
            {
                Vector2 mousePos = nonHUDElementsToTransform.ContainsKey(UIElement) switch
                {
                    true => nonHUDElementsToTransform[UIElement](mouseScreenPos),
                    false => mouseHUDPos
                };
                //Vector2 mousePos = HUDElements.Contains(UIElement) switch
                //{
                //    true => mouseHUDPos,
                //    false => mouseWorldPos
                //};

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
                if (halfClicked == otherHalfClicked && otherHalfClicked is not null && otherHalfClicked.Enabled && otherHalfClicked.CanBeClicked)
                    otherHalfClicked.OnClick();
                else
                    clickedNowhere.Raise(action: listener => listener.ClickedNowhereResponse());

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
