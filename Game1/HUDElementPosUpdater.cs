using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;

namespace Game1
{
    [Serializable]
    public sealed class HUDElementPosUpdater : IAction
    {
        private readonly IHUDElement HUDElement;
        private readonly IWithStandardPositions baseWorldObject;
        private readonly PosEnums HUDElementOrigin, anchorInBaseWorldObject;

        // TODO: HUDElementOrigin could be the opposite of anchorInBaseWorldObject
        public HUDElementPosUpdater(IHUDElement HUDElement, IWithStandardPositions baseWorldObject, PosEnums HUDElementOrigin, PosEnums anchorInBaseWorldObject)
        {
            this.HUDElement = HUDElement;
            this.baseWorldObject = baseWorldObject;
            this.HUDElementOrigin = HUDElementOrigin;
            this.anchorInBaseWorldObject = anchorInBaseWorldObject;
        }

        void IAction.Invoke()
            => HUDElement.Shape.SetPosition
            (
                position: WorldManager.CurWorldManager.WorldPosToHUDPos(worldPos: baseWorldObject.GetPosition(origin: anchorInBaseWorldObject)),
                origin: HUDElementOrigin
            );
    }
}
