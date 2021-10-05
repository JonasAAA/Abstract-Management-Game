using Microsoft.Xna.Framework;
using System;

namespace Game1
{
    public class NodeState
    {
        public readonly Vector2 position;
        public ULongArray storedRes;
        public readonly ulong maxBatchDemResStored;
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
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
