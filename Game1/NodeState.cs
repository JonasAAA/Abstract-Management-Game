using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class NodeState
    {
        public readonly Vector2 position;
        public ULongArray storedRes, waitingRes;
        public readonly ulong maxBatchDemResStored;
        public readonly List<Person> employees, travelingEmployees, unemployedPeople, travellingPeople;

        public NodeState(Vector2 position, ulong maxBatchDemResStored)
        {
            this.position = position;
            storedRes = new();
            waitingRes = new();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            employees = new();
            travelingEmployees = new();
            unemployedPeople = new();
            travellingPeople = new();
        }
    }
}
