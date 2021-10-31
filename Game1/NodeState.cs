using Microsoft.Xna.Framework;
using System;
using System.Runtime.Serialization;

namespace Game1
{
    [DataContract]
    public class NodeState
    {
        [DataMember]
        public readonly Vector2 position;
        [DataMember]
        public ULongArray storedRes;
        [DataMember]
        public readonly ulong maxBatchDemResStored;
        [DataMember]
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
        [DataMember]
        public readonly MyHashSet<Person> waitingPeople;

        public NodeState(Vector2 position, ulong maxBatchDemResStored)
        {
            this.position = position;
            storedRes = new();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = new();
            waitingPeople = new();
        }
    }
}
