using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;

using static Game1.WorldManager;

namespace Game1.UI
{
    [Serializable]
    // TChild is WorldUIElement to disallow text and similar UI elements which would should not scale when player zooms in/out
    // The correct approach here would be to have TChild IWorldUIElement (that being new interface) but I'm not sure if that'll work
    // with the save system (as UIElement<IUIElement> and UIElement<IWorldUIElement> would be indistinguishable types for the save system
    public abstract class WorldUIElement : UIElement<WorldUIElement>
    {
        public readonly Event<IActiveChangedListener> activeChanged;

        public sealed override bool CanBeClicked
            => !Active;

        public bool Active
        {
            get => active;
            set
            {
                if (active == value)
                    return;

                active = value;
                if (active)
                    ShowPopups(popups: Popups);
                else
                    HidePopups(popups: Popups);
                activeChanged.Raise(action: listener => listener.ActiveChangedResponse(worldUIElement: this));
            }
        }
        
        private bool active;

        /// <summary>
        /// POPUP must never change identity, at least while active. If it does, that will not be reflected in the UI and
        /// previous popup will not be removed from ActiveUIManager, thus always staying on screen
        /// </summary>
        protected abstract EfficientReadOnlyCollection<(IHUDElement popup, IAction popupHUDPosUpdater)> Popups { get; }

        protected WorldUIElement(Shape shape)
            : base(shape: shape)
        {
            activeChanged = new();
            active = false;
        }

        protected void RefreshPopups(EfficientReadOnlyCollection<(IHUDElement popup, IAction popupHUDPosUpdater)> oldPopups, EfficientReadOnlyCollection<(IHUDElement popup, IAction popupHUDPosUpdater)> newPopups)
        {
            if (active)
            {
                HidePopups(popups: oldPopups);
                ShowPopups(popups: newPopups);
            }
        }

        private static void ShowPopups(EfficientReadOnlyCollection<(IHUDElement popup, IAction popupHUDPosUpdater)> popups)
        {
            foreach (var (popup, HUDPosUpdater) in popups)
                CurWorldManager.AddWorldHUDElement
                (
                    worldHUDElement: popup,
                    updateHUDPos: HUDPosUpdater
                );
        }

        private static void HidePopups(EfficientReadOnlyCollection<(IHUDElement popup, IAction popupHUDPosUpdater)> popups)
        {
            foreach (var (popup, _) in popups)
                CurWorldManager.RemoveWorldHUDElement
                (
                    worldHUDElement: popup
                );
        }

        public sealed override void OnClick()
        {
            if (Active)
                return;
            base.OnClick();

            Active = true;
        }

        protected virtual void Delete()
        { }
    }
}
