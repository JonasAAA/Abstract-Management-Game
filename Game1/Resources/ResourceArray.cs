﻿using static Game1.UI.ActiveUIManager;

namespace Game1.Resources
{
    [Serializable]
    public sealed class ResourceArray : IMyArray<IResource>
    {
        private readonly ConstArray<IResource> resources;

        public ResourceArray()
        {
            resources = new ConstArray<IResource>
            (
                selector: resInd => (ulong)resInd switch
                {
                    < BasicResInd.count => (ulong)resInd switch
                    {
                        0 => new BasicRes
                        (
                            resInd: (BasicResInd)resInd,
                            mass: Mass.CreateFromKg(valueInKg: 2),
                            heatCapacity: HeatCapacity.CreateFromJPerK(valueInJPerK: 3),
                            area: 1,
                            color: colorConfig.Res0Color
                        ),
                        1 => new BasicRes
                        (
                            resInd: (BasicResInd)resInd,
                            mass: Mass.CreateFromKg(valueInKg: 1),
                            heatCapacity: HeatCapacity.CreateFromJPerK(valueInJPerK: 4),
                            area: 2,
                            color: colorConfig.Res1Color
                        ),
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    < BasicResInd.count + NonBasicResInd.count => (ulong)resInd switch
                    {
                        2 => new NonBasicRes
                        (
                            resInd: (NonBasicResInd)resInd,
                            ingredients: new ResAmounts()
                            {
                                [(ResInd)0] = 1,
                                [(ResInd)1] = 2,
                            }
                        ),
                        3 => new NonBasicRes
                        (
                            resInd: (NonBasicResInd)resInd,
                            ingredients: new ResAmounts()
                            {
                                [(ResInd)0] = 1,
                                [(ResInd)1] = 2,
                                [(ResInd)2] = 1,
                            }
                        ),
                        4 => new NonBasicRes
                        (
                            resInd: (NonBasicResInd)resInd,
                            ingredients: new ResAmounts()
                            {
                                [(ResInd)3] = 10,
                            }
                        ),
                        _ => throw new ArgumentOutOfRangeException()
                    },
                    _ => throw new ArgumentOutOfRangeException()
                }
            );
        }

        public void Initialize()
        {
            foreach (var resource in resources)
                if (resource is NonBasicRes nonBasicRes)
                    nonBasicRes.Initialize();

            // validate that resource invariants hold
            foreach (var resInd in ResInd.All)
                Debug.Assert(resources[resInd].ResInd == resInd);

            foreach (var resource in resources)
                Debug.Assert(resource is BasicRes or NonBasicRes);

            foreach (var basicResInd in BasicResInd.All)
                Debug.Assert(resources[basicResInd] is BasicRes);

            foreach (var nonBasicResInd in NonBasicResInd.All)
                Debug.Assert(resources[nonBasicResInd] is NonBasicRes);

            foreach (var resource in resources)
                Debug.Assert(!resource.Mass.IsZero);
        }

        public IResource this[ResInd resInd]
            => resources[resInd];

        public BasicRes this[BasicResInd basicResInd]
            => (BasicRes)resources[basicResInd];

        public NonBasicRes this[NonBasicResInd nonBasicResInd]
            => (NonBasicRes)resources[nonBasicResInd];
    }
}
