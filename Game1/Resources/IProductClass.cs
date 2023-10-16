﻿using Game1.Collections;
using static Game1.WorldManager;

namespace Game1.Resources
{
    public interface IProductClass : IHasToString
    {
        [Serializable]
        private sealed class Mechanical : IProductClass
        {
            public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToAmount
                => new()
                {
                    [CurWorldManager.matPurposeOptions.mechanical] = 1
                };

            public sealed override string ToString()
                => "Mechanical";
        }

        [Serializable]
        private sealed class Electronics : IProductClass
        {
            public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToAmount
                => new()
                {
                    [CurWorldManager.matPurposeOptions.electricalConductor] = 1,
                    [CurWorldManager.matPurposeOptions.electricalInsulator] = 1
                };

            public sealed override string ToString()
                => "Electronics";
        }

        [Serializable]
        private sealed class Roof : IProductClass
        {
            public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToAmount
                => new()
                {
                    [CurWorldManager.matPurposeOptions.roofSurface] = 1
                };

            public sealed override string ToString()
                => "Roof";
        }

        public static readonly IProductClass mechanical = new Mechanical();
        public static readonly IProductClass electronics = new Electronics();
        public static readonly IProductClass roof = new Roof();

        // DON'T forget to put all material purposes in this list.
        // There is a test to check that
        public static readonly EfficientReadOnlyCollection<IProductClass> all = new List<IProductClass>() { mechanical, electronics, roof }.ToEfficientReadOnlyCollection();

        public EfficientReadOnlyDictionary<IMaterialPurpose, ulong> MatPurposeToAmount { get; }

        public sealed void ThrowIfWrongIMatPurposeSet(EfficientReadOnlyDictionary<IMaterialPurpose, Material> materialChoices)
            => MatPurposeToAmount.Keys.ToHashSet().SetEquals(materialChoices.Keys);

        public sealed T SwitchExpression<T>(Func<T> mechanical, Func<T> electronics, Func<T> roof)
            => this switch
            {
                Mechanical => mechanical(),
                Electronics => electronics(),
                Roof => roof(),
                _ => throw new InvalidStateException()
            };
    }
}
