using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class NodeState
    {
        // TODO: define using the new notation
        //public double SurfaceGravitationalAccel
        //    => CurWorldConfig.gravitConst * Mass / MathHelper.Pow(radius, CurWorldConfig.gravitPower);
        public readonly NodeId nodeId;

        // TODO: inlcude other objects with mass in this calculation, i.e. buildings, people, resources, etc.
        public ulong Mass { get; private set; }
        public ulong Area { get; private set; }
        public UDouble Radius { get; private set; }
        public ulong ApproxSurfaceLength { get; private set; }
        public ulong MainResAmount
        {
            get => mainResAmount;
            private set
            {
                ulong prevMainResAmount = MainResAmount;
                mainResAmount = value;
                if (prevMainResAmount != value)
                    RecalculateValues();
            }
        }
        public MyVector2 position;
        public readonly ulong maxBatchDemResStored;
        public ResAmounts storedRes;
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
        public readonly MySet<Person> waitingPeople;
        public readonly BasicResInd consistsOfResInd;
        public readonly Resource consistsOfRes;

        // NEVER TO BE USED DIRECTLY
        private ulong mainResAmount;

        public NodeState(NodeId nodeId, MyVector2 position, UDouble approxRadius, BasicResInd consistsOfResInd, ulong maxBatchDemResStored)
        {
            this.nodeId = nodeId;
            this.position = position;
            consistsOfRes = CurResConfig.resources[consistsOfResInd];
            MainResAmount = Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / consistsOfRes.area);
            
            this.consistsOfResInd = consistsOfResInd;
            storedRes = new();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = new();
            waitingPeople = new();
        }

        public bool CanRemove(ulong resAmount)
            => MainResAmount >= resAmount + CurWorldConfig.minResAmountInPlanet;

        public void Remove(ulong resAmount)
        {
            if (!CanRemove(resAmount: resAmount))
                throw new ArgumentException();
            MainResAmount -= resAmount;
        }

        private void RecalculateValues()
        {
            Mass = MainResAmount * consistsOfRes.mass;
            Area = MainResAmount * consistsOfRes.area;
            Radius = MyMathHelper.Sqrt(value: Area / MyMathHelper.pi);
            ApproxSurfaceLength = (ulong)(2 * MyMathHelper.pi * Radius);
        }
        
        public void AddToStoredRes(ResInd resInd, ulong resAmount)
            => storedRes = storedRes.WithAdd(index: resInd, value: resAmount);
    }
}
