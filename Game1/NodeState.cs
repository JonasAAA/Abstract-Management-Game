using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class NodeState
    {
        public static ulong ResAmountFromApproxRadius(BasicResInd basicResInd, UDouble approxRadius)
            => Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / CurResConfig.resources[basicResInd].area);

        // TODO: define using the new notation
        //public double SurfaceGravitationalAccel
        //    => CurWorldConfig.gravitConst * Mass / MathHelper.Pow(radius, CurWorldConfig.gravitPower);
        public readonly NodeID nodeID;

        // TODO: inlcude other objects with mass in this calculation, i.e. buildings, people, resources, etc.
        public ulong Mass { get; private set; }
        public ulong Area { get; private set; }
        public UDouble Radius { get; private set; }
        public ulong ApproxSurfaceLength { get; private set; }
        public ulong MainResAmount
            => consistsOfResPile[consistsOfResInd];
        public ulong MaxAvailableResAmount
            => MainResAmount - CurWorldConfig.minResAmountInPlanet;
        public readonly MyVector2 position;
        public readonly ulong maxBatchDemResStored;
        public readonly ResPile storedResPile;
        public ResAmountsPacketsByDestin waitingResAmountsPackets;
        public readonly MySet<Person> waitingPeople;
        public readonly BasicResInd consistsOfResInd;
        public readonly BasicRes consistsOfRes;
        public UDouble wattsHittingSurfaceOrIndustry;

        private readonly ResPile consistsOfResPile;

        public NodeState(NodeID nodeID, MyVector2 position, BasicResInd consistsOfResInd, ulong mainResAmount, ResPile resSource, ulong maxBatchDemResStored)
        {
            this.nodeID = nodeID;
            this.position = position;
            this.consistsOfResInd = consistsOfResInd;
            consistsOfRes = CurResConfig.resources[consistsOfResInd];
            consistsOfResPile = ResPile.CreateEmpty();
            EnlargeFrom(source: resSource, resAmount: mainResAmount);
            
            storedResPile = ResPile.CreateEmpty();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = new();
            waitingPeople = new();
            wattsHittingSurfaceOrIndustry = 0;
        }

        public bool CanRemove(ulong resAmount)
            => MainResAmount >= resAmount + CurWorldConfig.minResAmountInPlanet;

        private void RecalculateValues()
        {
            Mass = MainResAmount * consistsOfRes.mass;
            Area = MainResAmount * consistsOfRes.area;
            Radius = MyMathHelper.Sqrt(value: Area / MyMathHelper.pi);
            ApproxSurfaceLength = (ulong)(2 * MyMathHelper.pi * Radius);
        }

        public void MineTo(ResPile destin, ulong resAmount)
        {
            if (!CanRemove(resAmount: resAmount))
                throw new ArgumentException();
            var reservedResPile = ReservedResPile.Create
            (
                source: consistsOfResPile,
                resAmount: new(resInd: consistsOfResInd, amount: resAmount)
            );
            Debug.Assert(reservedResPile is not null);
            ReservedResPile.TransferAll(reservedSource: ref reservedResPile, destin: destin);
            RecalculateValues();
        }

        public void EnlargeFrom(ResPile source, ulong resAmount)
        {
            var reservedResPile = ReservedResPile.Create
            (
                source: source,
                resAmount: new(resInd: consistsOfResInd, amount: resAmount)
            );
            if (reservedResPile is null)
                throw new ArgumentException();
            ReservedResPile.TransferAll(reservedSource: ref reservedResPile, destin: consistsOfResPile);
            RecalculateValues();
        }

        // TODO: delete if unused
        //public void TransferToStorage(ResPile resPile)
        //    => ResPile.TransferAll(source: resPile, destin: storedResPile);

        //public void AddRes(ulong resAmount)
        //    => MainResAmount += resAmount;

        //public void RemoveRes(ulong resAmount)
        //{
        //    if (!CanRemove(resAmount: resAmount))
        //        throw new ArgumentException();
        //    MainResAmount -= resAmount;
        //}


        //public void AddToStoredRes(ResInd resInd, ulong resAmount)
        //    => storedRes = storedRes.WithAdd(index: resInd, value: resAmount);
    }
}
