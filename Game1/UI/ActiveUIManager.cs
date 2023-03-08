using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class ActiveUIManager
    {
        public static readonly UIConfig curUIConfig;
        public static readonly ColorConfig colorConfig;
        public static readonly UDouble screenWidth, screenHeight;
        public static MyVector2 MouseHUDPos
            => HUDCamera.HUDPos(screenPos: (MyVector2)Mouse.GetState().Position);
        public static UDouble RectOutlineWidth
            => curUIConfig.rectOutlineWidth;

        private static readonly HUDCamera HUDCamera;

        static ActiveUIManager()
        {
            curUIConfig = new();
            colorConfig = new();
            Camera.Initialize();
            screenWidth = (UDouble)C.GraphicsDevice.Viewport.Width * curUIConfig.standardScreenHeight / (UDouble)C.GraphicsDevice.Viewport.Height;
            screenHeight = curUIConfig.standardScreenHeight;
            HUDCamera = new();
        }

        public Event<IClickedNowhereListener> clickedNowhere;

        private readonly List<IUIElement> activeUIElements;
        private readonly HashSet<IHUDElement> HUDElements, worldHUDElements;
        private readonly HashSet<IUIElement> worldUIElements;
        private bool leftDown, prevLeftDown;
        private IUIElement? halfClicked, contMouse;
        private readonly TimeSpan minDurationToGetTooltip;
        private TimeSpan hoverDuration;
        private ITooltip? tooltip;
        private readonly HUDPosSetter HUDPosSetter;
        private readonly WorldCamera? worldCamera;

        public ActiveUIManager(WorldCamera? worldCamera)
        {
            this.worldCamera = worldCamera;
            clickedNowhere = new();

            activeUIElements = new();
            HUDElements = new();
            leftDown = new();
            prevLeftDown = new();
            halfClicked = null;
            contMouse = null;
            minDurationToGetTooltip = TimeSpan.FromSeconds(.5);
            hoverDuration = TimeSpan.Zero;

            HUDPosSetter = new();
            worldUIElements = new();
            worldHUDElements = new();

            tooltip = null;
        }

        public static MyVector2 ScreenPosToHUDPos(MyVector2 screenPos)
            => HUDCamera.HUDPos(screenPos: screenPos);

        /// <summary>
        /// HUDElement is will not be drawn by this
        /// </summary>
        public void AddWorldUIElement(IUIElement UIElement)
        {
            activeUIElements.Add(UIElement);
            worldUIElements.Add(UIElement);
        }

        public void RemoveWorldUIElement(IUIElement UIElement)
        {
            activeUIElements.Remove(UIElement);
            worldUIElements.Remove(UIElement);
        }

        /// <summary>
        /// HUDElement will be drawn by this
        /// </summary>
        public void AddHUDElement(IHUDElement? HUDElement, HorizPos horizPos, VertPos vertPos)
        {
            if (HUDElement is null)
                return;

            HUDPosSetter.AddHUDElement(HUDElement: HUDElement, horizPos: horizPos, vertPos: vertPos);

            activeUIElements.Add(HUDElement);
            if (!HUDElements.Add(HUDElement))
                throw new ArgumentException();
        }

        /// <summary>
        /// worldHUDElement will be drawn by this
        /// </summary>
        public void AddWorldHUDElement(IHUDElement worldHUDElement)
        {
            activeUIElements.Add(worldHUDElement);
            worldHUDElements.Add(worldHUDElement);
        }

        public void RemoveWorldHUDElement(IHUDElement worldHUDElement)
        {
            activeUIElements.Remove(worldHUDElement);
            worldHUDElements.Remove(worldHUDElement);
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
            foreach (IUIElement UIElement in Enumerable.Reverse(activeUIElements))
            {
                IUIElement? catchingUIElement = UIElement.CatchUIElement(mousePos: getMousePos());

                if (catchingUIElement is not null)
                {
                    contMouse = catchingUIElement;
                    break;
                }

                MyVector2 getMousePos()
                {
                    if (HUDElements.Contains(UIElement) || worldHUDElements.Contains(UIElement))
                        return HUDCamera.HUDPos(screenPos: mouseScreenPos);
                    if (worldUIElements.Contains(UIElement))
                        return worldCamera!.WorldPos(screenPos: mouseScreenPos);
                    throw new();
                }
            }

            if (contMouse == prevContMouse)
            {
                hoverDuration += elapsed;
                if (contMouse?.Enabled is true && hoverDuration >= minDurationToGetTooltip && tooltip is null && contMouse is IWithTooltip UIElementWithTooltip)
                {
                    tooltip = UIElementWithTooltip.Tooltip;
                    tooltip.Update();
                    tooltip.Shape.TopLeftCorner = mouseHUDPos;
                }
            }
            else
            {
                hoverDuration = TimeSpan.Zero;
                tooltip = null;
                if (prevContMouse?.Enabled is true)
                    prevContMouse.MouseOn = false;

                if (contMouse?.Enabled is true)
                    contMouse.MouseOn = true;
            }

            if (leftDown && !prevLeftDown)
                halfClicked = contMouse;

            if (!leftDown && prevLeftDown)
            {
                IUIElement? otherHalfClicked = contMouse;
                if (halfClicked == otherHalfClicked && otherHalfClicked?.Enabled is true && otherHalfClicked.CanBeClicked)
                    otherHalfClicked.OnClick();
                else
                    clickedNowhere.Raise(action: listener => listener.ClickedNowhereResponse());

                halfClicked = null;
            }

            tooltip?.Update();
            tooltip?.Shape.ClampPosition
            (
                left: 0,
                right: screenWidth,
                top: 0,
                bottom: screenHeight
            );
        }

        public void DrawHUD()
        {
            HUDCamera.BeginDraw();
            foreach (var worldHUDElement in worldHUDElements)
                worldHUDElement.Draw();
            foreach (var HUDElement in HUDElements)
                HUDElement.Draw();
            tooltip?.Draw();
            HUDCamera.EndDraw();
        }
    }
}
