using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public class ActiveUIManager
    {
        public static readonly UIConfig curUIConfig;
        public static readonly UDouble screenWidth, screenHeight;
        public static MyVector2 MouseHUDPos
            => HUDCamera.HUDPos(screenPos: (MyVector2)Mouse.GetState().Position);
        public static UDouble RectOutlineWidth
            => curUIConfig.rectOutlineWidth;

        private static readonly HUDCamera HUDCamera;

        static ActiveUIManager()
        {
            curUIConfig = new();
            Camera.Initialize();
            screenWidth = (UDouble)C.GraphicsDevice.Viewport.Width * curUIConfig.standardScreenHeight / (UDouble)C.GraphicsDevice.Viewport.Height;
            screenHeight = curUIConfig.standardScreenHeight;
            HUDCamera = new();
        }

        public Event<IClickedNowhereListener> clickedNowhere;

        private readonly List<IUIElement> activeUIElements;
        private readonly HashSet<IHUDElement> HUDElements;
        private readonly Dictionary<IUIElement, IPosTransformer> nonHUDElementsToTransform;
        private bool leftDown, prevLeftDown;
        private IUIElement? halfClicked, contMouse;
        private readonly TimeSpan minDurationToGetExplanation;
        private TimeSpan hoverDuration;
        private ITooltip? tooltip;
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

            tooltip = null;
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

        public void AddHUDElement(IHUDElement? HUDElement, HorizPos horizPos, VertPos vertPos)
        {
            if (HUDElement is null)
                return;

            HUDPosSetter.AddHUDElement(HUDElement: HUDElement, horizPos: horizPos, vertPos: vertPos);

            activeUIElements.Add(HUDElement);
            if (!HUDElements.Add(HUDElement))
                throw new ArgumentException();
        }

        public void RemoveHUDElement(IHUDElement? HUDElement)
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
            IUIElement? prevContMouse = contMouse;

            MouseState mouseState = Mouse.GetState();
            prevLeftDown = leftDown;
            leftDown = mouseState.LeftButton == ButtonState.Pressed;
            MyVector2 mouseScreenPos = (MyVector2)mouseState.Position,
                mouseHUDPos = HUDCamera.HUDPos(screenPos: mouseScreenPos);

            contMouse = null;
            foreach (var UIElement in Enumerable.Reverse(activeUIElements))
            {
                MyVector2 mousePos = nonHUDElementsToTransform.ContainsKey(UIElement) switch
                {
                    true => nonHUDElementsToTransform[UIElement].Transform(screenPos: mouseScreenPos),
                    false => mouseHUDPos
                };

                IUIElement? catchingUIElement = UIElement.CatchUIElement(mousePos: mousePos);

                if (catchingUIElement is not null)
                {
                    contMouse = catchingUIElement;
                    break;
                }
            }

            if (contMouse == prevContMouse)
            {
                hoverDuration += elapsed;
                if (contMouse is not null && contMouse.Enabled && hoverDuration >= minDurationToGetExplanation && tooltip is null && contMouse is IWithTooltip UIElementWithTooltip)
                {
                    tooltip = UIElementWithTooltip.Tooltip;
                    tooltip.Shape.TopLeftCorner = mouseHUDPos;
                    tooltip.Shape.ClampPosition
                    (
                        left: 0,
                        right: screenWidth,
                        top: 0,
                        bottom: screenHeight
                    );
                }
            }
            else
            {
                hoverDuration = TimeSpan.Zero;
                tooltip = null;
                if (prevContMouse is not null && prevContMouse.Enabled)
                    prevContMouse.MouseOn = false;
                if (contMouse is not null && contMouse.Enabled)
                    contMouse.MouseOn = true;
            }

            if (leftDown && !prevLeftDown)
                halfClicked = contMouse;

            if (!leftDown && prevLeftDown)
            {
                IUIElement? otherHalfClicked = contMouse;
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
            tooltip?.Draw();
            HUDCamera.EndDraw();
        }
    }
}
