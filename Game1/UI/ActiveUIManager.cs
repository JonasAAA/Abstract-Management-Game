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
            => HUDCamera.ScreenPosToHUDPos(screenPos: (MyVector2)Mouse.GetState().Position);
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

        public static MyVector2 ScreenPosToHUDPos(MyVector2 screenPos)
            => HUDCamera.ScreenPosToHUDPos(screenPos: screenPos);

        public static MyVector2 HUDPosToScreenPos(MyVector2 HUDPos)
            => HUDCamera.HUDPosToScreenPos(HUDPos: HUDPos);

        public static UDouble HUDLengthToScreenLength(UDouble HUDLength)
            => HUDCamera.HUDLengthToScreenLength(HUDLength: HUDLength);

        //public Event<IClickedNowhereListener> clickedNowhere;

        private readonly List<IUIElement> activeUIElements;
        private readonly HashSet<IHUDElement> HUDElements;
        private readonly Dictionary<IHUDElement, IAction> worldHUDElementToUpdateHUDPosAction;
        private readonly HashSet<IUIElement> worldUIElements;
        private readonly AbstractButton mouseLeftButton;
        private IUIElement? halfClicked, contMouse;
        private readonly TimeSpan minDurationToGetTooltip;
        private TimeSpan hoverDuration;
        private ITooltip? tooltip;
        private readonly HUDPosSetter HUDPosSetter;
        private readonly WorldCamera? worldCamera;
        private IHUDElement? HUDPopup;

        public ActiveUIManager(WorldCamera? worldCamera)
        {
            this.worldCamera = worldCamera;
            //clickedNowhere = new();

            activeUIElements = new();
            HUDElements = new();
            mouseLeftButton = new();
            halfClicked = null;
            contMouse = null;
            minDurationToGetTooltip = TimeSpan.FromSeconds(.5);
            hoverDuration = TimeSpan.Zero;

            HUDPosSetter = new();
            worldUIElements = new();
            worldHUDElementToUpdateHUDPosAction = new();

            tooltip = null;
            HUDPopup = null;
        }

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
        public void AddHUDElement(IHUDElement? HUDElement, PosEnums position)
        {
            if (HUDElement is null)
                return;

            HUDPosSetter.AddHUDElement(HUDElement: HUDElement, position: position);

            activeUIElements.Add(HUDElement);
            if (!HUDElements.Add(HUDElement))
                throw new ArgumentException();
        }

        /// <summary>
        /// The popup will disappear when player presses anywhere.
        /// </summary>
        public void SetHUDPopup(IHUDElement HUDElement, MyVector2 HUDPos, PosEnums origin)
        {
            HUDPopup = HUDElement;

            HUDPosSetter.AddHUDElement(HUDElement: HUDElement, HUDPos: HUDPos, origin: origin);

            activeUIElements.Add(HUDElement);
        }

        private void RemoveHUDPopup()
        {
            if (HUDPopup is null)
                return;
            HUDPosSetter.RemoveHUDElement(HUDElement: HUDPopup);
            activeUIElements.Remove(HUDPopup);
            HUDPopup = null;
        }

        /// <summary>
        /// worldHUDElement will be drawn by this
        /// </summary>
        public void AddWorldHUDElement(IHUDElement worldHUDElement, IAction updateHUDPos)
        {
            activeUIElements.Add(worldHUDElement);
            worldHUDElementToUpdateHUDPosAction.Add(key: worldHUDElement, value: updateHUDPos);
        }

        public void RemoveWorldHUDElement(IHUDElement worldHUDElement)
        {
            activeUIElements.Remove(worldHUDElement);
            worldHUDElementToUpdateHUDPosAction.Remove(key: worldHUDElement);
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
            foreach (var worldHUDElementUpdatePosAction in worldHUDElementToUpdateHUDPosAction.Values)
                worldHUDElementUpdatePosAction.Invoke();
            IUIElement? prevContMouse = contMouse;

            MouseState mouseState = Mouse.GetState();
            mouseLeftButton.Update(down: mouseState.LeftButton == ButtonState.Pressed);
            MyVector2 mouseScreenPos = (MyVector2)mouseState.Position,
                mouseHUDPos = HUDCamera.ScreenPosToHUDPos(screenPos: mouseScreenPos);

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
                    if (HUDPopup == UIElement || (UIElement is HUDElement HUDElement && (HUDElements.Contains(HUDElement) || worldHUDElementToUpdateHUDPosAction.ContainsKey(HUDElement))))
                        return HUDCamera.ScreenPosToHUDPos(screenPos: mouseScreenPos);
                    if (worldUIElements.Contains(UIElement))
                        return worldCamera!.ScreenPosToWorldPos(screenPos: mouseScreenPos);
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

            if (mouseLeftButton.HalfClicked)
                halfClicked = contMouse;

            if (mouseLeftButton.Clicked)
            {
                RemoveHUDPopup();

                IUIElement? otherHalfClicked = contMouse;
                if (halfClicked == otherHalfClicked && otherHalfClicked?.Enabled is true && otherHalfClicked.CanBeClicked)
                    otherHalfClicked.OnClick();
                //else
                //    clickedNowhere.Raise(action: listener => listener.ClickedNowhereResponse());

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
            foreach (var worldHUDElement in worldHUDElementToUpdateHUDPosAction.Keys)
                worldHUDElement.Draw();
            foreach (var HUDElement in HUDElements)
                HUDElement.Draw();
            HUDPopup?.Draw();
            tooltip?.Draw();
            HUDCamera.EndDraw();
        }
    }
}
