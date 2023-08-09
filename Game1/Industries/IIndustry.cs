using Game1.Collections;
using Game1.UI;

namespace Game1.Industries
{
    public interface IIndustry : IEnergyConsumer, IDeletable
    {
        public string Name { get; }
        public IBuildingImage BuildingImage { get; }

        /// <summary>
        /// Null if no building
        /// </summary>
        public Material? SurfaceMaterial { get; }

        public IHUDElement UIElement { get; }

        public EfficientReadOnlyCollection<IResource> PotentiallyNotNeededBuildingComponents { get; }

        public EfficientReadOnlyCollection<IResource> GetConsumedResources();

        public EfficientReadOnlyCollection<IResource> GetProducedResources();

        public AllResAmounts TargetStoredResAmounts();

        public void FrameStartNoProduction(string error);

        public void FrameStart();

        public IIndustry? Update();

        public string GetInfo();
    }
}
