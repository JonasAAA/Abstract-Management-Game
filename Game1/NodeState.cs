﻿using Game1.ContentHelpers;
using Game1.Industries;
using Game1.Inhabitants;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class NodeState : IIndustryFacingNodeState
    {
        public static RawMatAmounts CalculateComposition(RawMatAmounts rawMatRatios, Length approxRadius)
            // This whole Max thing is so that the planet is not smaller than the minimal allowed area
            => rawMatRatios * MyMathHelper.Max
            (
                MyMathHelper.DivideThenTakeCeiling
                (
                    dividend: CurWorldConfig.minPlanetArea.valueInMetSq,
                    divisor: rawMatRatios.Area().valueInMetSq
                ),
                Convert.ToUInt64(DiskAlgos.Area(radius: approxRadius).valueInMetSq / rawMatRatios.Area().valueInMetSq)
            );

        public NodeID NodeID { get; }
        public Mass PlanetMass
            => consistsOfResPile.Amount.Mass();
        public AreaInt Area { get; private set; }
        public Length Radius { get; private set; }
        public Length SurfaceLength { get; private set; }
        public MyVector2 Position { get; }
        public EnergyPile<RadiantEnergy> RadiantEnergyPile { get; }
        public readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
        public RealPeople WaitingPeople { get; }
        public RawMatAmounts Composition { get; private set; }
        // TODO: could include linkEndPoints Mass in the Counter<Mass> in this NodeState
        public LocationCounters LocationCounters { get; }
        public ThermalBody ThermalBody { get; }
        public SurfaceGravity SurfaceGravity { get; private set; }
        /// <summary>
        /// This is current temperature to be used until the new value is calculated.
        /// Don't calculate temperature on the fly each time, as that would lead to temperature variations during the frame.
        /// </summary>
        public Temperature Temperature { get; private set; }
        public (EnergyPile<RadiantEnergy> lightPile, UDouble lightPerSec, NodeID targetCosmicBody)? LaserToShine { get; set; }

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
            if (Area <= CurWorldConfig.minPlanetArea)
                throw new ArgumentException();
        }

        public void UpdateTemperature()
            => Temperature = ResAndIndustryAlgos.CalculateTemperature(heatEnergy: ThermalBody.HeatEnergy, heatCapacity: ThermalBody.HeatCapacity);

        public void RecalculateValues()
        {
            Composition = consistsOfResPile.Amount.Filter<RawMaterial>();
            Area = Composition.Area();
            Radius = DiskAlgos.RadiusFromArea(area: Area.ToDouble());
            SurfaceLength = DiskAlgos.Length(radius: Radius);
            //var allResComposition = LocationCounters.GetCount<AllResAmounts>().RawMatComposition();
            //SurfaceGravity = WorldFunctions.SurfaceGravity(mass: allResComposition.Mass(), resArea: allResComposition.Area());
#warning Remove this simplification
            SurfaceGravity = WorldFunctions.SurfaceGravity(mass: Composition.Mass(), resArea: Composition.Area());
        }

        public Result<ResPile, TextErrors> Mine(AreaInt targetArea)
        {
            Debug.Assert(Composition == consistsOfResPile.Amount.Filter<RawMaterial>());
            (AreaInt finalMaxArea, bool minedOut) = (Area <= CurWorldConfig.minPlanetArea + targetArea) switch
            {
                true => (finalMaxArea: Area - CurWorldConfig.minPlanetArea, minedOut: true),
                false => (finalMaxArea: targetArea, minedOut: false),
            };
            RawMatAmounts rawMatsAmountsToMine = new
            (
                resAmounts: Algorithms.Split
                (
                    weights: Composition.ToEfficientReadOnlyDict(),
                    totalAmount: finalMaxArea.valueInMetSq / ResAndIndustryAlgos.rawMaterialArea.valueInMetSq
                ).Select
                (
                    rawMatAndAmount => new ResAmount<RawMaterial>
                    (
                        res: rawMatAndAmount.owner,
                        amount: rawMatAndAmount.amount
                    )
                )
            );
            Debug.Assert(rawMatsAmountsToMine.Area() <= targetArea);
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
