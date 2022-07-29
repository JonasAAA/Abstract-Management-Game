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
        public Mass PlanetMass
            => consistsOfResPile.Mass;
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
        // TODO: could include linkEndPoints mass in this
        public MassCounter MassCounter { get; }
        public UDouble SurfaceGravity
            => WorldFunctions.SurfaceGravity(mass: MassCounter.Mass, radius: Radius);

        private readonly ResPile consistsOfResPile;

        public NodeState(NodeID nodeID, MyVector2 position, BasicResInd consistsOfResInd, ulong mainResAmount, ResPile resSource, ulong maxBatchDemResStored)
        {
            MassCounter = MassCounter.CreateEmpty();
            NodeID = nodeID;
            Position = position;
            ConsistsOfResInd = consistsOfResInd;
            ConsistsOfRes = CurResConfig.resources[consistsOfResInd];
            consistsOfResPile = ResPile.CreateEmpty(locationMassCounter: MassCounter);
            EnlargeFrom(source: resSource, resAmount: mainResAmount);
            
            StoredResPile = ResPile.CreateEmpty(locationMassCounter: MassCounter);
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            MaxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(locationMassCounter: MassCounter);
            WaitingPeople = RealPeople.CreateEmpty(locationMassCounter: MassCounter);
            TooManyResStored = false;
            WattsHittingSurfaceOrIndustry = 0;
        }

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
            destin.TransferAllFrom(reservedSource: ref reservedResPile);
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
            consistsOfResPile.TransferAllFrom(reservedSource: ref reservedResPile);
            RecalculateValues();
        }
    }
}
