using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class ResConfig
    {
        public int ResCount
            => resources.Count;
        public readonly ReadOnlyCollection<Resource> resources;

        public ResConfig()
        {
            resources = new(new List<Resource>()
            {
                new
                (
                    id: 0,
                    weight: 1
                ),
                new
                (
                    id: 1,
                    weight: 2
                ),
                new
                (
                    id: 2,
                    weight: 10
                ),
            });

            Debug.Assert((int)MaxRes == ResCount - 1);
            Debug.Assert(ResCount + 3 == Enum.GetValues<Overlay>().Length);
        }
    }
}
