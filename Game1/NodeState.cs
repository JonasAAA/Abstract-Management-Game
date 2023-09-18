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
        public static RawMatAmounts CalculateComposition(RawMatAmounts rawMatRatios, UDouble approxRadius)
            => rawMatRatios * Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / rawMatRatios.Area().valueInMetSq);

        public NodeID NodeID { get; }
        public Mass PlanetMass
            => consistsOfResPile.Amount.Mass();
        public AreaInt Area { get; private set; }
        public UDouble Radius { get; private set; }
        public UDouble SurfaceLength { get; private set; }
        public MyVector2 Position { get; }
        public EnergyPile<RadiantEnergy> RadiantEnergyPile { get; }
        public readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
        public RealPeople WaitingPeople { get; }
        public RawMatAmounts Composition { get; private set; }
        // TODO: could include linkEndPoints Mass in the Counter<Mass> in this NodeState
        public LocationCounters LocationCounters { get; }
        public ThermalBody ThermalBody { get; }
        public UDouble SurfaceGravity { get; private set; }
        /// <summary>
        /// This is current temperature to be used until the new value is calculated.
        /// Don't calculate temperature on the fly each time, as that would lead to temperature variations during the frame.
        /// </summary>
        public Temperature Temperature { get; private set; }

        public readonly ResPile consistsOfResPile;

        public NodeState(WorldCamera mapInfoCamera, FullValidCosmicBodyInfo cosmicBodyInfo, RawMatAmounts rawMatRatios, ResPile resSource)
            : this
            (
                name: cosmicBodyInfo.Name,
                position: CurWorldManager.ScreenPosToWorldPos
                (
                    screenPos: mapInfoCamera.WorldPosToScreenPos(worldPos: cosmicBodyInfo.Position)
                ),
                composition: CalculateComposition
                (
                    rawMatRatios: rawMatRatios,
                    approxRadius: CurWorldManager.ScreenLengthToWorldLength
                    (
                        screenLength: mapInfoCamera.WorldLengthToScreenLength(worldLength: cosmicBodyInfo.Radius)
                    )
                ),
                resSource: resSource
            )
        { }

        public NodeState(string name, MyVector2 position, RawMatAmounts composition, ResPile resSource)
        {
#warning display the name
            LocationCounters = LocationCounters.CreateEmpty();
            ThermalBody = ThermalBody.CreateEmpty(locationCounters: LocationCounters);
            NodeID = NodeID.Create();
            Position = position;
            Composition = composition;
            consistsOfResPile = ResPile.CreateEmpty(thermalBody: ThermalBody);
            EnlargeFrom(source: resSource, amount: composition);
            
            RadiantEnergyPile = EnergyPile<RadiantEnergy>.CreateEmpty(locationCounters: LocationCounters);
            waitingResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(thermalBody: ThermalBody);
            WaitingPeople = RealPeople.CreateEmpty
            (
                thermalBody: ThermalBody,
                energyDistributor: CurWorldManager.EnergyDistributor,
                electricalEnergySourceNodeID: NodeID,
                closestNodeID: NodeID,
                isInActivityCenter: false
            );
            UpdateTemperature();
        }

        public void UpdateTemperature()
            => Temperature = ResAndIndustryAlgos.Temperature(heatEnergy: ThermalBody.HeatEnergy, heatCapacity: ThermalBody.HeatCapacity);

        public void RecalculateValues()
        {
            Composition = consistsOfResPile.Amount.Filter<RawMaterial>();
            Area = Composition.Area();
            Radius = MyMathHelper.Sqrt(value: Area.valueInMetSq / MyMathHelper.pi);
            SurfaceLength = 2 * MyMathHelper.pi * Radius;
            var allResComposition = LocationCounters.GetCount<AllResAmounts>().RawMatComposition();
            SurfaceGravity = WorldFunctions.SurfaceGravity(mass: allResComposition.Mass(), resArea: allResComposition.Area());
        }

        public Result<ResPile, TextErrors> Mine(AreaDouble targetArea, RawMatAllocator rawMatAllocator)
        {
            Debug.Assert(Composition == consistsOfResPile.Amount.Filter<RawMaterial>());
            AreaInt targetAreaInt = targetArea.RoundDown();
            (AreaInt finalMaxArea, bool minedOut) = (Area <= CurWorldConfig.minPlanetArea + targetAreaInt) switch
            {
                true => (finalMaxArea: Area - CurWorldConfig.minPlanetArea, minedOut: true),
                false => (finalMaxArea: targetAreaInt, minedOut: false),
            };
            var rawMatsAmountsToMine = rawMatAllocator.TakeAtMostFrom
            (
                source: Composition,
                maxArea: finalMaxArea
            );
            if (rawMatsAmountsToMine.IsEmpty && minedOut)
                return new(errors: new(UIAlgorithms.CosmicBodyIsMinedOut));

            var result = ResPile.CreateEmpty(thermalBody: ThermalBody);
            result.TransferFrom
            (
                source: consistsOfResPile,
                amount: rawMatsAmountsToMine.ToAll()
            );
            RecalculateValues();
            return new(ok: result);
        }

        public void EnlargeFrom(ResPile source, RawMatAmounts amount)
        {
            consistsOfResPile.TransferFrom(source: source, amount: amount.ToAll());
            RecalculateValues();
        }

        public void TransportRes(ResPile source, NodeID destination, AllResAmounts amount)
            => waitingResAmountsPackets.TransferFrom(source: source, destination: destination, amount: amount);
    }
}
