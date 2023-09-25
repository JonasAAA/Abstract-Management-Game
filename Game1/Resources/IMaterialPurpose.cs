using Game1.Collections;
using Game1.UI;

namespace Game1.Resources
{
    public interface IMaterialPurpose : IHasToString
    {
        [Serializable]
        private sealed class Mechanical : IMaterialPurpose
        {
            public sealed override string ToString()
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
            public sealed override string ToString()
                => "Roof Surface";
        }

        [Serializable]
        private sealed class ElectricalConductor : IMaterialPurpose
        {
            public sealed override string ToString()
                => "Electrical Conductor";
        }

        [Serializable]
        private sealed class ElectricalInsulator : IMaterialPurpose
        {
            public sealed override string ToString()
                => "Electrical Insulator";
        }

        public static readonly IMaterialPurpose mechanical = new Mechanical();
        //public static readonly IMaterialPurpose hydraulicFluid = new HydraulicFluid();
        public static readonly IMaterialPurpose roofSurface = new RoofSurface();
        public static readonly IMaterialPurpose electricalConductor = new ElectricalConductor();
        public static readonly IMaterialPurpose electricalInsulator = new ElectricalInsulator();

        // DON'T forget to put all material purposes in this list.
        // There is a test to check that
        public static readonly EfficientReadOnlyCollection<IMaterialPurpose> all = new List<IMaterialPurpose> { mechanical, /* hydraulicFluid, */ roofSurface, electricalConductor, electricalInsulator }.ToEfficientReadOnlyCollection();
    }
}
