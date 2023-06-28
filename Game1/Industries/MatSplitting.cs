using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class MatSplitting : IIndustry
    {
        [Serializable]
        public sealed class GeneralParams : IBuildingGeneralParams
        {
            public string Name { get; }
            public EfficientReadOnlyDictionary<IMaterialPurpose, Propor> BuildingComponentMaterialPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly GeneralProdAndMatAmounts buildingCostPropors;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralParams(string name, EnergyPriority energyPriority, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, color: ActiveUIManager.colorConfig.materialSplittingBuildingColor);
                Name = name;
                buildingCostPropors = new GeneralProdAndMatAmounts(ingredProdToAmounts: buildingComponentPropors, ingredMatPurposeToUsefulAreas: new());
                if (buildingCostPropors.materialPropors[IMaterialPurpose.roofSurface].IsEmpty)
                    throw new ArgumentException();
                BuildingComponentMaterialPropors = buildingCostPropors.materialPropors;

                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public Result<IBuildingConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
                => ResAndIndustryAlgos.BuildingComponentsToAmountPUBA
                (
                    buildingComponentPropors: buildingComponentPropors,
                    buildingMatChoices: neededBuildingMatChoices
                ).Select<IBuildingConcreteParams>
                (
                    buildingComponentsToAmountPUBA => new ConcreteParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        buildingImage: buildingImageParams.CreateImage(nodeShapeParams: nodeState),
                        buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                        buildingMatChoices: neededBuildingMatChoices,
                        surfaceMaterial: neededBuildingMatChoices[IMaterialPurpose.roofSurface]
                    )
                );
        }

        [Serializable]
        public readonly struct ConcreteParams : IBuildingConcreteParams
        {
            public readonly string name;
            public readonly IIndustryFacingNodeState nodeState;
            public readonly DiskBuildingImage buildingImage;
            public readonly Material surfaceMaterial;
            public readonly EnergyPriority energyPriority;

            /// <summary>
            /// Things depend on this rather than on building components target area as can say that if planet underneath building shrinks,
            /// building gets not enough space to operate at maximum efficiency
            /// </summary>
            private AreaDouble CurBuildingArea
                => buildingImage.Area;
            private readonly GeneralParams generalParams;
            private readonly EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA;
            private readonly MaterialChoices buildingMatChoices;
            private readonly SomeResAmounts<IResource> startingBuildingCost;

            public ConcreteParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, DiskBuildingImage buildingImage,
                EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA,
                MaterialChoices buildingMatChoices, Material surfaceMaterial)
            {
                name = generalParams.Name;
                this.nodeState = nodeState;
                this.buildingImage = buildingImage;
                this.surfaceMaterial = surfaceMaterial;
                energyPriority = generalParams.energyPriority;

                this.generalParams = generalParams;
                this.buildingComponentsToAmountPUBA = buildingComponentsToAmountPUBA;
                this.buildingMatChoices = buildingMatChoices;

                startingBuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);
            }

            public AreaDouble AreaToSplit()
                => ResAndIndustryAlgos.AreaInProduction(buildingArea: CurBuildingArea);

            /// <param Name="splittingMass">Mass of materials curretly being split</param>
            public CurProdStats CurSplittingStats(Mass splittingMass)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.buildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature,
                    buildingArea: CurBuildingArea,
                    productionMass: splittingMass
                );

            public void RemoveUnneededBuildingComponents(ResPile buildingResPile)
                => ResAndIndustryHelpers.RemoveUnneededBuildingComponents(nodeState: nodeState, buildingResPile: buildingResPile, buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);

            SomeResAmounts<IResource> IBuildingConcreteParams.BuildingCost
                => startingBuildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IBuildingConcreteParams.CreateIndustry(ResPile buildingResPile)
                => new MatSplitting(parameters: this, buildingResPile: buildingResPile);
        }

        [Serializable]
        private sealed class State
        {
            public static (Result<State, TextErrors> state, HistoricCorrector<double> splitAreaHistoricCorrector) Create(ConcreteParams parameters, ResPile buildingResPile,
                HistoricCorrector<double> splitAreaHistoricCorrector, RawMatsMixAllocator splitResAllocator)
            {
                var splitAreaCorrectorWithTarget = splitAreaHistoricCorrector.WithTarget(target: parameters.AreaToSplit().valueInMetSq);

                // Since will never split more raw materials mix than requested, suggestion will never be smaller than parameters.AreaToSplit(), thus will always be >= 0.
                var maxSplittingArea = AreaInt.CreateFromMetSq(valueInMetSq: (ulong)splitAreaCorrectorWithTarget.suggestion);
                var rawMatsMixToSplit = splitResAllocator.TakeAtMostFrom
                (
                    source: parameters.nodeState.StoredResPile.Amount.rawMatsMix,
                    maxArea: maxSplittingArea
                );
                if (rawMatsMixToSplit.IsEmpty)
                    return (state: new(errors: new(UIAlgorithms.NoRawMaterialMixToSplit)), splitAreaHistoricCorrector: new());
                ResPile splittingRes = ResPile.CreateEmpty(thermalBody: parameters.nodeState.ThermalBody);
                splittingRes.TransferFrom
                (
                    source: parameters.nodeState.StoredResPile,
                    amount: rawMatsMixToSplit
                );
                return
                (
                    state: new(ok: new State(parameters: parameters, buildingResPile: buildingResPile, splittingRes: splittingRes, maxSplittingArea: maxSplittingArea)),
                    splitAreaHistoricCorrector: parameters.nodeState.StoredResPile.IsEmpty ? new() : splitAreaCorrectorWithTarget.WithValue(value: splittingRes.Amount.rawMatsMix.Area().valueInMetSq)
                );
            }

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool IsDone
                => donePropor.IsFull;

            private readonly ConcreteParams parameters;
            private readonly ResPile buildingResPile, splittingRes;
            private readonly ResRecipe recipe;
            /// <summary>
            /// Mass in process of splitting
            /// </summary>
            private readonly Mass splittingMass;
            /// <summary>
            /// Area in process of splitting
            /// </summary>
            private readonly AreaInt splittingArea;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private readonly Propor proporUtilized;

            private CurProdStats curSplittingStats;
            private Propor donePropor, workingPropor;

            private State(ConcreteParams parameters, ResPile buildingResPile, ResPile splittingRes, AreaInt maxSplittingArea)
            {
                this.parameters = parameters;
                this.buildingResPile = buildingResPile;
                this.splittingRes = splittingRes;
                recipe = splittingRes.Amount.rawMatsMix.SplittingRecipe();
                splittingMass = splittingRes.Amount.Mass();
                splittingArea = splittingRes.Amount.RawMatComposition().Area();
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: parameters.nodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                proporUtilized = Propor.Create(part: splittingArea.valueInMetSq, whole: maxSplittingArea.valueInMetSq)!.Value;
                donePropor = Propor.empty;
            }

            public void FrameStart()
            {
                curSplittingStats = parameters.CurSplittingStats(splittingMass: splittingMass);
#warning if production will be done this frame, could request just enough energy to complete it rather than the usual amount
                ReqEnergy = ElectricalEnergy.CreateFromJoules
                (
                    valueInJ: reqEnergyHistoricRounder.Round
                    (
                        value: (decimal)curSplittingStats.ReqWatts * (decimal)CurWorldManager.Elapsed.TotalSeconds,
                        curTime: CurWorldManager.CurTime
                    )
                );
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingPropor = proporUtilized * Propor.Create(part: electricalEnergy.ValueInJ, whole: ReqEnergy.ValueInJ)!.Value;
            }

            /// <summary>
            /// This will not remove no longer needed building components until material splitting cycle is done since fix current max splitting area
            /// and some other splitting stats at the start of material splitting cycle
            /// </summary>
            public void Update()
            {
                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                AreaDouble areaSplit = AreaDouble.CreateFromMetSq(valueInMetSq: workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curSplittingStats.ProducedAreaPerSec);
                donePropor = Propor.CreateByClamp((UDouble)donePropor + areaSplit.valueInMetSq / splittingArea.valueInMetSq);
                if (donePropor.IsFull)
                {
                    parameters.nodeState.StoredResPile.TransformFrom(source: splittingRes, recipe: recipe);
                    parameters.RemoveUnneededBuildingComponents(buildingResPile: buildingResPile);
                }
            }

            public void Delete()
            {
                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                parameters.nodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
                parameters.nodeState.EnlargeFrom(source: splittingRes, amount: splittingRes.Amount.RawMatComposition());
            }
        }

        public string Name
            => parameters.name;

        public Material SurfaceMaterial
            => parameters.surfaceMaterial;

        public IHUDElement UIElement
            => throw new NotImplementedException();

        public IEvent<IDeletedListener> Deleted
            => deleted;

        public IBuildingImage BuildingImage
            => parameters.buildingImage;

        private readonly ConcreteParams parameters;
        private readonly ResPile buildingResPile;
        private readonly Event<IDeletedListener> deleted;
        private Result<State, TextErrors> stateOrReasonForNotStartingRawMatSplitting;
        private HistoricCorrector<double> splitAreaHistoricCorrector;
        private readonly RawMatsMixAllocator splitResAllocator;

        private MatSplitting(ConcreteParams parameters, ResPile buildingResPile)
        {
            this.parameters = parameters;
            this.buildingResPile = buildingResPile;
            deleted = new();
            stateOrReasonForNotStartingRawMatSplitting = new(errors: new("Not yet initialized"));
            splitAreaHistoricCorrector = new();
            splitResAllocator = new RawMatsMixAllocator();
        }

        public string GetInfo()
        {
            throw new NotImplementedException();
        }

        public SomeResAmounts<IResource> TargetStoredResAmounts()
            => ;

        public void FrameStartNoProduction(string error)
        {
            throw new NotImplementedException();
        }

        public void FrameStart()
        {
            (stateOrReasonForNotStartingRawMatSplitting, splitAreaHistoricCorrector) = stateOrReasonForNotStartingRawMatSplitting.SwitchExpression
            (
                ok: state => state.IsDone ? CreateState() : (new(ok: state), splitAreaHistoricCorrector),
                error: _ => CreateState()
            );
            stateOrReasonForNotStartingRawMatSplitting.PerformAction
            (
                action: state => state.FrameStart()
            );

            return;

            (Result<State, TextErrors> state, HistoricCorrector<double> splitAreaHistoricCorrector) CreateState()
                => State.Create
                (
                    parameters: parameters,
                    buildingResPile: buildingResPile,
                    splitAreaHistoricCorrector: splitAreaHistoricCorrector,
                    splitResAllocator: splitResAllocator
                );
        }

        public IIndustry? Update()
        {
            stateOrReasonForNotStartingRawMatSplitting.PerformAction(action: state => state.Update());
            return this;
        }

        private void Delete()
        {
            stateOrReasonForNotStartingRawMatSplitting.PerformAction(action: state => state.Delete());
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        EnergyPriority IEnergyConsumer.EnergyPriority
            => parameters.energyPriority;

        NodeID IEnergyConsumer.NodeID
            => parameters.nodeState.NodeID;

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => stateOrReasonForNotStartingRawMatSplitting.SwitchExpression
            (
                ok: state => state.ReqEnergy,
                error: _ => ElectricalEnergy.zero
            );

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            => stateOrReasonForNotStartingRawMatSplitting.SwitchStatement
            (
                ok: state => state.ConsumeElectricalEnergy(source: source, electricalEnergy: electricalEnergy),
                error: _ => Debug.Assert(electricalEnergy.IsZero)
            );
    }
}
