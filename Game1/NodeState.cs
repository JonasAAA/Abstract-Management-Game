using Game1.ChangingValues;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class NodeState
    {
        // TODO: define using the new notation
        //public double Mass
        //    => MathHelper.Pi * radius * radius * CurWorldConfig.planetMassPerUnitArea;
        public readonly IReadOnlyChangingULong approxSurfaceLength;
        //public double SurfaceGravitationalAccel
        //    => CurWorldConfig.gravitConst * Mass / MathHelper.Pow(radius, CurWorldConfig.gravitPower);

        public readonly IReadOnlyChangingUDouble radius;
        public readonly MyVector2 position;
        public readonly ulong maxBatchDemResStored;
        public ResAmounts storedRes;
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
        public readonly MySet<Person> waitingPeople;
        public readonly BasicResInd consistsOf;

        private readonly ChangingUDouble changingRadius;

        public NodeState(MyVector2 position, UDouble radius, BasicResInd consistsOf, ulong maxBatchDemResStored)
        {
            this.position = position;
            changingRadius = new(radius);
            this.radius = changingRadius;
            approxSurfaceLength = (2 * MyMathHelper.pi * this.radius).RoundDown();
            this.consistsOf = consistsOf;
            storedRes = new();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = new();
            waitingPeople = new();
        }

        public void SetRadius(UDouble radius)
            => changingRadius.Value = radius;
        
        public void AddToStoredRes(ResInd resInd, ulong resAmount)
            => storedRes = storedRes.WithAdd(index: resInd, value: resAmount);
    }
}
