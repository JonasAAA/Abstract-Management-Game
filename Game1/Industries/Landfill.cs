﻿using Game1.Collections;
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
#pragma warning disable IDE0001 // Otherwise it says to use ManufacturingProductionChoice instead of MaterialPalette everywhere
        [Serializable]
        public sealed class GeneralParams : IGeneralBuildingConstructionParams
        {
            public string Name { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, EnergyPriority energyPriority)
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
                => IndustryUIAlgos.CreateRresourceChoiceDropdown(resChoiceSetter: productionChoiceSetter.Convert<LandfillResourceChoice>());

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
                    landfillResourceChoice: (LandfillResourceChoice)productionChoice.Choice,
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
            public readonly EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA;

            private AreaDouble BuildingArea
                => buildingImage.Area;
            
            private readonly GeneralParams generalParams;
            private readonly MaterialPaletteChoices buildingMatPaletteChoices;
            private readonly LandfillResourceChoice landfillResourceChoice;
            private readonly AllResAmounts buildingCost;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, DiskBuildingImage buildingImage,
                EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA,
                MaterialPaletteChoices buildingMatPaletteChoices, LandfillResourceChoice landfillResourceChoice, MaterialPalette surfaceMatPalette)
            {
                Name = generalParams.Name;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                SurfaceMatPalette = surfaceMatPalette;
                EnergyPriority = generalParams.energyPriority;
                this.generalParams = generalParams;
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
                    curBuildingArea: BuildingArea * CurWorldConfig.inputStorageProporOfBuildingArea * CurWorldConfig.buildingComponentStorageProporOfInputStorageArea
                );

            /// <param Name="landfillingMassIfFull">Mass of stuff being dumped if landfill was fully operational</param>
            public CurProdStats CurLandfillingStats(Mass landfillingMassIfFull)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.BuildingCostPropors,
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
                => new Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, LandfillCycleState>
                (
                    productionParams: new(landfillResourceChoice: landfillResourceChoice),
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

            AreaInt Industry.IConcreteBuildingParams<ConcreteProductionParams>.MaxStoredOutputArea()
                => AreaInt.zero;
        }

        [Serializable]
        public sealed class ConcreteProductionParams
        {
            public EfficientReadOnlyCollection<IResource> ProducedResources { get; private set; }

            /// <summary>
            /// In case of error, returns the needed but not yet set material purposes
            /// </summary>
            public Result<IResource, TextErrors> CurResource
            {
                get => curResource;
                private set
                {
                    curResource = value;
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
            private Result<IResource, TextErrors> curResource;

            public ConcreteProductionParams()
                : this(landfillResourceChoice: null)
            { }

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
                ResPile inputStorage, AreaInt maxOutputArea)
            {
                return productionParams.CurResource.SelectMany
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

                        (AllResAmounts buildingComponentsToAdd, ulong resToDump) = TakeResToUse
                        (
                            resForBuildingExpansion: resForBuildingExpansion,
                            resForDumping: resForDumping
                        );
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
                                    overallMaxProductionAmount: buildingParams.OverallMaxResAmount(resource: resource)
                                )
                            ),
                            0 => new(errors: new(UIAlgorithms.NotEnoughResourcesToStartProduction))
                        };
                    }
                );

                (AllResAmounts buildingComponentsToAdd, ulong resToDump) TakeResToUse(AllResAmounts resForBuildingExpansion, ResAmount<IResource> resForDumping)
                {
#warning Test this (probably extract it to a method, put it into ResAndIndustryAlgos, and test that
                    // TODO: extract the binary search part to a separate algorithm? Or it it not general enough?
                    ulong minResToDump = 0, maxResToDump = resForDumping.amount;
                    while (true)
                    {
                        ulong midResToDump = (minResToDump + maxResToDump + 1) / 2;
                        var newPlanetArea = buildingParams.NodeState.Area + resForDumping.res.Area * midResToDump;
                        var newNeededBuildingComponents = ResAndIndustryHelpers.CurNeededBuildingComponents
                        (
                            buildingComponentsToAmountPUBA: buildingParams.buildingComponentsToAmountPUBA,
                            curBuildingArea: buildingParams.buildingImage.HypotheticalArea(hypotheticPlanetArea: newPlanetArea)
                        ) - buildingResPile.Amount;
                        if (newNeededBuildingComponents <= resForBuildingExpansion)
                            minResToDump = midResToDump;
                        else
                            maxResToDump = midResToDump - 1;
                        if (minResToDump == maxResToDump)
                            return (buildingComponentsToAdd: newNeededBuildingComponents, resToDump: minResToDump);
                    }
                }
            }

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool ShouldRestart
                => donePropor.IsFull;

            private readonly ConcreteBuildingParams buildingParams;
            private readonly ResPile buildingResPile, buildingComponentsToAdd, resToDump;
            private readonly Mass landfillingMassIfFull;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private readonly Propor proporUtilized;
            private readonly AreaInt areaToDump;

            private CurProdStats curLandfillingStats;
            private Propor donePropor, workingPropor;

            private LandfillCycleState(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ResPile buildingComponentsToAdd, ResPile resToDump, ulong productionAmount, ulong overallMaxProductionAmount)
            {
                this.buildingParams = buildingParams;
                this.buildingResPile = buildingResPile;
                this.buildingComponentsToAdd = buildingComponentsToAdd;
                this.resToDump = resToDump;
                Debug.Assert(resToDump.Amount.Mass().valueInKg % productionAmount is 0);
                landfillingMassIfFull = Mass.CreateFromKg(valueInKg: resToDump.Amount.Mass().valueInKg / productionAmount * overallMaxProductionAmount);
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
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
                ReqEnergy = reqEnergyHistoricRounder.CurEnergy<ElectricalEnergy>(watts: curLandfillingStats.ReqWatts, proporUtilized: proporUtilized, elapsed: CurWorldManager.Elapsed);
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
                    producedAreaPerSec: curLandfillingStats.ProducedAreaPerSec,
                    elapsed: CurWorldManager.Elapsed,
                    areaInProduction: areaToDump
                );

                if (donePropor.IsFull)
                {
                    resToDump.TransformFrom(source: resToDump, recipe: resToDump.Amount.TurningIntoRawMatsRecipe());
                    buildingParams.NodeState.EnlargeFrom(source: resToDump, amount: resToDump.Amount.RawMatComposition());
                    buildingResPile.TransferAllFrom(source: buildingComponentsToAdd);
                }
                return null;
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

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Industry<ConcreteProductionParams, ConcreteBuildingParams, ResPile, LandfillCycleState>)
            };
#pragma warning restore IDE0001
    }
}