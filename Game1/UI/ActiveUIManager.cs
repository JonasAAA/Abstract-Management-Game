using Game1.Events;
using Game1.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Game1.UI
{
    [Serializable]
    public class ActiveUIManager
    {
        public static UIConfig CurUIConfig { get; private set; }
        public static double ScreenWidth { get; private set; }
        public static double ScreenHeight
            => CurUIConfig.standardScreenHeight;
        public static Vector2 MouseHUDPos
            => HUDCamera.HUDPos(screenPos: Mouse.GetState().Position.ToVector2());
        public static float RectOutlineWidth
            => CurUIConfig.rectOutlineWidth;

        private static HUDCamera HUDCamera;

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            CurUIConfig = new();
            Camera.Initialize(graphicsDevice: graphicsDevice);
            ScreenWidth = (double)graphicsDevice.Viewport.Width * CurUIConfig.standardScreenHeight / graphicsDevice.Viewport.Height;
            HUDCamera = new();
        }

        public Event<IClickedNowhereListener> clickedNowhere;

        private readonly List<IUIElement> activeUIElements;
        private readonly HashSet<IHUDElement> HUDElements;
        private readonly Dictionary<IUIElement, IPosTransformer> nonHUDElementsToTransform;
        private bool leftDown, prevLeftDown;
        private IUIElement halfClicked, contMouse;
        private readonly TimeSpan minDurationToGetExplanation;
        private TimeSpan hoverDuration;
        private readonly TextBox explanationTextBox;
        private readonly HUDPosSetter HUDPosSetter;

        public ActiveUIManager()
        {
            clickedNowhere = new();

            activeUIElements = new();
            HUDElements = new();
            leftDown = new();
            prevLeftDown = new();
            halfClicked = null;
            contMouse = null;
            minDurationToGetExplanation = TimeSpan.FromSeconds(.5);
            hoverDuration = TimeSpan.Zero;

            HUDPosSetter = new();
            nonHUDElementsToTransform = new();

            explanationTextBox = new();
            explanationTextBox.Shape.Color = Color.LightPink;
        }

        public void AddNonHUDElement(IUIElement UIElement, IPosTransformer posTransformer)
        {
            activeUIElements.Add(UIElement);
            nonHUDElementsToTransform.Add(UIElement, posTransformer);
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

        public void EnableAllUIElements()
        {
            foreach (var UIElement in activeUIElements)
                UIElement.HasDisabledAncestor = false;
        }

        public void DisableAllUIElements()
        {
            foreach (var UIElement in activeUIElements)
                UIElement.HasDisabledAncestor = true;
        }

        public void Update(TimeSpan elapsed)
        {
            IUIElement prevContMouse = contMouse;

            MouseState mouseState = Mouse.GetState();
            prevLeftDown = leftDown;
            leftDown = mouseState.LeftButton == ButtonState.Pressed;
            Vector2 mouseScreenPos = mouseState.Position.ToVector2(),
                mouseHUDPos = HUDCamera.HUDPos(screenPos: mouseScreenPos);

            contMouse = null;
            foreach (var UIElement in Enumerable.Reverse(activeUIElements))
            {
                Vector2 mousePos = nonHUDElementsToTransform.ContainsKey(UIElement) switch
                {
                    true => nonHUDElementsToTransform[UIElement].Transform(screenPos: mouseScreenPos),
                    false => mouseHUDPos
                };

                IUIElement catchingUIElement = UIElement.CatchUIElement(mousePos: mousePos);

                if (catchingUIElement is not null)
                {
                    contMouse = catchingUIElement;
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
