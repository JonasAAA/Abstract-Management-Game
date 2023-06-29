using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    /// <summary>
    /// Responds properly to planet shrinking, but NOT to planet widening
    /// </summary>
    [Serializable]
    public sealed class Mining : IIndustry
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
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, color: ActiveUIManager.colorConfig.miningBuildingColor);
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
            private readonly AllResAmounts startingBuildingCost;

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

            public AreaDouble AreaToMine()
                => ResAndIndustryAlgos.AreaInProduction(buildingArea: CurBuildingArea);

            /// <param Name="splittingMass">Mass of materials curretly being mined</param>
            public CurProdStats CurMiningStats(Mass miningMass)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.buildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature,
                    buildingArea: CurBuildingArea,
                    productionMass: miningMass
                );

            public void RemoveUnneededBuildingComponents(ResPile buildingResPile)
                => ResAndIndustryHelpers.RemoveUnneededBuildingComponents(nodeState: nodeState, buildingResPile: buildingResPile, buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);

            AllResAmounts IBuildingConcreteParams.BuildingCost
                => startingBuildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IBuildingConcreteParams.CreateIndustry(ResPile buildingResPile)
                => new Mining(parameters: this, buildingResPile: buildingResPile);
        }

        [Serializable]
        private sealed class State
        {
            public static (Result<State, TextErrors> state, HistoricCorrector<double> minedAreaHistoricCorrector) Create(ConcreteParams parameters, ResPile buildingResPile, HistoricCorrector<double> minedAreaHistoricCorrector, ResAllocator minedResAllocator)
            {
                var minedAreaCorrectorWithTarget = minedAreaHistoricCorrector.WithTarget(target: parameters.AreaToMine().valueInMetSq);

                // Since will never mine more than requested, suggestion will never be smaller than parameters.AreaToSplit(), thus will always be >= 0.
                var maxAreaToMine = (UDouble)minedAreaCorrectorWithTarget.suggestion;
                return parameters.nodeState.Mine
                (
                    targetArea: AreaDouble.CreateFromMetSq(valueInMetSq: maxAreaToMine),
                    rawMatsMixAllocator: minedResAllocator
                ).SwitchExpression<(Result<State, TextErrors> state, HistoricCorrector<double> minedAreaHistoricCorrector)>
                (
                    ok: miningRes =>
                    {
                        AreaInt miningArea = miningRes.Amount.Filter<RawMaterial>().Area();
                        return
                        (
                            state: new(ok: new State(parameters: parameters, buildingResPile: buildingResPile, miningRes: miningRes, miningArea: miningArea)),
                            minedAreaHistoricCorrector: minedAreaCorrectorWithTarget.WithValue(value: miningArea.valueInMetSq)
                        );
                    },
                    error: errors =>
                    (
                        state: new(errors: errors),
                        minedAreaHistoricCorrector: new()
                    )
                );
            }

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool IsDone
                => donePropor.IsFull;

            private readonly ConcreteParams parameters;
            private readonly ResPile buildingResPile, miningRes;
            /// <summary>
            /// Mass in process of mining
            /// </summary>
            private readonly Mass miningMass;
            /// <summary>
            /// Area in process of mining
            /// </summary>
            private readonly AreaInt miningArea;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;

            private CurProdStats curMiningStats;
            private Propor donePropor, workingPropor;

            private State(ConcreteParams parameters, ResPile buildingResPile, ResPile miningRes, AreaInt miningArea)
            {
                this.parameters = parameters;
                this.buildingResPile = buildingResPile;
                this.miningRes = miningRes;
                miningMass = miningRes.Amount.Mass();
                this.miningArea = miningArea;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: parameters.nodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                donePropor = Propor.empty;
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

            /// <summary>
            /// This will not remove no longer needed building components until mining cycle is done since fix current mining volume
            /// and some other mining stats at the start of the mining cycle. 
            /// </summary>
            public void Update()
            {
                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                AreaDouble areaMined = AreaDouble.CreateFromMetSq(valueInMetSq: workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curMiningStats.ProducedAreaPerSec);
                // If mine less than could, probably shouldn't increase mining speed because of that
                // On the other hand that would be a very niche circumstance anyway - basically only when mining last resources of the planet
                donePropor = Propor.CreateByClamp(value: (UDouble)donePropor + areaMined.valueInMetSq / miningArea.valueInMetSq);
                if (donePropor.IsFull)
                {
                    parameters.nodeState.StoredResPile.TransferAllFrom(source: miningRes);
                    parameters.RemoveUnneededBuildingComponents(buildingResPile: buildingResPile);
                }
            }

            public void Delete()
            {
                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                parameters.nodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
                parameters.nodeState.EnlargeFrom(source: miningRes, amount: miningRes.Amount.Filter<RawMaterial>());
                Debug.Assert(miningRes.IsEmpty);
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
        private Result<State, TextErrors> stateOrReasonForNotStartingMining;
        private HistoricCorrector<double> minedAreaHistoricCorrector;
        private readonly ResAllocator minedResAllocator;

        private Mining(ConcreteParams parameters, ResPile buildingResPile)
        {
            this.parameters = parameters;
            this.buildingResPile = buildingResPile;
            deleted = new();
            stateOrReasonForNotStartingMining = new(errors: new("Not yet initialized"));
            minedAreaHistoricCorrector = new();
            minedResAllocator = new();
        }

        public string GetInfo()
        {
            throw new NotImplementedException();
        }

        public AllResAmounts TargetStoredResAmounts()
            => AllResAmounts.empty;

        public void FrameStartNoProduction(string error)
        {
            throw new NotImplementedException();
        }

        public void FrameStart()
        {
            (stateOrReasonForNotStartingMining, minedAreaHistoricCorrector) = stateOrReasonForNotStartingMining.SwitchExpression
            (
                ok: state => state.IsDone ? CreateState() : (new(ok: state), minedAreaHistoricCorrector),
                error: _ => CreateState()
            );
            stateOrReasonForNotStartingMining.PerformAction
            (
                action: state => state.FrameStart()
            );

            return;

            (Result<State, TextErrors> state, HistoricCorrector<double> minedAreaHistoricCorrector) CreateState()
                => State.Create
                (
                    parameters: parameters,
                    buildingResPile: buildingResPile,
                    minedAreaHistoricCorrector: minedAreaHistoricCorrector,
                    minedResAllocator: minedResAllocator
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
    }
}
