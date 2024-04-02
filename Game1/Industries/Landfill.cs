using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    // So that if change ManufacturingProductionChoice, will get compilation errors about giving player something to choose in UI and using something different in code
    using LandfillResourceChoice = IResource;
    public static class Landfill
    {
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, EnergyPriority energyPriority)
            {
                NameVisual = UIAlgorithms.GetBuildingNameVisual(name: name);
                BuildingCostPropors = new BuildingCostPropors(ingredProdToAmounts: buildingComponentPropors);

                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.landfillBuildingColor);

                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public IHUDElement? CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                => ResAndIndustryUIAlgos.CreateResourceChoiceDropdown(resChoiceSetter: productionChoiceSetter.Convert<LandfillResourceChoice>());

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
                    landfillResourceChoice: (LandfillResourceChoice)productionChoice.Choice,
                    surfaceMatPalette: neededBuildingMatPaletteChoices[ProductClass.roof]
                );
            IndustryFunctionVisualParams IGeneralBuildingConstructionParams.IncompleteFunctionVisualParams(ProductionChoice? productionChoice)
                => IncompleteFunctionVisualParams(productionParams: new((LandfillResourceChoice?)productionChoice?.Choice));
        }

        [Serializable]
        private readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<ConcreteProductionParams>, IConcreteBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public MaterialPalette SurfaceMatPalette { get; }
            public readonly DiskBuildingImage buildingImage;
            public readonly BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA;

            private AreaDouble BuildingArea
                => buildingImage.Area;
            
            private readonly BuildingCostPropors buildingCostPropors;
            private readonly MaterialPaletteChoices buildingMatPaletteChoices;
            private readonly LandfillResourceChoice landfillResourceChoice;
            private readonly AllResAmounts buildingCost;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA,
                MaterialPaletteChoices buildingMatPaletteChoices, LandfillResourceChoice landfillResourceChoice, MaterialPalette surfaceMatPalette)
            {
                NameVisual = generalParams.NameVisual;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                SurfaceMatPalette = surfaceMatPalette;
                EnergyPriority = generalParams.energyPriority;
                buildingCostPropors = generalParams.BuildingCostPropors;
                this.buildingMatPaletteChoices = buildingMatPaletteChoices;
                this.landfillResourceChoice = landfillResourceChoice;
                this.buildingComponentsToAmountPUBA = buildingComponentsToAmountPUBA;

                buildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: BuildingArea);
            }

            public ulong OverallMaxResAmount(IResource resource)
                => ResAndIndustryAlgos.MaxAmount
                (
                    availableArea: BuildingArea * CurWorldConfig.productionProporOfBuildingArea,
                    itemArea: resource.Area
                );

            public AllResAmounts MaxBuildingComponentStoredAmount()
                => ResAndIndustryHelpers.MaxBuildingComponentsInArea
                (
                    buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                    area: BuildingArea * CurWorldConfig.inputStorageProporOfBuildingArea * CurWorldConfig.buildingComponentStorageProporOfInputStorageArea,
                    buildingComponentsProporOfBuildingArea: CurWorldConfig.buildingComponentsProporOfBuildingArea
                );

            /// <param Name="landfillingMassIfFull">Mass of stuff being dumped if landfill was fully operational</param>
            public MechProdStats CurLandfillingStats(Mass landfillingMassIfFull)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature,
                    buildingArea: BuildingArea,
                    productionMass: landfillingMassIfFull
                );

            AllResAmounts IConcreteBuildingConstructionParams.BuildingCost
                => buildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
            {
                var statsGraphsParams = (buildingMatPaletteChoices, buildingCostPropors);
                return new Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, LandfillCycleState>
                (
                    productionParams: new(landfillResourceChoice: landfillResourceChoice),
                    buildingParams: this,
                    persistentState: buildingResPile,
                    statsGraphsParams: statsGraphsParams
                );
            }

            static bool Industry.IConcreteBuildingParams<ConcreteProductionParams>.RequiresResources
                => true;

            static bool Industry.IConcreteBuildingParams<ConcreteProductionParams>.ProducesResources
                => false;

            IBuildingImage Industry.IConcreteBuildingParams<ConcreteProductionParams>.IdleBuildingImage
                => buildingImage;

            MaterialPalette? Industry.IConcreteBuildingParams<ConcreteProductionParams>.SurfaceMatPalette(bool productionInProgress)
                => SurfaceMatPalette;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<ConcreteProductionParams>.GetProducedResources(ConcreteProductionParams productionParams)
                => SortedResSet<IResource>.empty;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<ConcreteProductionParams>.GetConsumedResources(ConcreteProductionParams productionParams)
                => productionParams.ConsumedResources.UnionWith(otherResSet: buildingCost.ResSet);

            AllResAmounts Industry.IConcreteBuildingParams<ConcreteProductionParams>.MaxStoredInput(ConcreteProductionParams productionParams)
            {
                var inputStorageArea = BuildingArea * CurWorldConfig.inputStorageProporOfBuildingArea;
                return productionParams.CurResource.SwitchExpression
                (
                    ok: resource => new AllResAmounts
                    (
                        res: resource,
                        amount: ResAndIndustryAlgos.MaxAmount
                        (
                            availableArea: inputStorageArea * CurWorldConfig.buildingComponentStorageProporOfInputStorageArea.Opposite(),
                            itemArea: resource.Area
                        )
                    ),
                    error: _ => AllResAmounts.empty
                ) + MaxBuildingComponentStoredAmount();
            }

            IndustryFunctionVisualParams Industry.IConcreteBuildingParams<ConcreteProductionParams>.IndustryFunctionVisualParams(ConcreteProductionParams productionParams)
                => IncompleteFunctionVisualParams(productionParams: productionParams);
        }

        [Serializable]
        public sealed class ConcreteProductionParams
        {
            public SortedResSet<IResource> ConsumedResources { get; private set; }

            /// <summary>
            /// In case of error, returns the needed but not yet set material purposes
            /// </summary>
            public Result<IResource, TextErrors> CurResource
            {
                get => curResource;
                private set
                {
                    curResource = value;
                    ConsumedResources = value.SwitchExpression
                    (
                        ok: res => new SortedResSet<IResource>(res: res),
                        error: res => SortedResSet<IResource>.empty
                    );
                }
            }

            /// <summary>
            /// NEVER use this directly. Always use CurResource instead
            /// </summary>
            private Result<IResource, TextErrors> curResource;

            public ConcreteProductionParams(LandfillResourceChoice? landfillResourceChoice)
                => Update(landfillRes: landfillResourceChoice);

            // FOR NOW, don't allow to change the resource choice on the fly
            private void Update(IResource? landfillRes)
                => CurResource = landfillRes switch
                {
                    IResource resource => new(ok: resource),
                    null => new(errors: new(UIAlgorithms.NoMaterialPaletteChosen))
                };
        }

        /// <summary>
        /// Responds properly to planet shrinking, but NOT to planet widening
        /// </summary>
        [Serializable]
        private sealed class LandfillCycleState : Industry.IProductionCycleState<ConcreteProductionParams, ConcreteBuildingParams, ResPile, LandfillCycleState>
        {
            public static bool IsRepeatable
                => true;

            public static Result<LandfillCycleState, TextErrors> Create(ConcreteProductionParams productionParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile,
                ResPile inputStorage, AreaInt storedOutputArea)
                => productionParams.CurResource.SelectMany
                (
                    resource =>
                    {
                        // Want to ensure to use inputs as building components first and only then as landfill resources.
                        // The minus part only relevant (i.e. non-zero) in case one of the building components is also dumped in the landfill.
                        var resForBuildingExpansion = MyMathHelper.Min(inputStorage.Amount, buildingParams.MaxBuildingComponentStoredAmount());
                        var resForDumpingAll = inputStorage.Amount - resForBuildingExpansion;
                        ResAmount<IResource> resForDumping = new(res: resource, amount: resForDumpingAll[resource]);
                        // Ensure that only the resource chosen for dumping is being dumped
                        Debug.Assert(resForDumpingAll == new AllResAmounts(resForDumping));

                        ulong overallMaxResToDump = buildingParams.OverallMaxResAmount(resource: resource);

                        ulong resToDump = Algorithms.FindMaxOkValue
                        (
                            minValue: 0,
                            maxValue: MyMathHelper.Min(resForDumping.amount, overallMaxResToDump),
                            isValueOk: tryResToDump => BuildingComponentsToAdd(resToDump: tryResToDump) <= resForBuildingExpansion
                        );
                        var buildingComponentsToAdd = BuildingComponentsToAdd(resToDump: resToDump);
                        Debug.Assert(buildingComponentsToAdd <= resForBuildingExpansion);

                        return resToDump switch
                        {
                            > 0 => new Result<LandfillCycleState, TextErrors>
                            (
                                ok: new
                                (
                                    buildingParams: buildingParams,
                                    buildingResPile: buildingResPile,
                                    buildingComponentsToAdd: ResPile.CreateIfHaveEnough(source: inputStorage, amount: buildingComponentsToAdd)!,
                                    resToDump: ResPile.CreateIfHaveEnough
                                    (
                                        source: inputStorage,
                                        amount: new AllResAmounts(res: resource, amount: resToDump)
                                    )!,
                                    productionAmount: resToDump,
                                    overallMaxProductionAmount: overallMaxResToDump
                                )
                            ),
                            0 => new(errors: new(UIAlgorithms.NotEnoughResourcesToStartProduction))
                        };

                        AllResAmounts BuildingComponentsToAdd(ulong resToDump)
                        {
                            var newPlanetArea = buildingParams.NodeState.Area + resource.Area * resToDump;
                            return ResAndIndustryHelpers.CurNeededBuildingComponents
                            (
                                buildingComponentsToAmountPUBA: buildingParams.buildingComponentsToAmountPUBA,
                                curBuildingArea: buildingParams.buildingImage.HypotheticalArea(hypotheticPlanetArea: newPlanetArea)
                            ) - buildingResPile.Amount;
                        }
                    }
                );

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool ShouldRestart
                => donePropor.IsFull;

            private readonly ConcreteBuildingParams buildingParams;
            private readonly ResPile buildingResPile, buildingComponentsToAdd, resToDump;
            private readonly Mass landfillingMassIfFull;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly Propor proporUtilized;
            private readonly AreaInt areaToDump;

            private MechProdStats curLandfillingStats;
            private Propor donePropor;
            private Result<Propor, TextErrors> workingProporOrPauseReasons;

            private LandfillCycleState(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ResPile buildingComponentsToAdd, ResPile resToDump, ulong productionAmount, ulong overallMaxProductionAmount)
            {
                this.buildingParams = buildingParams;
                this.buildingResPile = buildingResPile;
                this.buildingComponentsToAdd = buildingComponentsToAdd;
                this.resToDump = resToDump;
                Debug.Assert(resToDump.Amount.Mass().valueInKg % productionAmount is 0);
                landfillingMassIfFull = Mass.CreateFromKg(valueInKg: resToDump.Amount.Mass().valueInKg / productionAmount * overallMaxProductionAmount);
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                proporUtilized = Propor.Create(part: productionAmount, whole: overallMaxProductionAmount)!.Value;
                areaToDump = resToDump.Amount.Area();
                donePropor = Propor.empty;
            }

            public IBuildingImage BusyBuildingImage()
                => buildingParams.buildingImage;

            public void FrameStart()
            {
#warning Currenlty, landfill adding new building components and mining removing building components doesn't cost any energy. Should probably change that 
                curLandfillingStats = buildingParams.CurLandfillingStats(landfillingMassIfFull: landfillingMassIfFull);
#warning if production will be done this frame, could request just enough energy to complete it rather than the usual amount
                ReqEnergy = ResAndIndustryHelpers.CurEnergy<ElectricalEnergy>
                (
                    watts: curLandfillingStats.ReqWatts,
                    proporUtilized: proporUtilized,
                    elapsed: CurWorldManager.Elapsed
                );
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingProporOrPauseReasons = ResAndIndustryHelpers.WorkingPropor(proporUtilized: proporUtilized, allocatedEnergy: electricalEnergy, reqEnergy: ReqEnergy);
            }

            /// <summary>
            /// This will not remove no longer needed building components until production cycle is done since fix current max production amount
            /// and some other production stats at the start of production cycle
            /// </summary>
            public Result<IIndustry?, TextErrors> Update(ResPile outputStorage)
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                (donePropor, var pauseReasons) = donePropor.UpdateDonePropor
                (
                    workingProporOrPauseReasons: workingProporOrPauseReasons,
                    producedAreaPerSecOrPauseReasons: curLandfillingStats.ProducedAreaPerSecOrPauseReasons,
                    elapsed: CurWorldManager.Elapsed,
                    areaInProduction: areaToDump
                );

                if (donePropor.IsFull)
                {
                    resToDump.TransformFrom(source: resToDump, recipe: resToDump.Amount.TurningIntoRawMatsRecipe());
                    buildingParams.NodeState.EnlargeFrom(source: resToDump, amount: resToDump.Amount.RawMatComposition());
                    buildingResPile.TransferAllFrom(source: buildingComponentsToAdd);
                }
                return pauseReasons.Select<IIndustry?>(func: _ => null);
            }

            public void Delete(ResPile outputStorage)
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                outputStorage.TransferAllFrom(source: resToDump);
                outputStorage.TransferAllFrom(source: buildingComponentsToAdd);
            }

            public static void DeletePersistentState(ResPile buildingResPile, ResPile outputStorage)
                => outputStorage.TransferAllFrom(source: buildingResPile);
        }

        private static IndustryFunctionVisualParams IncompleteFunctionVisualParams(ConcreteProductionParams productionParams)
            => productionParams.CurResource.SwitchExpression<IndustryFunctionVisualParams>
            (
                ok: res => new
                (
                    InputIcons: [res.SmallIcon, IIndustry.electricityIcon],
                    OutputIcons: [IIndustry.cosmicBodyIcon]
                ),
                error: _ => new
                (
                    InputIcons: [IIndustry.resIcon, IIndustry.electricityIcon],
                    OutputIcons: [IIndustry.cosmicBodyIcon]
                )
            );

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, LandfillCycleState>)
            };
    }
}
