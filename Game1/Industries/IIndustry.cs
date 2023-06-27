using Game1.Collections;
using Game1.UI;

namespace Game1.Industries
{
    public interface IIndustry : IEnergyConsumer, IDeletable
    {
        public string Name { get; }
        public IBuildingImage BuildingImage { get; }

        //public ILightBlockingObject? BuildingShape { get; }

        /// <summary>
        /// Null if no building
        /// </summary>
        public Material? SurfaceMaterial { get; }

        public IHUDElement UIElement { get; }

        public SomeResAmounts<IResource> TargetStoredResAmounts();

        public void FrameStartNoProduction(string error);

        public void FrameStart();

        public IIndustry? Update();

        public string GetInfo();

        //public void Draw(Color otherColor, Propor otherColorPropor);
    }
}
