using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Game1
{
    public class Resource
    {
        public static uint Count
            => (uint)all.Count;
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
                    id: 0,
                    weight: 2
                ),
                new
                (
                    id: 0,
                    weight: 10
                ),
            });
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
