using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Game1
{
    public class Resource
    {
        public static int Count
            => all.Count;
        public static readonly ReadOnlyCollection<Resource> all;

        static Resource()
        {
            all = new(new List<Resource>()
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

            Debug.Assert((int)C.MaxRes == Count - 1);
            Debug.Assert(Count + 3 == Enum.GetValues<Overlay>().Length);
        }

        public readonly int id;
        public readonly ulong weight;

        private Resource(int id, ulong weight)
        {
            this.id = id;
            if (weight is 0)
                throw new ArgumentOutOfRangeException();
            this.weight = weight;
        }
    }
}
