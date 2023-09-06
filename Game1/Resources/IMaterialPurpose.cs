using Game1.UI;

namespace Game1.Resources
{
    public interface IMaterialPurpose
    {
        [Serializable]
        private sealed class Mechanical : IMaterialPurpose
        {
            string IMaterialPurpose.Name
                => "Mechanical";
        }

        //[Serializable]
        //private sealed class Structural : IMaterialPurpose
        //{ }

        //[Serializable]
        //private sealed class HydraulicFluid : IMaterialPurpose
        //{
        //    string IMaterialPurpose.Name
        //        => "Hydraulic Fluid";
        //}

        [Serializable]
        private sealed class RoofSurface : IMaterialPurpose
        {
            string IMaterialPurpose.Name
                => "Roof Surface";
        }

        [Serializable]
        private sealed class ElectricalConductor : IMaterialPurpose
        {
            string IMaterialPurpose.Name
                => "Electrical Conductor";
        }

        [Serializable]
        private sealed class ElectricalInsulator : IMaterialPurpose
        {
            string IMaterialPurpose.Name
                => "Electrical Insulator";
        }

        public static readonly IReadOnlyCollection<IMaterialPurpose> all;

        public static readonly IMaterialPurpose mechanical = new Mechanical();
        //public static readonly IMaterialPurpose hydraulicFluid = new HydraulicFluid();
        public static readonly IMaterialPurpose roofSurface = new RoofSurface();
        public static readonly IMaterialPurpose electricalConductor = new ElectricalConductor();
        public static readonly IMaterialPurpose electricalInsulator = new ElectricalInsulator();

        static IMaterialPurpose()
            => all = new IMaterialPurpose[] { mechanical, /* hydraulicFluid, */ roofSurface, electricalConductor, electricalInsulator };
#warning Check that all contains all public fields and properties of type IMaterialPurpose. This should go into test project probably.

        public string Name { get; }

        public sealed string TooltipTextFor(Material material)
            => UIAlgorithms.ChooseMaterialForMaterialPurpose(material: material, materialPurpose: this);
    }
}
