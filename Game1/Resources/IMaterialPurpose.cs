﻿using Game1.UI;

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

#warning Check that all contains all public fields and properties of type IMaterialPurpose. This should go into test project probably.
        public static readonly IReadOnlyCollection<IMaterialPurpose> all = new IMaterialPurpose[] { mechanical, /* hydraulicFluid, */ roofSurface, electricalConductor, electricalInsulator };

        public sealed string TooltipTextFor(Material material)
            => UIAlgorithms.ChooseMaterialForMaterialPurpose(material: material, materialPurpose: this);
    }
}
