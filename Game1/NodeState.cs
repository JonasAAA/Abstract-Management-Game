using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class NodeState
    {
        public readonly Position position;
        public ULongArray storedRes;
        public readonly ulong maxBatchDemResStored;
        public List<Person> unemployedPeople;
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
        public List<Person> waitingPeople;

        public NodeState(Position position, ulong maxBatchDemResStored)
        {
            this.position = position;
            storedRes = new();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            unemployedPeople = new();
            waitingResAmountsPackets = new();
            waitingPeople = new();
        }
    }
}
