using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    // So that if change MaterialProductionChoice, will get compilation errors about giving player something to choose in UI and using something different in code
    using MaterialProductionChoice = Material;
    /// <summary>
    /// Responds properly to planet shrinking, but NOT to planet widening
    /// </summary>
    public static class MaterialProduction
    {
#pragma warning disable IDE0001 // Otherwise it says to use MaterialProductionChoice instead of Material everywhere
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public string Name { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, EnergyPriority energyPriority)
            {
                Name = name;
                BuildingCostPropors = new BuildingCostPropors(ingredProdToAmounts: buildingComponentPropors);

                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.manufacturingBuildingColor);

                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public IHUDElement? CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                => IndustryUIAlgos.CreateMaterialChoiceDropdown(materialChoiceSetter: productionChoiceSetter.Convert<MaterialProductionChoice>());

            public IConcreteBuildingConstructionParams CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
                => new ConcreteBuildingParams
                (
                    nodeState: nodeState,
                    generalParams: this,
                    buildingImage: buildingImageParams.CreateImage(nodeState),
                    buildingComponentsToAmountPUBA: ResAndIndustryHelpers.BuildingComponentsToAmountPUBA
                    (
                        buildingComponentPropors: buildingComponentPropors,
                        buildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                        buildingComponentsProporOfBuildingArea: CurWorldConfig.buildingComponentsProporOfBuildingArea
                    ),
                    buildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                    materialProductionChoice: (MaterialProductionChoice)productionChoice.Choice,
                    surfaceMatPalette: neededBuildingMatPaletteChoices[IProductClass.roof]
                );
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<ConcreteProductionParams>, IConcreteBuildingConstructionParams
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public MaterialPalette SurfaceMatPalette { get; }
            public readonly DiskBuildingImage buildingImage;
            public readonly AreaInt maxStoredOutputArea;

            private readonly AreaDouble buildingArea;
            private readonly GeneralBuildingParams generalParams;
            private readonly BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA;
            private readonly MaterialPaletteChoices buildingMatPaletteChoices;
            private readonly MaterialProductionChoice materialProductionChoice;
            private readonly AllResAmounts buildingCost;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA, MaterialPaletteChoices buildingMatPaletteChoices,
                MaterialProductionChoice materialProductionChoice, MaterialPalette surfaceMatPalette)
            {
                Name = generalParams.Name;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                SurfaceMatPalette = surfaceMatPalette;
                EnergyPriority = generalParams.energyPriority;

                buildingArea = buildingImage.Area;
                this.generalParams = generalParams;
                this.buildingComponentsToAmountPUBA = buildingComponentsToAmountPUBA;
                this.buildingMatPaletteChoices = buildingMatPaletteChoices;
                this.materialProductionChoice = materialProductionChoice;
                maxStoredOutputArea = (buildingArea * CurWorldConfig.outputStorageProporOfBuildingArea).RoundDown();
                buildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: buildingArea);
            }

            public ulong OverallMaxProductionAmount(AreaInt materialArea)
                => ResAndIndustryAlgos.MaxAmount
                (
                    availableArea: buildingArea * CurWorldConfig.productionProporOfBuildingArea,
                    itemArea: materialArea
                );

            public ulong CurMaxProductionAmount(AreaInt materialArea, AreaInt maxOutputArea)
                => ResAndIndustryAlgos.MaxAmount
                (
                    availableArea: MyMathHelper.Min(buildingArea * CurWorldConfig.productionProporOfBuildingArea, maxOutputArea.ToDouble()),
                    itemArea: materialArea
                );

            /// <param Name="productionMassIfFull">Mass of stuff in production if industry was fully operational</param>
            public CurProdStats CurProdStats(Mass productionMassIfFull)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                    buildingCostPropors: generalParams.BuildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature,
                    buildingArea: buildingArea,
                    productionMass: productionMassIfFull
                );

            AllResAmounts IConcreteBuildingConstructionParams.BuildingCost
                => buildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            string IConcreteBuildingConstructionParams.GetBuildingStats()
                => "Material production stats TODO";

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, ProductionCycleState>
                (
                    productionParams: new(materialProductionChoice: materialProductionChoice),
                    buildingParams: this,
                    persistentState: buildingResPile
                );

            IBuildingImage Industry.IConcreteBuildingParams<ConcreteProductionParams>.IdleBuildingImage
                => buildingImage;

            MaterialPalette? Industry.IConcreteBuildingParams<ConcreteProductionParams>.SurfaceMatPalette(bool productionInProgress)
                => SurfaceMatPalette;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<ConcreteProductionParams>.GetProducedResources(ConcreteProductionParams productionParams)
                => productionParams.ProducedResources;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<ConcreteProductionParams>.GetConsumedResources(ConcreteProductionParams productionParams)
                => productionParams.ConsumedResources;

            AllResAmounts Industry.IConcreteBuildingParams<ConcreteProductionParams>.MaxStoredInput(ConcreteProductionParams productionParams)
            {
                var buildingAreaCopy = buildingArea;
                return productionParams.CurMaterial.SwitchExpression
                (
                    ok: material => material.Recipe.ingredients * ResAndIndustryAlgos.MaxAmount
                    (
                        availableArea: buildingAreaCopy * CurWorldConfig.inputStorageProporOfBuildingArea,
                        itemArea: material.Area
                    ),
                    error: _ => AllResAmounts.empty
                );
            }
        }

        [Serializable]
        public sealed class ConcreteProductionParams
        {
            public SortedResSet<IResource> ProducedResources { get; private set; }
            public SortedResSet<IResource> ConsumedResources { get; private set; }

            /// <summary>
            /// Eiher material, or error saying no material was chosen
            /// </summary>
            public Result<Material, TextErrors> CurMaterial
            {
                get => curMaterial;
                private set
                {
                    curMaterial = value;
                    (ProducedResources, ConsumedResources) = value.SwitchExpression
                    (
                        ok: material => (ProducedResources: new SortedResSet<IResource>(res: material), ConsumedResources: material.Recipe.ingredients.ResSet),
                        error: material => (ProducedResources: SortedResSet<IResource>.empty, ConsumedResources: SortedResSet<IResource>.empty)
                    );
                }
            }

            /// <summary>
            /// NEVER use this directly. Always use CurMaterial instead
            /// </summary>
            private Result<Material, TextErrors> curMaterial;

            public ConcreteProductionParams()
                => CurMaterial = new(errors: new(UIAlgorithms.NoMaterialIsChosen));

            public ConcreteProductionParams(MaterialProductionChoice materialProductionChoice)
                => CurMaterial = new(ok: materialProductionChoice);
        }

        [Serializable]
        private sealed class ProductionCycleState : Industry.IProductionCycleState<ConcreteProductionParams, ConcreteBuildingParams, ResPile, ProductionCycleState>
        {
            public static bool IsRepeatable
                => true;

            public static Result<ProductionCycleState, TextErrors> Create(ConcreteProductionParams productionParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile,
                ResPile inputStorage, AreaInt storedOutputArea)
                => productionParams.CurMaterial.SelectMany
                (
                    material =>
                    {
                        var maxCount = buildingParams.CurMaxProductionAmount(materialArea: material.Area, maxOutputArea: buildingParams.maxStoredOutputArea - storedOutputArea);
                        if (maxCount is 0)
                            return new(errors: new(UIAlgorithms.OutputStorageFullSoNoProduction));
                        var resInUseAndCount = ResPile.CreateMultipleIfHaveEnough
                        (
                            source: inputStorage,
                            amount: material.Recipe.ingredients,
                            maxCount: maxCount
                        );
                        return resInUseAndCount switch
                        {
                            (ResPile resInUse, ulong count) => new Result<ProductionCycleState, TextErrors>
                            (
                                ok: new
                                (
                                    buildingParams: buildingParams,
                                    resInUse: resInUse,
                                    material: material,
                                    productionAmount: count,
                                    overallMaxProductionAmount: buildingParams.OverallMaxProductionAmount(materialArea: material.Area)
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
            private readonly ResPile resInUse;
            private readonly ResRecipe recipe;
            private readonly Mass prodMassIfFull;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private readonly Propor proporUtilized;
            private readonly AreaInt areaInProduction;

            private CurProdStats curProdStats;
            private Propor donePropor, workingPropor;

            private ProductionCycleState(ConcreteBuildingParams buildingParams, ResPile resInUse, Material material, ulong productionAmount, ulong overallMaxProductionAmount)
            {
                this.buildingParams = buildingParams;
                this.resInUse = resInUse;
                recipe = material.Recipe * productionAmount;
                prodMassIfFull = material.Recipe.ingredients.Mass() * overallMaxProductionAmount;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                proporUtilized = Propor.Create(part: productionAmount, whole: overallMaxProductionAmount)!.Value;
                areaInProduction = material.Area * productionAmount;
                donePropor = Propor.empty;
            }

            public IBuildingImage BusyBuildingImage()
                => buildingParams.buildingImage;

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
            public IIndustry? Update(ResPile outputStorage)
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
                    outputStorage.TransformFrom(source: resInUse, recipe: recipe);
                return null;
            }

            public void Delete(ResPile outputStorage)
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                outputStorage.TransferAllFrom(source: resInUse);
            }

            public static void DeletePersistentState(ResPile buildingResPile, ResPile outputStorage)
                => outputStorage.TransferAllFrom(source: buildingResPile);
        }

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, ProductionCycleState>)
            };
#pragma warning restore IDE0001
    }
}