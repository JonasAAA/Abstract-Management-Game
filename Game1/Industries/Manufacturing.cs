using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    // So that if change ManufacturingProductionChoice, will get compilation errors about giving player something to choose in UI and using something different in code
    using ManufacturingProductionChoice = MaterialPalette;
    public static class Manufacturing
    {
#pragma warning disable IDE0001 // Otherwise it says to use ManufacturingProductionChoice instead of MaterialPalette everywhere
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public string Name { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly EnergyPriority energyPriority;
            public readonly Product.Params productParams;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, EnergyPriority energyPriority, Product.Params productParams)
            {
                Name = name;
                BuildingCostPropors = new BuildingCostPropors(ingredProdToAmounts: buildingComponentPropors);

                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.manufacturingBuildingColor);
                
                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.productParams = productParams;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public IHUDElement? CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                => IndustryUIAlgos.CreateMatPaletteChoiceDropdown
                (
                    matPaletteChoiceSetter: productionChoiceSetter.Convert<ManufacturingProductionChoice>(),
                    productClass: productParams.productClass
                );

            public IConcreteBuildingConstructionParams CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
                => new ConcreteBuildingParams
                (
                    nodeState: nodeState,
                    generalParams: this,
                    buildingImage: buildingImageParams.CreateImage(nodeState),
                    buildingComponentsToAmountPUBA: ResAndIndustryAlgos.BuildingComponentsToAmountPUBA
                    (
                        buildingComponentPropors: buildingComponentPropors,
                        buildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                        buildingComponentsProporOfBuildingArea: CurWorldConfig.buildingComponentsProporOfBuildingArea
                    ),
                    buildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                    manufacturingProductionChoice: (ManufacturingProductionChoice)productionChoice.Choice,
                    surfaceMatPalette: neededBuildingMatPaletteChoices[IProductClass.roof]
                );
        }

        [Serializable]
        private readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<ConcreteProductionParams>, IConcreteBuildingConstructionParams
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public MaterialPalette SurfaceMatPalette { get; }
            public readonly DiskBuildingImage buildingImage;
            public readonly Product.Params productParams;
            public readonly ulong overallMaxProductAmount;
            public readonly AreaInt maxStoredOutputArea;

            private readonly AreaDouble buildingArea;
            private readonly GeneralBuildingParams generalParams;
            private readonly MaterialPaletteChoices buildingMatPaletteChoices;
            private readonly ManufacturingProductionChoice manufacturingProductionChoice;
            private readonly AllResAmounts buildingCost;
            private readonly ulong maxInputAmountStored;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA,
                MaterialPaletteChoices buildingMatPaletteChoices, ManufacturingProductionChoice manufacturingProductionChoice, MaterialPalette surfaceMatPalette)
            {
                Name = generalParams.Name;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                SurfaceMatPalette = surfaceMatPalette;
                EnergyPriority = generalParams.energyPriority;
                productParams = generalParams.productParams;
                buildingArea = buildingImage.Area;
                this.generalParams = generalParams;
                this.buildingMatPaletteChoices = buildingMatPaletteChoices;
                this.manufacturingProductionChoice = manufacturingProductionChoice;
                maxStoredOutputArea = (buildingArea * CurWorldConfig.outputStorageProporOfBuildingArea).RoundDown();

                maxInputAmountStored = ResAndIndustryAlgos.MaxAmount
                (
                    availableArea: buildingArea * CurWorldConfig.inputStorageProporOfBuildingArea,
                    itemArea: productParams.area
                );
                overallMaxProductAmount = ResAndIndustryAlgos.MaxAmount
                (
                    availableArea: buildingArea * CurWorldConfig.productionProporOfBuildingArea,
                    itemArea: productParams.area
                );
                buildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: buildingArea);
            }

            public ulong CurMaxProductAmount(AreaInt maxOutputArea)
                => ResAndIndustryAlgos.MaxAmount
                (
                    availableArea: MyMathHelper.Min(buildingArea * CurWorldConfig.productionProporOfBuildingArea, maxOutputArea.ToDouble()),
                    itemArea: productParams.area
                );

            /// <param Name="productionMassIfFull">Mass of stuff in production if industry was fully operational</param>
            public CurProdStats CurProdStats(Mass productionMassIfFull)
                => ResAndIndustryAlgos.CurMechProdStats
                (
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

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, ManufacturingCycleState>
                (
                    productionParams: new(productParams: productParams, manufacturingProductionChoice: manufacturingProductionChoice),
                    buildingParams: this,
                    persistentState: buildingResPile
                );

            IBuildingImage Industry.IConcreteBuildingParams<ConcreteProductionParams>.IdleBuildingImage
                => buildingImage;

            MaterialPalette? Industry.IConcreteBuildingParams<ConcreteProductionParams>.SurfaceMatPalette(bool productionInProgress)
                => SurfaceMatPalette;

            EfficientReadOnlyCollection<IResource> Industry.IConcreteBuildingParams<ConcreteProductionParams>.GetProducedResources(ConcreteProductionParams productionParams)
                => productionParams.ProducedResources;

            AllResAmounts Industry.IConcreteBuildingParams<ConcreteProductionParams>.MaxStoredInput(ConcreteProductionParams productionParams)
            {
                var maxInputAmountStoredCopy = maxInputAmountStored;
                return productionParams.CurProduct.SwitchExpression
                (
                    ok: product => product.Recipe.ingredients * maxInputAmountStoredCopy,
                    error: _ => AllResAmounts.empty
                );
            }
        }

        [Serializable]
        public sealed class ConcreteProductionParams
        {
            public EfficientReadOnlyCollection<IResource> ProducedResources { get; private set; }
            
            /// <summary>
            /// In case of error, returns the needed but not yet set material purposes
            /// </summary>
            public Result<Product, TextErrors> CurProduct
            {
                get => curProduct;
                private set
                {
                    curProduct = value;
                    ProducedResources = value.SwitchExpression
                    (
                        ok: product => new List<IResource>() { product }.ToEfficientReadOnlyCollection(),
                        error: errors => EfficientReadOnlyCollection<IResource>.empty
                    );
                }
            }

            /// <summary>
            /// NEVER use this directly. Always use CurResource instead
            /// </summary>
            private Result<Product, TextErrors> curProduct;
            private readonly Product.Params productParams;

            public ConcreteProductionParams(Product.Params productParams)
                : this(productParams: productParams, manufacturingProductionChoice: null)
            { }

            public ConcreteProductionParams(Product.Params productParams, ManufacturingProductionChoice? manufacturingProductionChoice)
            {
                this.productParams = productParams;
                Update(productMaterialPalette: manufacturingProductionChoice);
            }

            // FOR NOW, don't allow to change the material choices on the fly
            private void Update(MaterialPalette? productMaterialPalette)
                => CurProduct = productMaterialPalette switch
                {
                    MaterialPalette materialPalette => new(ok: productParams.GetProduct(materialPalette: materialPalette)),
                    null => new(errors: new(UIAlgorithms.NoMaterialPaletteChosen))
                };
        }

        /// <summary>
        /// Responds properly to planet shrinking, but NOT to planet widening
        /// </summary>
        [Serializable]
        private sealed class ManufacturingCycleState : Industry.IProductionCycleState<ConcreteProductionParams, ConcreteBuildingParams, ResPile, ManufacturingCycleState>
        {
            public static bool IsRepeatable
                => true;

            public static Result<ManufacturingCycleState, TextErrors> Create(ConcreteProductionParams productionParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile,
                ResPile inputStorage, AreaInt storedOutputArea)
                => productionParams.CurProduct.SelectMany
                (
                    product =>
                    {
                        var resInUseAndCount = ResPile.CreateMultipleIfHaveEnough
                        (
                            source: inputStorage,
                            amount: product.Recipe.ingredients,
                            maxCount: buildingParams.CurMaxProductAmount(maxOutputArea: buildingParams.maxStoredOutputArea - storedOutputArea)
                        );
                        return resInUseAndCount switch
                        {
                            (ResPile resInUse, ulong count) => new Result<ManufacturingCycleState, TextErrors>
                            (
                                ok: new
                                (
                                    buildingParams: buildingParams,
                                    resInUse: resInUse,
                                    productRecipe: product.Recipe,
                                    productionAmount: count,
                                    overallMaxProductionAmount: buildingParams.overallMaxProductAmount
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

            private ManufacturingCycleState(ConcreteBuildingParams buildingParams, ResPile resInUse, ResRecipe productRecipe, ulong productionAmount, ulong overallMaxProductionAmount)
            {
                this.buildingParams = buildingParams;
                this.resInUse = resInUse;
                recipe = productRecipe * productionAmount;
                prodMassIfFull = productRecipe.ingredients.Mass() * overallMaxProductionAmount;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                proporUtilized = Propor.Create(part: productionAmount, whole: overallMaxProductionAmount)!.Value;
                areaInProduction = buildingParams.productParams.area * productionAmount;
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
                typeof(Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, ManufacturingCycleState>)
            };
#pragma warning restore IDE0001
    }
}