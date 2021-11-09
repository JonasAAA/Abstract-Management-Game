using System;
using System.Runtime.Serialization;

namespace Game1
{
    [DataContract]
    public class Resource
    {
        [DataMember] public readonly int id;
        [DataMember] public readonly ulong weight;

        public Resource(int id, ulong weight)
        {
            this.id = id;
            if (weight is 0)
                throw new ArgumentOutOfRangeException();
            this.weight = weight;
        }
    }
}
