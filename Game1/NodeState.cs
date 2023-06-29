using Game1.Collections;
using Game1.ContentHelpers;
using Game1.Industries;
using Game1.Inhabitants;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class NodeState : IIndustryFacingNodeState
    {
        public static ulong ResAmountFromApproxRadius(RawMaterial rawMat, UDouble approxRadius)
            => Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / rawMat.Area.valueInMetSq);

        public NodeID NodeID { get; }
        public Mass PlanetMass
            => consistsOfResPile.Amount.Mass();
        public AreaInt Area { get; private set; }
        public UDouble Radius { get; private set; }
        public UDouble SurfaceLength { get; private set; }
        //public ulong MainResAmount
        //    => consistsOfResPile.Amount.rawMatsMix[Composition];
        //public ulong MaxAvailableResAmount
        //    => MainResAmount - CurWorldConfig.minResAmountInPlanet;
        public MyVector2 Position { get; }
        public ulong MaxBatchDemResStored { get; }
        public ResPile StoredResPile { get; }
        public EnergyPile<RadiantEnergy> RadiantEnergyPile { get; }
        public readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
        public RealPeople WaitingPeople { get; }
        public SomeResAmounts<RawMaterial> Composition { get; private set; }
        public bool TooManyResStored { get; set; }
        // TODO: could include linkEndPoints Mass in the Counter<Mass> in this NodeState
        public LocationCounters LocationCounters { get; }
        public ThermalBody ThermalBody { get; }
        public UDouble SurfaceGravity
            => WorldFunctions.SurfaceGravity(mass: LocationCounters.GetCount<AllResAmounts>().Mass(), radius: Radius);
        /// <summary>
        /// This is current temperature to be used until the new value is calculated.
        /// Don't calculate temperature on the fly each time, as that would lead to temperature variations during the frame.
        /// </summary>
        public Temperature Temperature { get; set; }

        public readonly ResPile consistsOfResPile;

        public NodeState(WorldCamera mapInfoCamera, FullValidCosmicBodyInfo cosmicBodyInfo, SomeResAmounts<RawMaterial> composition, ResPile resSource)
            : this
            (
                name: cosmicBodyInfo.Name,
                position: CurWorldManager.ScreenPosToWorldPos
                (
                    screenPos: mapInfoCamera.WorldPosToScreenPos(worldPos: cosmicBodyInfo.Position)
                ),
                composition: composition,
                //mainResAmount: ResAmountFromApproxRadius
                //(
                //    rawMat: composition,
                //    approxRadius: CurWorldManager.ScreenLengthToWorldLength
                //    (
                //        screenLength: mapInfoCamera.WorldLengthToScreenLength(worldLength: cosmicBodyInfo.radius)
                //    )
                //),
                resSource: resSource,
                maxBatchDemResStored: 2
            )
        { }

        public NodeState(string name, MyVector2 position, SomeResAmounts<RawMaterial> composition, ResPile resSource, ulong maxBatchDemResStored)
        {
#warning display the name
            LocationCounters = LocationCounters.CreateEmpty();
            ThermalBody = ThermalBody.CreateEmpty(locationCounters: LocationCounters);
            NodeID = NodeID.Create();
            Position = position;
            Composition = composition;
            consistsOfResPile = ResPile.CreateEmpty(thermalBody: ThermalBody);
            EnlargeFrom(source: resSource, amount: composition);
            
            StoredResPile = ResPile.CreateEmpty(thermalBody: ThermalBody);
            RadiantEnergyPile = EnergyPile<RadiantEnergy>.CreateEmpty(locationCounters: LocationCounters);
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            MaxBatchDemResStored = maxBatchDemResStored;
            waitingResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(thermalBody: ThermalBody);
            WaitingPeople = RealPeople.CreateEmpty
            (
                thermalBody: ThermalBody,
                energyDistributor: CurWorldManager.EnergyDistributor,
                electricalEnergySourceNodeID: NodeID,
                closestNodeID: NodeID,
                isInActivityCenter: false
            );
            TooManyResStored = false;
        }

        public void RecalculateValues()
        {
            Composition = consistsOfResPile.Amount.Filter<RawMaterial>();
            Area = Composition.Area();
            Radius = MyMathHelper.Sqrt(value: Area.valueInMetSq / MyMathHelper.pi);
            SurfaceLength = 2 * MyMathHelper.pi * Radius;
        }

        public Result<ResPile, TextErrors> Mine(AreaDouble targetArea, ResAllocator rawMatsMixAllocator)
        {
            Debug.Assert(Composition == consistsOfResPile.Amount.Filter<RawMaterial>());
            AreaInt targetAreaInt = targetArea.RoundDown();
            (AreaInt finalMaxArea, bool minedOut) = (Area <= CurWorldConfig.minPlanetArea + targetAreaInt) switch
            {
                true => (finalMaxArea: Area - CurWorldConfig.minPlanetArea, minedOut: true),
                false => (finalMaxArea: targetAreaInt, minedOut: false),
            };
            var rawMatsMixToMine = rawMatsMixAllocator.TakeAtMostFrom
            (
                source: Composition,
                maxArea: finalMaxArea
            );
            if (rawMatsMixToMine.IsEmpty && minedOut)
                return new(errors: new(UIAlgorithms.CosmicBodyIsMinedOut));

            ResPile result = ResPile.CreateEmpty(thermalBody: ThermalBody);
            result.TransferFrom
            (
                source: consistsOfResPile,
                amount: rawMatsMixToMine.ToAll()
            );
            RecalculateValues();
            return new(ok: result);
        }

        public void EnlargeFrom(ResPile source, SomeResAmounts<RawMaterial> amount)
        {
            consistsOfResPile.TransferFrom(source: source, amount: amount.ToAll());
            RecalculateValues();
        }
    }
}
