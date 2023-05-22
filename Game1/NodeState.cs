using Game1.Collections;
﻿using Game1.ContentHelpers;
using Game1.Industries;
using Game1.Inhabitants;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class NodeState : IIndustryFacingNodeState
    {
        public static ulong ResAmountFromApproxRadius(RawMaterial rawMat, UDouble approxRadius)
            => Convert.ToUInt64(MyMathHelper.pi * approxRadius * approxRadius / rawMat.Area);

        public NodeID NodeID { get; }
        public Mass PlanetMass
            => consistsOfResPile.Amount.Mass();
        public ulong Area { get; private set; }
        public UDouble Radius { get; private set; }
        public ulong ApproxSurfaceLength { get; private set; }
        public ulong MainResAmount
            => consistsOfResPile.Amount.rawMatsMix[ConsistsOf];
        public ulong MaxAvailableResAmount
            => MainResAmount - CurWorldConfig.minResAmountInPlanet;
        public MyVector2 Position { get; }
        public ulong MaxBatchDemResStored { get; }
        public ResPile StoredResPile { get; }
        public EnergyPile<RadiantEnergy> RadiantEnergyPile { get; }
        public readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
        public RealPeople WaitingPeople { get; }
        public RawMaterial ConsistsOf { get; }
        public bool TooManyResStored { get; set; }
        // TODO: could include linkEndPoints Mass in the Counter<Mass> in this NodeState
        public LocationCounters LocationCounters { get; }
        public ThermalBody ThermalBody { get; }
        public UDouble SurfaceGravity
            => WorldFunctions.SurfaceGravity(mass: LocationCounters.GetCount<AllResAmounts>().Mass(), radius: Radius);

        public readonly ResPile consistsOfResPile;

        public NodeState(WorldCamera mapInfoCamera, FullValidCosmicBodyInfo cosmicBodyInfo, RawMaterial consistsOf, ResPile resSource)
            : this
            (
                name: cosmicBodyInfo.Name,
                position: CurWorldManager.ScreenPosToWorldPos
                (
                    screenPos: mapInfoCamera.WorldPosToScreenPos(worldPos: cosmicBodyInfo.Position)
                ),
                consistsOf: consistsOf,
                mainResAmount: ResAmountFromApproxRadius
                (
                    rawMat: consistsOf,
                    approxRadius: CurWorldManager.ScreenLengthToWorldLength
                    (
                        screenLength: mapInfoCamera.WorldLengthToScreenLength(worldLength: cosmicBodyInfo.Radius)
                    )
                ),
                resSource: resSource,
                maxBatchDemResStored: 2
            )
        { }

        public NodeState(string name, MyVector2 position, RawMaterial consistsOf, ulong mainResAmount, ResPile resSource, ulong maxBatchDemResStored)
        {
#warning display the name
            LocationCounters = LocationCounters.CreateEmpty();
            ThermalBody = ThermalBody.CreateEmpty(locationCounters: LocationCounters);
            NodeID = NodeID.Create();
            Position = position;
            ConsistsOf = consistsOf;
            consistsOfResPile = ResPile.CreateEmpty(thermalBody: ThermalBody);
            EnlargeFrom(source: resSource, resAmount: mainResAmount);
            
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

        public bool CanRemove(ulong resAmount)
            => MainResAmount >= resAmount + CurWorldConfig.minResAmountInPlanet;

        public void RecalculateValues()
        {
            Area = MainResAmount * ConsistsOf.Area;
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
                amount: AllResAmounts.CreateFromOnlyMix
                (
                    rawMatsMix: new(res: ConsistsOf, amount: resAmount)
                )
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
                amount: AllResAmounts.CreateFromOnlyMix
                (
                    rawMatsMix: new(res: ConsistsOf, amount: resAmount)
                )
            ) ?? throw new ArgumentException();
            consistsOfResPile.TransferAllFrom(source: reservedResPile);
            RecalculateValues();
        }
    }
}
