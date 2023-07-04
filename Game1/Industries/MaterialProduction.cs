using Game1.Collections;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    /// <summary>
    /// Responds properly to planet shrinking, but NOT to planet widening
    /// </summary>
    public static class MaterialProduction
    {
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public string Name { get; }
            public EfficientReadOnlyDictionary<IMaterialPurpose, Propor> BuildingComponentMaterialPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly GeneralProdAndMatAmounts buildingCostPropors;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, EnergyPriority energyPriority)
            {
                Name = name;
                BuildingComponentMaterialPropors = buildingCostPropors.materialPropors;
                buildingCostPropors = new GeneralProdAndMatAmounts(ingredProdToAmounts: buildingComponentPropors, ingredMatPurposeToUsefulAreas: new());
                if (buildingCostPropors.materialPropors[IMaterialPurpose.roofSurface].IsEmpty)
                    throw new ArgumentException();
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, color: ActiveUIManager.colorConfig.manufacturingBuildingColor);

                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public Result<IConcreteBuildingConstructionParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
                => ResAndIndustryAlgos.BuildingComponentsToAmountPUBA
                (
                    buildingComponentPropors: buildingComponentPropors,
                    buildingMatChoices: neededBuildingMatChoices
                ).Select<IConcreteBuildingConstructionParams>
                (
                    buildingComponentsToAmountPUBA => new ConcreteBuildingParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        buildingImage: buildingImageParams.CreateImage(nodeState),
                        buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                        buildingMatChoices: neededBuildingMatChoices,
                        surfaceMaterial: neededBuildingMatChoices[IMaterialPurpose.roofSurface]
                    )
                );
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<ConcreteProductionParams>, IConcreteBuildingConstructionParams
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
            }

            public ulong MaxProductionAmount(AreaInt materialArea)
                => ResAndIndustryAlgos.MaxAmountInProduction
                (
                    areaInProduction: ResAndIndustryAlgos.AreaInProduction(buildingArea: CurBuildingArea),
                    itemUsefulArea: materialArea
                );

            /// <param Name="productionMassIfFull">Mass of stuff in production if industry was fully operational</param>
            public CurProdStats CurProdStats(Mass productionMassIfFull)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.buildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature,
                    buildingArea: CurBuildingArea,
                    productionMass: productionMassIfFull
                );

            public void RemoveUnneededBuildingComponents(ResPile buildingResPile)
                => ResAndIndustryHelpers.RemoveUnneededBuildingComponents(nodeState: NodeState, buildingResPile: buildingResPile, buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);

            AllResAmounts IConcreteBuildingConstructionParams.BuildingCost
                => startingBuildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, ProductionCycleState>(productionParams: new(), buildingParams: this, persistentState: buildingResPile);

            IBuildingImage Industry.IConcreteBuildingParams<ConcreteProductionParams>.IdleBuildingImage
                => buildingImage;

            Material? Industry.IConcreteBuildingParams<ConcreteProductionParams>.SurfaceMaterial(bool productionInProgress)
                => SurfaceMaterial;

            AllResAmounts Industry.IConcreteBuildingParams<ConcreteProductionParams>.TargetStoredResAmounts(ConcreteProductionParams productionParams)
            {
                var thisCopy = this;
                return productionParams.CurMaterial.SwitchExpression
                (
                    ok: material => material.Recipe.ingredients * thisCopy.MaxProductionAmount(materialArea: material.Area) * thisCopy.NodeState.MaxBatchDemResStored,
                    error: _ => AllResAmounts.empty
                );
            }
        }

        [Serializable]
        public sealed class ConcreteProductionParams
        {
            /// <summary>
            /// Eiher material, or error saying no material was chosen
            /// </summary>
            public Result<Material, TextErrors> CurMaterial { get; private set; }

            public ConcreteProductionParams()
                => CurMaterial = new(errors: new(UIAlgorithms.NoMaterialIsChosen));

            public ConcreteProductionParams(Material material)
                => CurMaterial = new(ok: material);
        }

        [Serializable]
        private sealed class ProductionCycleState : Industry.IProductionCycleState<ConcreteProductionParams, ConcreteBuildingParams, ResPile, ProductionCycleState>
        {
            public static bool IsRepeatable
                => true;

            public static Result<ProductionCycleState, TextErrors> Create(ConcreteProductionParams productionParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile)
                => productionParams.CurMaterial.SelectMany
                (
                    material =>
                    {
                        ulong maxProductionAmount = buildingParams.MaxProductionAmount(materialArea: material.Area);
                        var resInUseAndCount = ResPile.CreateMultipleIfHaveEnough
                        (
                            source: buildingParams.NodeState.StoredResPile,
                            amount: material.Recipe.ingredients,
                            maxCount: maxProductionAmount
                        );
                        return resInUseAndCount switch
                        {
                            (ResPile resInUse, ulong count) => new Result<ProductionCycleState, TextErrors>
                            (
                                ok: new
                                (
                                    buildingParams: buildingParams,
                                    buildingResPile: buildingResPile,
                                    resInUse: resInUse,
                                    material: material,
                                    productionAmount: count,
                                    maxProductionAmount: maxProductionAmount
                                )
                            ),
                            null => new(errors: new(UIAlgorithms.NotEnoughResourcesToStartProduction))
                        };
                    }
                );

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool ShouldRestart
                => donePropor.IsFull;

            private readonly ConcreteBuildingParams buildingParams;
            private readonly ResPile buildingResPile, resInUse;
            private readonly ResRecipe recipe;
            private readonly Mass prodMassIfFull;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private readonly Propor proporUtilized;
            private readonly AreaDouble areaInProduction;

            private CurProdStats curProdStats;
            private Propor donePropor, workingPropor;

            private ProductionCycleState(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ResPile resInUse, Material material, ulong productionAmount, ulong maxProductionAmount)
            {
                this.buildingParams = buildingParams;
                this.buildingResPile = buildingResPile;
                this.resInUse = resInUse;
                recipe = material.Recipe * productionAmount;
                prodMassIfFull = material.Recipe.ingredients.Mass() * maxProductionAmount;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                proporUtilized = Propor.Create(part: productionAmount, whole: maxProductionAmount)!.Value;
                areaInProduction = material.Area.ToDouble() * productionAmount;
                donePropor = Propor.empty;
            }

            public IBuildingImage BusyBuildingImage()
                => buildingParams.buildingImage;

            public void FrameStartNoProduction()
            { }

            public void FrameStart()
            {
                curProdStats = buildingParams.CurProdStats(productionMassIfFull: prodMassIfFull);
#warning if production will be done this frame, could request just enough energy to complete it rather than the usual amount
                ReqEnergy = reqEnergyHistoricRounder.CurEnergy<ElectricalEnergy>(watts: curProdStats.ReqWatts, proporUtilized: proporUtilized, elapsed: CurWorldManager.Elapsed);
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingPropor = ResAndIndustryHelpers.WorkingPropor(proporUtilized: proporUtilized, allocatedEnergy: electricalEnergy, reqEnergy: ReqEnergy);
            }

            /// <summary>
            /// This will not remove no longer needed building components until production cycle is done since fix current max production amount
            /// and some other production stats at the start of production cycle
            /// </summary>
            public IIndustry? Update()
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                donePropor = donePropor.UpdateDonePropor
                (
                    workingPropor: workingPropor,
                    producedAreaPerSec: curProdStats.ProducedAreaPerSec,
                    elapsed: CurWorldManager.Elapsed,
                    areaInProduction: areaInProduction
                );

                if (donePropor.IsFull)
                {
                    buildingParams.NodeState.StoredResPile.TransformFrom(source: resInUse, recipe: recipe);
                    buildingParams.RemoveUnneededBuildingComponents(buildingResPile: buildingResPile);
                }
                return null;
            }

            public void Delete()
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                buildingParams.NodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
                buildingParams.NodeState.StoredResPile.TransferAllFrom(source: resInUse);
            }
        }
    }
}