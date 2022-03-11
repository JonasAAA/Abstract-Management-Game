using Microsoft.Xna.Framework;
using System;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class NodeState
    {
        // TODO: define using the new notation
        //public double Mass
        //    => MathHelper.Pi * radius * radius * CurWorldConfig.planetMassPerUnitArea;
        //public double SurfaceLength
        //    => MathHelper.TwoPi * radius;
        //public double SurfaceGravitationalAccel
        //    => CurWorldConfig.gravitConst * Mass / Math.Pow(radius, CurWorldConfig.gravitPower);

        public readonly IReadOnlyChangingFloat radius;
        public readonly Vector2 position;
        public readonly ulong maxBatchDemResStored;
        public ULongArray storedRes;
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
        public readonly MySet<Person> waitingPeople;

        private readonly ChangingFloat changingRadius;

        public NodeState(Vector2 position, float radius, ulong maxBatchDemResStored)
        {
            this.position = position;
            changingRadius = new(radius);
            this.radius = changingRadius;
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            storedRes = new();
            this.maxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = new();
            waitingPeople = new();
        }

        public float SetRadius(float radius)
            => changingRadius.Value = radius;
    }
}
