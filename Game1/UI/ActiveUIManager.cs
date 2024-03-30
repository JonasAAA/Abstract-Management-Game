using Game1.Delegates;
using Game1.Shapes;

namespace Game1.UI
{
    [Serializable]
    public sealed class ActiveUIManager
    {
        public static readonly UDouble standardScreenHeight = 1080;
        public static readonly ColorConfig colorConfig = new();
        public static readonly UDouble
            screenWidth = (UDouble)C.GraphicsDevice.Viewport.Width * standardScreenHeight / (UDouble)C.GraphicsDevice.Viewport.Height,
            screenHeight = standardScreenHeight;
        public static Vector2Bare MouseHUDPos
            => HUDCamera.ScreenPosToHUDPos(screenPos: (Vector2Bare)Mouse.GetState().Position);

        private static readonly HUDCamera HUDCamera = new();

        public static Vector2Bare ScreenPosToHUDPos(Vector2Bare screenPos)
            => HUDCamera.ScreenPosToHUDPos(screenPos: screenPos);

        public static Vector2Bare HUDPosToScreenPos(Vector2Bare HUDPos)
            => HUDCamera.HUDPosToScreenPos(HUDPos: HUDPos);

        public static UDouble HUDLengthToScreenLength(UDouble HUDLength)
            => HUDCamera.HUDLengthToScreenLength(HUDLength: HUDLength);

        /// <summary>
        /// THIS should not be relied on to draw stuff in correct order or to catch mouse clicks
        /// That's because the top most drawn element should have the toppriority in terms of catching clicks
        /// World UI elements are always drawn first, but they may not appear first in this list
        /// </summary>
        private readonly List<IUIElement> activeUIElements;
        private readonly List<IHUDElement> HUDElements;
        private readonly List<(IHUDElement HUDElement, IAction updateHUDPos)> worldHUDElements;
        private readonly List<IUIElement> worldUIElements;
        private readonly AbstractButton mouseLeftButton;
        private IUIElement? halfClicked, contMouse;
        private readonly TimeSpan minDurationToGetTooltip;
        private TimeSpan hoverDuration;
        private ITooltip? tooltip;
        private readonly HUDPosSetter HUDPosSetter;
        private IHUDElement? HUDPopup;
        private IAction? oneUseClickedNowhereResponse;

        public ActiveUIManager()
        {
            activeUIElements = [];
            HUDElements = [];
            mouseLeftButton = new();
            halfClicked = null;
            contMouse = null;
            minDurationToGetTooltip = TimeSpan.FromSeconds(.5);
            hoverDuration = TimeSpan.Zero;

            HUDPosSetter = new();
            worldUIElements = [];
            worldHUDElements = [];

            tooltip = null;
            HUDPopup = null;
            oneUseClickedNowhereResponse = null;
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
            if (!activeUIElements.Remove(UIElement))
                throw new ArgumentException();
            if (!worldUIElements.Remove(UIElement))
                throw new ArgumentException();
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
            HUDElements.Add(HUDElement);
        }

        /// <summary>
        /// The popup will disappear when player presses anywhere.
        /// </summary>
        public void SetHUDPopup(IHUDElement HUDElement, Vector2Bare HUDPos, PosEnums origin)
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
            Debug.Assert(activeUIElements.Remove(HUDPopup));
            HUDPopup = null;
        }

        /// <summary>
        /// worldHUDElement will be drawn by this
        /// </summary>
        public void AddWorldHUDElement(IHUDElement worldHUDElement, IAction updateHUDPos)
        {
            activeUIElements.Add(worldHUDElement);
            worldHUDElements.Add((worldHUDElement, updateHUDPos));
        }

        public void RemoveWorldHUDElement(IHUDElement worldHUDElement)
        {
            if (!activeUIElements.Remove(worldHUDElement))
                throw new ArgumentException();
            if (!worldHUDElements.Remove(worldHUDElements.Find(el => el.HUDElement == worldHUDElement)))
                throw new ArgumentException();
        }

        public void RemoveHUDElement(IHUDElement? HUDElement)
        {
            if (HUDElement is null)
                return;
            if (!HUDElements.Remove(HUDElement))
                throw new ArgumentException();
            if (!activeUIElements.Remove(HUDElement))
                throw new ArgumentException();
            HUDPosSetter.RemoveHUDElement(HUDElement: HUDElement);
        }

        public void SetOneUseClickedNowhereResponse(IAction oneUseClickedNowhereResponse)
            => this.oneUseClickedNowhereResponse = oneUseClickedNowhereResponse;

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
            foreach (var (_, worldHUDElementUpdatePosAction) in worldHUDElements)
                worldHUDElementUpdatePosAction.Invoke();
            IUIElement? prevContMouse = contMouse;

            MouseState mouseState = Mouse.GetState();
            mouseLeftButton.Update(down: mouseState.LeftButton == ButtonState.Pressed);
            Vector2Bare mouseScreenPos = (Vector2Bare)mouseState.Position,
                mouseHUDPos = HUDCamera.ScreenPosToHUDPos(screenPos: mouseScreenPos);

            contMouse = CatchUIElement(mouseScreenPos: mouseScreenPos, mouseHUDPos: mouseHUDPos);

            Mouse.SetCursor(contMouse?.CanBeClicked is true ? MouseCursor.Hand : MouseCursor.Arrow);

            if (contMouse == prevContMouse)
            {
                hoverDuration += elapsed;
                if (contMouse?.Enabled is true && hoverDuration >= minDurationToGetTooltip && tooltip is null
                    && contMouse is IMaybeWithTooltip UIElementWithTooltip && UIElementWithTooltip.Tooltip is ITooltip notNullTooltip)
                {
                    tooltip = notNullTooltip;
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
                if (halfClicked == otherHalfClicked && otherHalfClicked?.Enabled is true)
                    otherHalfClicked.OnClick();
                else
                {
                    oneUseClickedNowhereResponse?.Invoke();
                    oneUseClickedNowhereResponse = null;
                }

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

        private IEnumerable<IHUDElement> AllHUDElements
        {
            get
            {
                foreach (var (HUDElement, _) in worldHUDElements)
                    yield return HUDElement;
                foreach (var HUDElement in HUDElements)
                    yield return HUDElement;
                if (HUDPopup is not null)
                    yield return HUDPopup;
            }
        }

        private IUIElement? CatchUIElement(Vector2Bare mouseScreenPos, Vector2Bare mouseHUDPos)
            => worldUIElements.Select
            (
                worldUIElement => worldUIElement.CatchUIElement(mouseScreenPos: mouseScreenPos)
            ).Concat
            (
                AllHUDElements.Select
                (
                    HUDElement => HUDElement.CatchUIElement(mouseScreenPos: mouseHUDPos)
                )
            ).Where(UIElement => UIElement is not null)
            .LastOrDefault();

        public void DrawHUD()
        {
            HUDCamera.BeginDraw();
            foreach (var HUDElement in AllHUDElements)
                HUDElement.Draw();
            tooltip?.Draw();
            HUDCamera.EndDraw();
        }
    }
}
