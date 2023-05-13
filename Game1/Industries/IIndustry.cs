using Game1.Lighting;
using Game1.UI;

namespace Game1.Industries
{
    public interface IIndustry
    {
        public ILightBlockingObject? LightBlockingObject { get; }

        /// <summary>
        /// Null if no building
        /// </summary>
        public Propor? SurfaceReflectance { get; }

        /// <summary>
        /// Null if no building
        /// </summary>
        public Propor? SurfaceEmissivity { get; }

        public IHUDElement UIElement { get; }

        public SomeResAmounts<IResource> TargetStoredResAmounts();

        public IIndustry? Update();

        public string GetInfo();

        public void DrawBeforePlanet(Color otherColor, Propor otherColorPropor);

        public void DrawAfterPlanet();
    }
}
