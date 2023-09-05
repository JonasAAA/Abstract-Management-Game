using Game1.Collections;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    /// <summary>
    /// Responds properly to planet shrinking, but NOT to planet widening
    /// </summary>
    public static class Mining
    {

        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public string Name { get; }
            public GeneralProdAndMatAmounts BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EnergyPriority energyPriority, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, color: ActiveUIManager.colorConfig.miningBuildingColor);
                Name = name;
                BuildingCostPropors = new GeneralProdAndMatAmounts(ingredProdToAmounts: buildingComponentPropors, ingredMatPurposeToUsefulAreas: new());
                if (BuildingCostPropors.materialPropors[IMaterialPurpose.roofSurface].IsEmpty)
                    throw new ArgumentException();

                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public Result<IConcreteBuildingConstructionParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
                => ResAndIndustryAlgos.BuildingComponentsToAmountPUBA
                (
                    buildingComponentPropors: buildingComponentPropors,
                    buildingMatChoices: neededBuildingMatChoices,
                    buildingComponentsProporOfBuildingArea: CurWorldConfig.buildingComponentsProporOfBuildingArea
                ).Select<IConcreteBuildingConstructionParams>
                (
                    buildingComponentsToAmountPUBA => new ConcreteBuildingParams
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
        public readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<UnitType>, IConcreteBuildingConstructionParams
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public Material SurfaceMaterial { get; }
            public readonly DiskBuildingImage buildingImage;

            /// <summary>
            /// Things depend on this rather than on building components target area as can say that if planet underneath building shrinks,
            /// building gets not enough space to operate at maximum efficiency
            /// </summary>
            private AreaDouble CurBuildingArea
                => buildingImage.Area;

            private readonly GeneralBuildingParams generalParams;
            private readonly EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA;
            private readonly MaterialChoices buildingMatChoices;
            private readonly EfficientReadOnlyCollection<IResource> producedResources;
            private readonly AllResAmounts startingBuildingCost;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA,
                MaterialChoices buildingMatChoices, Material surfaceMaterial)
            {
                Name = generalParams.Name;
                this.NodeState = nodeState;
                this.buildingImage = buildingImage;
                this.SurfaceMaterial = surfaceMaterial;
                EnergyPriority = generalParams.energyPriority;

                this.generalParams = generalParams;
                this.buildingComponentsToAmountPUBA = buildingComponentsToAmountPUBA;
                this.buildingMatChoices = buildingMatChoices;
                startingBuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);
                producedResources = (nodeState.Composition.ToAll() + startingBuildingCost).resList;
            }

            public AreaDouble AreaToMine()
                => CurBuildingArea * CurWorldConfig.productionProporOfBuildingArea;

            /// <param Name="splittingMass">Mass of materials curretly being mined</param>
            public CurProdStats CurMiningStats(Mass miningMass)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.BuildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature,
                    buildingArea: CurBuildingArea,
                    productionMass: miningMass
                );

            public void RemoveUnneededBuildingComponents(ResPile buildingResPile, ResPile outputStorage)
            {
                var buildingComponentsToRemove = buildingResPile.Amount - ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA, CurBuildingArea);
                if (buildingComponentsToRemove.UsefulArea() >= CurWorldConfig.minUsefulBuildingComponentAreaToRemove)
                {
                    outputStorage.TransferFrom
                    (
                        source: buildingResPile,
                        amount: buildingComponentsToRemove
                    );
                }
            }
            
            AllResAmounts IConcreteBuildingConstructionParams.BuildingCost
                => startingBuildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new Industry<UnitType, ConcreteBuildingParams, PersistentState, MiningCycleState>(productionParams: new(), buildingParams: this, persistentState: new(buildingResPile: buildingResPile));

            IBuildingImage Industry.IConcreteBuildingParams<UnitType>.IdleBuildingImage
                => buildingImage;

            Material? Industry.IConcreteBuildingParams<UnitType>.SurfaceMaterial(bool productionInProgress)
                => SurfaceMaterial;

            // This assumes that planet composition never changes while mining is happening
            EfficientReadOnlyCollection<IResource> Industry.IConcreteBuildingParams<UnitType>.GetProducedResources(UnitType productionParams)
                => producedResources;

            AllResAmounts Industry.IConcreteBuildingParams<UnitType>.MaxStoredInput(UnitType productionParams)
                => AllResAmounts.empty;

            AreaInt Industry.IConcreteBuildingParams<UnitType>.MaxStoredOutputArea()
                => (CurBuildingArea * CurWorldConfig.outputStorageProporOfBuildingArea).RoundDown();
        }

        [Serializable]
        public class PersistentState
        {
            public readonly ResPile buildingResPile;
            public readonly RawMatAllocator minedResAllocator;
            public HistoricCorrector<double> minedAreaHistoricCorrector;

            public PersistentState(ResPile buildingResPile)
            {
                this.buildingResPile = buildingResPile;
                minedResAllocator = new();
                minedAreaHistoricCorrector = new();
            }
        }

        [Serializable]
        private sealed class MiningCycleState : Industry.IProductionCycleState<UnitType, ConcreteBuildingParams, PersistentState, MiningCycleState>
        {
            public static bool IsRepeatable
                => true;

            public static Result<MiningCycleState, TextErrors> Create(UnitType productionParams, ConcreteBuildingParams parameters, PersistentState persistentState,
                ResPile inputStorage, AreaInt maxOutputArea)
            {
                var minedAreaCorrectorWithTarget = persistentState.minedAreaHistoricCorrector.WithTarget(target: parameters.AreaToMine().valueInMetSq);

                // Since will never mine more than requested, suggestion will never be smaller than parameters.AreaToSplit(), thus will always be >= 0.
                var maxAreaToMine = MyMathHelper.Min((UDouble)minedAreaCorrectorWithTarget.suggestion, maxOutputArea.valueInMetSq);
                bool isCapped = minedAreaCorrectorWithTarget.suggestion >= maxOutputArea.valueInMetSq;
                return parameters.NodeState.Mine
                (
                    targetArea: AreaDouble.CreateFromMetSq(valueInMetSq: maxAreaToMine),
                    rawMatAllocator: persistentState.minedResAllocator
                ).SwitchExpression<Result<MiningCycleState, TextErrors>>
                (
                    ok: miningRes =>
                    {
                        // Filter and RawMatComposition does the same thing here as planet contains only raw materials
                        AreaInt miningArea = miningRes.Amount.Filter<RawMaterial>().Area();
                        persistentState.minedAreaHistoricCorrector = isCapped ? new() : minedAreaCorrectorWithTarget.WithValue(value: miningArea.valueInMetSq);
                        return new(ok: new MiningCycleState(buildingParams: parameters, buildingResPile: persistentState.buildingResPile, miningRes: miningRes, miningArea: miningArea));
                    },
                    error: errors =>
                    {
                        persistentState.minedAreaHistoricCorrector = new();
                        return new(errors: errors);
                    }
                );
            }

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool ShouldRestart
                => donePropor.IsFull;

            private readonly ConcreteBuildingParams buildingParams;
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

            private MiningCycleState(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ResPile miningRes, AreaInt miningArea)
            {
                this.buildingParams = buildingParams;
                this.buildingResPile = buildingResPile;
                this.miningRes = miningRes;
                miningMass = miningRes.Amount.Mass();
                this.miningArea = miningArea;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                donePropor = Propor.empty;
            }

            public IBuildingImage BusyBuildingImage()
                => buildingParams.buildingImage;

            public void FrameStart()
            {
                curMiningStats = buildingParams.CurMiningStats(miningMass: miningMass);
                ReqEnergy = reqEnergyHistoricRounder.CurEnergy<ElectricalEnergy>(watts: curMiningStats.ReqWatts, proporUtilized: Propor.full, elapsed: CurWorldManager.Elapsed);
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingPropor = ResAndIndustryHelpers.WorkingPropor(proporUtilized: Propor.full, allocatedEnergy: electricalEnergy, reqEnergy: ReqEnergy);
            }

            /// <summary>
            /// This will not remove no longer needed building components until mining cycle is done since fix current mining volume
            /// and some other mining stats at the start of the mining cycle. 
            /// </summary>
            public IIndustry? Update(ResPile outputStorage)
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                // If mine less than could, probably shouldn't increase mining speed because of that
                // On the other hand that would be a very niche circumstance anyway - basically only when mining last resources of the planet
                donePropor = donePropor.UpdateDonePropor
                (
                    workingPropor: workingPropor,
                    producedAreaPerSec: curMiningStats.ProducedAreaPerSec,
                    elapsed: CurWorldManager.Elapsed,
                    areaInProduction: miningArea.ToDouble()
                );

                if (donePropor.IsFull)
                {
                    outputStorage.TransferAllFrom(source: miningRes);
                    buildingParams.RemoveUnneededBuildingComponents(buildingResPile: buildingResPile, outputStorage: outputStorage);
                }
                return null;
            }

            public void Delete(ResPile outputStorage)
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                buildingParams.NodeState.EnlargeFrom(source: miningRes, amount: miningRes.Amount.Filter<RawMaterial>());
                Debug.Assert(miningRes.IsEmpty);
            }

            public static void DeletePersistentState(PersistentState persistentState, ResPile outputStorage)
                => outputStorage.TransferAllFrom(source: persistentState.buildingResPile);
        }

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Industry<UnitType, ConcreteBuildingParams, PersistentState, MiningCycleState>)
            };
    }
}
