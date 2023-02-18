using Game1.Industries;
using Game1.Inhabitants;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class NodeState : IIndustryFacingNodeState
    {
        public static ulong ResAmountFromApproxRadius(BasicResInd basicResInd, UDouble approxRadius)
            => Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / CurResConfig.resources[basicResInd].Area);

        // TODO: define using the new notation
        //public double SurfaceGravitationalAccel
        //    => CurWorldConfig.gravitConst * Mass / MathHelper.Pow(radius, CurWorldConfig.gravitPower);
        public NodeID NodeID { get; }
        public Mass PlanetMass
            => consistsOfResPile.Amount.Mass();
        public ulong Area { get; private set; }
        public UDouble Radius { get; private set; }
        public ulong ApproxSurfaceLength { get; private set; }
        public ulong MainResAmount
            => consistsOfResPile.Amount[ConsistsOfResInd];
        public ulong MaxAvailableResAmount
            => MainResAmount - CurWorldConfig.minResAmountInPlanet;
        public MyVector2 Position { get; }
        public ulong MaxBatchDemResStored { get; }
        public ResPile StoredResPile { get; }
        public EnergyPile<RadiantEnergy> RadiantEnergyPile { get; }
        public readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
        public RealPeople WaitingPeople { get; }
        public BasicResInd ConsistsOfResInd { get; }
        public BasicRes ConsistsOfRes { get; }
        public bool TooManyResStored { get; set; }
        // TODO: could include linkEndPoints Mass in the Counter<Mass> in this NodeState
        public LocationCounters LocationCounters { get; }
        public ThermalBody ThermalBody { get; }
        public UDouble SurfaceGravity
            => WorldFunctions.SurfaceGravity(mass: LocationCounters.GetCount<ResAmounts>().Mass(), radius: Radius);

        private readonly ResPile consistsOfResPile;

        public NodeState(MyVector2 position, BasicResInd consistsOfResInd, ulong mainResAmount, ResPile resSource, ulong maxBatchDemResStored)
        {
            LocationCounters = LocationCounters.CreateEmpty();
            ThermalBody = ThermalBody.CreateEmpty(locationCounters: LocationCounters);
            NodeID = NodeID.Create();
            Position = position;
            ConsistsOfResInd = consistsOfResInd;
            ConsistsOfRes = CurResConfig.resources[consistsOfResInd];
            consistsOfResPile = ResPile.CreateEmpty(thermalBody: ThermalBody);
            EnlargeFrom(source: resSource, resAmount: mainResAmount);
            
            StoredResPile = ResPile.CreateEmpty(thermalBody: ThermalBody);
            RadiantEnergyPile = EnergyPile<RadiantEnergy>.CreateEmpty(locationCounters: LocationCounters);
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            MaxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(thermalBody: ThermalBody);
            WaitingPeople = RealPeople.CreateEmpty(thermalBody: ThermalBody, energyDistributor: CurWorldManager.EnergyDistributor);
            TooManyResStored = false;
        }

        public bool CanRemove(ulong resAmount)
            => MainResAmount >= resAmount + CurWorldConfig.minResAmountInPlanet;

        private void RecalculateValues()
        {
            Area = MainResAmount * ConsistsOfRes.Area;
            Radius = MyMathHelper.Sqrt(value: Area / MyMathHelper.pi);
            ApproxSurfaceLength = (ulong)(2 * MyMathHelper.pi * Radius);
        }

        public void MineTo(ResPile destin, ulong resAmount)
        {
            if (!CanRemove(resAmount: resAmount))
                throw new ArgumentException();
            var reservedResPile = ResPile.CreateIfHaveEnough
            (
                source: consistsOfResPile,
                amount: new(resInd: ConsistsOfResInd, amount: resAmount)
            );
            Debug.Assert(reservedResPile is not null);
            destin.TransferAllFrom(source: reservedResPile);
            RecalculateValues();
        }

        public void EnlargeFrom(ResPile source, ulong resAmount)
        {
            var reservedResPile = ResPile.CreateIfHaveEnough
            (
                source: source,
                amount: new(resInd: ConsistsOfResInd, amount: resAmount)
            );
            if (reservedResPile is null)
                throw new ArgumentException();
            consistsOfResPile.TransferAllFrom(source: reservedResPile);
            RecalculateValues();
        }
    }
}
