using Game1.Collections;

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

        [Serializable]
        public sealed class Options
        {
            public readonly IMaterialPurpose mechanical;
            //public readonly IMaterialPurpose hydraulicFluid;
            public readonly IMaterialPurpose roofSurface;
            public readonly IMaterialPurpose electricalConductor;
            public readonly IMaterialPurpose electricalInsulator;

            // DON'T forget to put all material purposes in this list.
            // There is a test to check that
            public readonly EfficientReadOnlyCollection<IMaterialPurpose> all;
        
            public Options()
            {
                mechanical = new Mechanical();
                //hydraulicFluid = new HydraulicFluid();
                roofSurface = new RoofSurface();
                electricalConductor = new ElectricalConductor();
                electricalInsulator = new ElectricalInsulator();

                all = new List<IMaterialPurpose> { mechanical, /* hydraulicFluid, */ roofSurface, electricalConductor, electricalInsulator }.ToEfficientReadOnlyCollection();
            }
        }
    }
}
