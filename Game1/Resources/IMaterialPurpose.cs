namespace Game1.Resources
{
    public interface IMaterialPurpose
    {
        [Serializable]
        private sealed class Mechanical : IMaterialPurpose
        {
            string IMaterialPurpose.Name
                => "Mechanical";

            string IMaterialPurpose.TooltipTextFor(Material material)
                // Should be something like "Strength at current conditions is 10.3, the bigger the better"
                => throw new NotImplementedException();

            bool IMaterialPurpose.LiquidUse(Material material)
                => false;
        }

        //[Serializable]
        //private sealed class Structural : IMaterialPurpose
        //{ }

        [Serializable]
        private sealed class HydraulicFluid : IMaterialPurpose
        {
            string IMaterialPurpose.Name
                => "Hydraulic Fluid";

            string IMaterialPurpose.TooltipTextFor(Material material)
                // Should be something like "Strength at current conditions is 10.3, the bigger the better"
                => throw new NotImplementedException();

            bool IMaterialPurpose.LiquidUse(Material material)
                => true;
        }

        [Serializable]
        private sealed class RoofSurface : IMaterialPurpose
        {
            string IMaterialPurpose.Name
                => "Roof Surface";

            string IMaterialPurpose.TooltipTextFor(Material material)
                // Should be something like "Strength at current conditions is 10.3, the bigger the better"
                => throw new NotImplementedException();

            bool IMaterialPurpose.LiquidUse(Material material)
                => false;
        }

        //[Serializable]
        //private sealed class ElectricalConductor : IMaterialPurpose
        //{ }

        //[Serializable]
        //private sealed class ElectricalInsulator : IMaterialPurpose
        //{ }

        public static readonly IReadOnlyCollection<IMaterialPurpose> all;

        public static readonly IMaterialPurpose mechanical = new Mechanical();
        public static readonly IMaterialPurpose hydraulicFluid = new HydraulicFluid();
        public static readonly IMaterialPurpose roofSurface = new RoofSurface();
        //public static readonly IMaterialPurpose electricalConductor = new ElectricalConductor();
        //public static readonly IMaterialPurpose electricalInsulator = new ElectricalInsulator();

        static IMaterialPurpose()
        {
            all = new IMaterialPurpose[] { mechanical, hydraulicFluid, roofSurface };
#warning Check that all contains all public fields and properties of type IMaterialPurpose. This should go into test project probably.
        }

        public string Name { get; }

        public string TooltipTextFor(Material material);

        /// <summary>
        /// If tempearature is any higher, than the returned value, and the product uses this material for this purpose
        /// the product is destroyed, i.e. turns into garbage.
        /// </summary>
        public sealed Temperature DestructionPoint(Material material)
            => LiquidUse(material: material) ? Temperature.CreateFromK(valueInK: UDouble.positiveInfinity) : material.MeltingPoint;

        protected bool LiquidUse(Material material);
    }
}
