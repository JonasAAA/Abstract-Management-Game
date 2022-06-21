using Game1.Industries;
using Game1.Inhabitants;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class NodeState : IIndustryFacingNodeState
    {
        public static ulong ResAmountFromApproxRadius(BasicResInd basicResInd, UDouble approxRadius)
            => Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / CurResConfig.resources[basicResInd].area);

        // TODO: define using the new notation
        //public double SurfaceGravitationalAccel
        //    => CurWorldConfig.gravitConst * Mass / MathHelper.Pow(radius, CurWorldConfig.gravitPower);
        public NodeID NodeID { get; }

        // TODO: could include linkEndPoints mass in this
        public ulong Mass { get; private set; }
        //=> planetMass + StoredResPile.TotalMass + waitingResAmountsPackets.TotalMass + (Industry?.Mass ?? 0);
        public ulong Area { get; private set; }
        public UDouble Radius { get; private set; }
        public ulong ApproxSurfaceLength { get; private set; }
        public ulong MainResAmount
            => consistsOfResPile[ConsistsOfResInd];
        public ulong MaxAvailableResAmount
            => MainResAmount - CurWorldConfig.minResAmountInPlanet;
        public MyVector2 Position { get; }
        public ulong MaxBatchDemResStored { get; }
        public ResPile StoredResPile { get; }
        public readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
        public RealPeople WaitingPeople { get; }
        public BasicResInd ConsistsOfResInd { get; }
        public BasicRes ConsistsOfRes { get; }
        public bool TooManyResStored { get; set; }
        public UDouble WattsHittingSurfaceOrIndustry { get; set; }

        private readonly ResPile consistsOfResPile;
        //private ulong planetMass;

        public NodeState(NodeID nodeID, MyVector2 position, BasicResInd consistsOfResInd, ulong mainResAmount, ResPile resSource, ulong maxBatchDemResStored)
        {
            NodeID = nodeID;
            Position = position;
            ConsistsOfResInd = consistsOfResInd;
            ConsistsOfRes = CurResConfig.resources[consistsOfResInd];
            consistsOfResPile = ResPile.CreateEmpty();
            EnlargeFrom(source: resSource, resAmount: mainResAmount);
            
            StoredResPile = ResPile.CreateEmpty();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            MaxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty();
            WaitingPeople = RealPeople.CreateEmpty();
            TooManyResStored = false;
            WattsHittingSurfaceOrIndustry = 0;
            Mass = MainResAmount * ConsistsOfRes.mass;
        }

        public void Initialize(ulong startingNonPlanetMass)
            => Mass += startingNonPlanetMass;

        public bool CanRemove(ulong resAmount)
            => MainResAmount >= resAmount + CurWorldConfig.minResAmountInPlanet;

        private void RecalculateValues()
        {
            Area = MainResAmount * ConsistsOfRes.area;
            Radius = MyMathHelper.Sqrt(value: Area / MyMathHelper.pi);
            ApproxSurfaceLength = (ulong)(2 * MyMathHelper.pi * Radius);
        }

        public void MineTo(ResPile destin, ulong resAmount)
        {
            if (!CanRemove(resAmount: resAmount))
                throw new ArgumentException();
            var reservedResPile = ReservedResPile.CreateIfHaveEnough
            (
                source: consistsOfResPile,
                resAmount: new(resInd: ConsistsOfResInd, amount: resAmount)
            );
            Debug.Assert(reservedResPile is not null);
            ReservedResPile.TransferAll(reservedSource: ref reservedResPile, destin: destin);
            RecalculateValues();
        }

        public void EnlargeFrom(ResPile source, ulong resAmount)
        {
            var reservedResPile = ReservedResPile.CreateIfHaveEnough
            (
                source: source,
                resAmount: new(resInd: ConsistsOfResInd, amount: resAmount)
            );
            if (reservedResPile is null)
                throw new ArgumentException();
            ReservedResPile.TransferAll(reservedSource: ref reservedResPile, destin: consistsOfResPile);
            RecalculateValues();
        }

        public void RegisterArriving(IHasMass hasMass)
            => Mass += hasMass.Mass;

        public void RegisterLeaving(IHasMass hasMass)
            => Mass -= hasMass.Mass;
    }
}
