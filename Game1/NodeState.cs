using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class NodeState
    {
        // TODO: define using the new notation
        //public double Mass
        //    => MathHelper.Pi * radius * radius * CurWorldConfig.planetMassPerUnitArea;
        
        //public double SurfaceGravitationalAccel
        //    => CurWorldConfig.gravitConst * Mass / MathHelper.Pow(radius, CurWorldConfig.gravitPower);
        public ulong mass
            => mainResAmount * consistsOfRes.mass;
        public ulong area
            => mainResAmount * consistsOfRes.area;
        public UDouble radius
            => MyMathHelper.Sqrt(value: area / MyMathHelper.pi);
        public ulong approxSurfaceLength
            => (ulong)(2 * MyMathHelper.pi * radius);
        public ulong mainResAmount { get; private set; }
        public readonly MyVector2 position;
        public readonly ulong maxBatchDemResStored;
        public ResAmounts storedRes;
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
        public readonly MySet<Person> waitingPeople;
        public readonly BasicResInd consistsOfResInd;
        public readonly Resource consistsOfRes;

        public NodeState(MyVector2 position, UDouble approxRadius, BasicResInd consistsOfResInd, ulong maxBatchDemResStored)
        {
            this.position = position;
            consistsOfRes = CurResConfig.resources[consistsOfResInd];
            mainResAmount = Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / consistsOfRes.area);
            // TODO: delete
            //mainResAmount = changingResAmount;
            //mass = mainResAmount * consistsOfRes.mass;
            //area = mainResAmount * consistsOfRes.area;
            //radius = MyMathHelper.Sqrt(value: area.ToReadOnlyChangingUDouble() / MyMathHelper.pi);
            //approxSurfaceLength = (2 * MyMathHelper.pi * radius).RoundDown();
            this.consistsOfResInd = consistsOfResInd;
            storedRes = new();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = new();
            waitingPeople = new();
        }

        public bool CanRemove(ulong resAmount)
            => mainResAmount >= resAmount + CurWorldConfig.minResAmountInPlanet;

        public void Remove(ulong resAmount)
        {
            if (!CanRemove(resAmount: resAmount))
                throw new ArgumentException();
            mainResAmount -= resAmount;
        }
        
        public void AddToStoredRes(ResInd resInd, ulong resAmount)
            => storedRes = storedRes.WithAdd(index: resInd, value: resAmount);
    }
}
