using Game1.Collections;
using Game1.Delegates;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Mining : IIndustry, Disk.IParams
    {
        [Serializable]
        public readonly struct GeneralParams : IConstructedIndustryGeneralParams
        {
            public string Name { get; }
            public Color Color { get; }
            public GeneralProdAndMatAmounts BuildingCostPropors { get; }

            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralParams(string name, Color color, EnergyPriority energyPriority, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                Name = name;
                Color = color;
                BuildingCostPropors = new GeneralProdAndMatAmounts(ingredProdToAmounts: buildingComponentPropors, ingredMatPurposeToTargetAreas: new());
                this.energyPriority = energyPriority;
                this.buildingComponentPropors = buildingComponentPropors;
                if (BuildingCostPropors.materialPropors[IMaterialPurpose.roofSurface] == Propor.empty)
                    throw new ArgumentException();
            }

            //public Result<ConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices)
            //{
            //    // ALSO filter the material choices so that ConcreteParams knows only about the useful ones
            //    throw new NotImplementedException();
            //}

            public IIndustry CreateIndustry(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices, ResPile buildingResPile)
                => new ConcreteParams
                (
                    nodeState: nodeState,
                    buildingResPile: buildingResPile,
                    generalParams: this,
                    buildingComponentsToAmountPUS: ResAndIndustryAlgos.BuildingComponentsToAmountPUSOrThrow
                    (
                        buildingComponentPropors: buildingComponentPropors,
                        buildingMatChoices: buildingMatChoices
                    ),
                    buildingMatChoices: buildingMatChoices,
                    surfaceMaterial: buildingMatChoices[IMaterialPurpose.roofSurface]
                ).CreateIndustry();
        }

        [Serializable]
        public readonly struct ConcreteParams
        {
            public readonly Color color;
            public readonly IIndustryFacingNodeState nodeState;
            public readonly Material surfaceMaterial;
            public readonly EnergyPriority energyPriority;
            public readonly ResPile buildingResPile;

            private readonly GeneralParams generalParams;
            private readonly EfficientReadOnlyCollection<(Product prod, UDouble amountPUS)> buildingComponentsToAmountPUS;
            private readonly MaterialChoices buildingMatChoices;

            public ConcreteParams(IIndustryFacingNodeState nodeState, ResPile buildingResPile, GeneralParams generalParams, EfficientReadOnlyCollection<(Product prod, UDouble amountPUS)> buildingComponentsToAmountPUS,
                MaterialChoices buildingMatChoices, Material surfaceMaterial)
            {
                color = generalParams.Color;
                this.nodeState = nodeState;
                this.surfaceMaterial = surfaceMaterial;
                energyPriority = generalParams.energyPriority;
                this.buildingResPile = buildingResPile;

                this.generalParams = generalParams;
                this.buildingComponentsToAmountPUS = buildingComponentsToAmountPUS;
                this.buildingMatChoices = buildingMatChoices;
            }

            public UDouble AreaToMine()
                => ResAndIndustryAlgos.AreaInProduction(surfaceLength: nodeState.SurfaceLength);

            /// <param Name="miningMass">Mass of materials curretly being mined</param>
            public CurProdStats CurMiningStats(Mass miningMass)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.BuildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature,
                    surfaceLength: nodeState.SurfaceLength,
                    productionMass: miningMass
                );

            ///// <param Name="miningMass">Mass of materials curretly being mined</param>
            //public CurMiningStats CurMiningStats(Mass miningMass)
            //{
            //    UDouble relevantMassPUS = ResAndIndustryAlgos.RelevantMassPUS
            //    (
            //        buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        productionMassPUS: miningMass.valueInKg / nodeState.SurfaceLength
            //    );

            //    UDouble maxMechThroughputPUS = ResAndIndustryAlgos.MaxMechThroughputPUS
            //    (
            //        buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        gravity: nodeState.SurfaceGravity,
            //        temperature: nodeState.Temperature,
            //        relevantMassPUS: relevantMassPUS
            //    );

            //    UDouble maxElectricalPowerPUS = ResAndIndustryAlgos.MaxElectricalPowerPUS
            //    (
            //        buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        temperature: nodeState.Temperature
            //    );

            //    UDouble electricalEnergyPerUnitArea = ResAndIndustryAlgos.ElectricalEnergyPerUnitAreaPhys
            //    (
            //        buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        gravity: nodeState.SurfaceGravity,
            //        temperature: nodeState.Temperature,
            //        relevantMassPUS: relevantMassPUS
            //    );

            //    UDouble
            //        reqWattsPUS = MyMathHelper.Min(maxElectricalPowerPUS, maxMechThroughputPUS * electricalEnergyPerUnitArea),
            //        reqWatts = reqWattsPUS * nodeState.SurfaceLength,
            //        minedAreaPerSec = reqWatts / electricalEnergyPerUnitArea;

            //    return new
            //    (
            //        ReqWatts: reqWatts,
            //        MinedAreaPerSec: minedAreaPerSec
            //    );
            //}

            // This is separate from CurMiningStats as it's supposed to be called after the mining for that frame was done
            public AllResAmounts CurNeededBuildingComponents()
            {
                // This is needed here so that the compiler doesn't complain
                // "Lambda expressions inside structs cannot access members of 'this'" 
                var nodeStateCopy = nodeState;
                return AllResAmounts.CreateFromNoMix
                (
                    resAmounts: new
                    (
                        buildingComponentsToAmountPUS.Select
                        (
                            prodAndAmountPUS => new ResAmount<IResource>
                            (
                                prodAndAmountPUS.prod,
                                MyMathHelper.Ceiling(prodAndAmountPUS.amountPUS * nodeStateCopy.SurfaceLength)
                            )
                        )
                    )
                );
            }

            public Mining CreateIndustry()
                => new(parameters: this);
        }

        //[Serializable]
        //public readonly record struct CurMiningStats(UDouble ReqWatts, UDouble MinedAreaPerSec);

        [Serializable]
        private sealed class State
        {
            public static Result<State, TextErrors> Create(ConcreteParams parameters)
                => parameters.nodeState.Mine(targetArea: parameters.AreaToMine()).Select
                (
                    miningRes => new State(parameters: parameters, miningRes: miningRes)
                );

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool IsDone
                => donePropor >= 1;

            private readonly ConcreteParams parameters;
            private readonly ResPile miningRes;
            /// <summary>
            /// Mass in process of mining
            /// </summary>
            private readonly Mass miningMass;
            /// <summary>
            /// Area in process of mining
            /// </summary>
            private readonly Area miningArea;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;

            private CurProdStats curMiningStats;
            private UDouble donePropor;
            private Propor workingPropor;

            private State(ConcreteParams parameters, ResPile miningRes)
            {
                this.parameters = parameters;
                this.miningRes = miningRes;
                miningMass = miningRes.Amount.Mass();
                miningArea = miningRes.Amount.RawMatComposition().Area();
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: parameters.nodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                donePropor = 0;
            }

            public void FrameStart()
            {
                curMiningStats = parameters.CurMiningStats(miningMass: miningMass);
                ReqEnergy = ElectricalEnergy.CreateFromJoules
                (
                    valueInJ: reqEnergyHistoricRounder.Round
                    (
                        value: (decimal)curMiningStats.ReqWatts * (decimal)CurWorldManager.Elapsed.TotalSeconds,
                        curTime: CurWorldManager.CurTime
                    )
                );
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingPropor = Propor.Create(part: electricalEnergy.ValueInJ, whole: ReqEnergy.ValueInJ)!.Value;
            }

            public void Update()
            {
                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                UDouble areaMined = workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curMiningStats.ProducedAreaPerSec;
                donePropor += areaMined / miningArea.valueInMetSq;
                if (donePropor >= 1)
                {
                    parameters.nodeState.StoredResPile.TransferAllFrom(source: miningRes);
                    donePropor = 1;
                }
                // Remove not needed building components
                parameters.nodeState.StoredResPile.TransferFrom
                (
                    source: parameters.buildingResPile,
                    amount: parameters.buildingResPile.Amount - parameters.CurNeededBuildingComponents()
                );
            }

            public void Delete()
            {
                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                parameters.nodeState.StoredResPile.TransferAllFrom(source: parameters.buildingResPile);
                parameters.nodeState.EnlargeFrom(source: miningRes, amount: miningRes.Amount.RawMatComposition());
            }
        }

        public ILightBlockingObject? LightBlockingObject
            => lightBlockingDisk;

        public Material SurfaceMaterial
            => parameters.surfaceMaterial;

        public IHUDElement UIElement
            => throw new NotImplementedException();

        public IEvent<IDeletedListener> Deleted
            => deleted;

        private readonly ConcreteParams parameters;
        private Result<State, TextErrors> stateOrReasonForNotStartingMining;
        private readonly Event<IDeletedListener> deleted;
        private readonly LightBlockingDisk lightBlockingDisk;

        private Mining(ConcreteParams parameters)
        {
            this.parameters = parameters;
            stateOrReasonForNotStartingMining = new(errors: new("Not yet initialized"));
            deleted = new();
            lightBlockingDisk = new(parameters: this);
        }

        public string GetInfo()
        {
            throw new NotImplementedException();
        }

        public SomeResAmounts<IResource> TargetStoredResAmounts()
            => SomeResAmounts<IResource>.empty;
        
        public void FrameStartNoProduction(string error)
        {
            throw new NotImplementedException();
        }

        public void FrameStart()
        {
            stateOrReasonForNotStartingMining = stateOrReasonForNotStartingMining.SwitchExpression
            (
                ok: state => state.IsDone ? State.Create(parameters: parameters) : new(ok: state),
                error: _ => State.Create(parameters: parameters)
            );
            stateOrReasonForNotStartingMining.PerformAction
            (
                action: state => state.FrameStart()
            );
        }

        public IIndustry? Update()
        {
            stateOrReasonForNotStartingMining.PerformAction(action: state => state.Update());
            return this;
        }

        private void Delete()
        {
            stateOrReasonForNotStartingMining.PerformAction(action: state => state.Delete());
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        public void Draw(Color otherColor, Propor otherColorPropor)
            => lightBlockingDisk.Draw(baseColor: parameters.color, otherColor: otherColor, otherColorPropor: otherColorPropor);

        EnergyPriority IEnergyConsumer.EnergyPriority
            => parameters.energyPriority;

        NodeID IEnergyConsumer.NodeID
            => parameters.nodeState.NodeID;

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => stateOrReasonForNotStartingMining.SwitchExpression
            (
                ok: state => state.ReqEnergy,
                error: _ => ElectricalEnergy.zero
            );

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            => stateOrReasonForNotStartingMining.SwitchStatement
            (
                ok: state => state.ConsumeElectricalEnergy(source: source, electricalEnergy: electricalEnergy),
                error: _ => Debug.Assert(electricalEnergy.IsZero)
            );

        MyVector2 Disk.IParams.Center
            => parameters.nodeState.Position;

        UDouble Disk.IParams.Radius
            => parameters.nodeState.Radius + ResAndIndustryAlgos.BuildingHeight;
    }
}
