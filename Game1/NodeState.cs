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
        public readonly IReadOnlyChangingULong mass, area;
        public readonly IReadOnlyChangingUDouble radius;
        public readonly IReadOnlyChangingULong mainResAmount;
        public readonly MyVector2 position;
        public readonly ulong maxBatchDemResStored;
        public ResAmounts storedRes;
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
        public readonly MySet<Person> waitingPeople;
        public readonly BasicResInd consistsOf;

        private readonly ChangingULong changingResAmount;

        public NodeState(MyVector2 position, UDouble approxRadius, BasicResInd consistsOf, ulong maxBatchDemResStored)
        {
            this.position = position;
            Resource consistsOfRes = CurResConfig.resources[consistsOf];
            changingResAmount = new(value: Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / consistsOfRes.area));
            mainResAmount = changingResAmount;
            mass = mainResAmount * consistsOfRes.mass;
            area = mainResAmount * consistsOfRes.area;
            radius = MyMathHelper.Sqrt(value: area.ToReadOnlyChangingUDouble() / MyMathHelper.pi);
            approxSurfaceLength = (2 * MyMathHelper.pi * radius).RoundDown();
            this.consistsOf = consistsOf;
            storedRes = new();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = new();
            waitingPeople = new();
        }

        public bool CanRemove(ulong resAmount)
            => changingResAmount.Value >= resAmount + CurWorldConfig.minResAmountInPlanet;

        public void Remove(ulong resAmount)
        {
            if (!CanRemove(resAmount: resAmount))
                throw new ArgumentException();
            changingResAmount.Value -= resAmount;
        }
        
        public void AddToStoredRes(ResInd resInd, ulong resAmount)
            => storedRes = storedRes.WithAdd(index: resInd, value: resAmount);
    }
}
