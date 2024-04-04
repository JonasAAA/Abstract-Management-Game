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
    public static class Mining
    {
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EnergyPriority energyPriority, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.miningBuildingColor);
                NameVisual = UIAlgorithms.GetBuildingNameVisual(name: name);
                BuildingCostPropors = new BuildingCostPropors(ingredProdToAmounts: buildingComponentPropors);

                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            IHUDElement? IGeneralBuildingConstructionParams.CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                => null;

            public IConcreteBuildingConstructionParams CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
                => new ConcreteBuildingParams
                (
                    nodeState: nodeState,
                    generalParams: this,
                    buildingImage: buildingImageParams.CreateImage(nodeShapeParams: nodeState),
                    buildingComponentsToAmountPUBA: ResAndIndustryHelpers.BuildingComponentsToAmountPUBA
                    (
                        buildingComponentPropors: buildingComponentPropors,
                        buildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                        buildingComponentsProporOfBuildingArea: CurWorldConfig.buildingComponentsProporOfBuildingArea
                    ),
                    buildingMatPaletteChoices: neededBuildingMatPaletteChoices
                );

            IndustryFunctionVisualParams IGeneralBuildingConstructionParams.IncompleteFunctionVisualParams(ProductionChoice? productionChoice)
                => new
                (
                    InputIcons: [IIndustry.cosmicBodyIcon, IIndustry.electricityIcon],
                    OutputIcons: [IIndustry.resIcon]
                );
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<UnitType>, IConcreteBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public readonly DiskBuildingImage buildingImage;
            public readonly BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA;

            /// <summary>
            /// Things depend on this rather than on building components target area as can say that if planet underneath building shrinks,
            /// building gets not enough space to operate at maximum efficiency
            /// </summary>
            public AreaDouble CurBuildingArea
                => buildingImage.Area;

            private readonly BuildingCostPropors buildingCostPropors;
            private readonly MaterialPaletteChoices buildingMatPaletteChoices;
            // BOTH mined and produced resources will not show any new materials the planet produces after this is built
            // They would still be mined, I think. Or maybe the game would crash, don't know.
            private readonly SortedResSet<RawMaterial> minedResources;
            private readonly SortedResSet<IResource> producedResources;
            private readonly AllResAmounts startingBuildingCost;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA,
                MaterialPaletteChoices buildingMatPaletteChoices)
            {
                NameVisual = generalParams.NameVisual;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                EnergyPriority = generalParams.energyPriority;

                buildingCostPropors = generalParams.BuildingCostPropors;
                this.buildingComponentsToAmountPUBA = buildingComponentsToAmountPUBA;
                this.buildingMatPaletteChoices = buildingMatPaletteChoices;
                startingBuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);
                minedResources = nodeState.Composition.ResSet;
                producedResources = minedResources.ToAll().UnionWith(otherResSet: startingBuildingCost.ResSet);
            }

            public static AreaInt HypotheticOutputStorageArea(AreaDouble hypotheticBuildingArea)
                => (hypotheticBuildingArea * CurWorldConfig.outputStorageProporOfBuildingArea).RoundDown();

            public AreaDouble AreaToMine()
                => CurBuildingArea * CurWorldConfig.productionProporOfBuildingArea;

            /// <param Name="miningMass">Mass of materials curretly being mined</param>
            public MechProdStats CurMiningStats(Mass miningMass)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature,
                    buildingArea: CurBuildingArea,
                    productionMass: miningMass
                );

            public void RemoveUnneededBuildingComponents(ResPile buildingResPile, ResPile outputStorage)
            {
                var buildingComponentsToRemove = buildingResPile.Amount - ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA, CurBuildingArea);
                if (buildingComponentsToRemove.Area() >= CurWorldConfig.minUsefulBuildingComponentAreaToRemove)
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
            {
                var statsGraphsParams = (buildingMatPaletteChoices, buildingCostPropors);
                return new Industry<UnitType, ConcreteBuildingParams, PersistentState, MiningCycleState>
                (
                    productionParams: UnitType.value,
                    buildingParams: this,
                    persistentState: new(buildingResPile: buildingResPile),
                    statsGraphsParams: statsGraphsParams
                );
            }

            static bool Industry.IConcreteBuildingParams<UnitType>.RequiresResources
                => false;

            static bool Industry.IConcreteBuildingParams<UnitType>.ProducesResources
                => true;

            IBuildingImage Industry.IConcreteBuildingParams<UnitType>.IdleBuildingImage
                => buildingImage;

            // This assumes that planet composition never changes while mining is happening
            SortedResSet<IResource> Industry.IConcreteBuildingParams<UnitType>.GetProducedResources(UnitType productionParams)
                => producedResources;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<UnitType>.GetConsumedResources(UnitType productionParams)
                => SortedResSet<IResource>.empty;

            AllResAmounts Industry.IConcreteBuildingParams<UnitType>.MaxStoredInput(UnitType productionParams)
                => AllResAmounts.empty;

            IndustryFunctionVisualParams Industry.IConcreteBuildingParams<UnitType>.IndustryFunctionVisualParams(UnitType productionParams)
                => new
                (
                    InputIcons: [IIndustry.cosmicBodyIcon, IIndustry.electricityIcon],
                    OutputIcons:
                        from res in minedResources
                        select res.SmallIcon
                );
        }

        [Serializable]
        public sealed class PersistentState
        {
            public readonly ResPile buildingResPile;
            public HistoricCorrector<double> minedAreaHistoricCorrector;

            public PersistentState(ResPile buildingResPile)
            {
                this.buildingResPile = buildingResPile;
                minedAreaHistoricCorrector = new();
            }
        }

        [Serializable]
        private sealed class MiningCycleState : Industry.IProductionCycleState<UnitType, ConcreteBuildingParams, PersistentState, MiningCycleState>
        {
            public static bool IsRepeatable
                => true;

            public static Result<MiningCycleState, TextErrors> Create(UnitType productionParams, ConcreteBuildingParams parameters, PersistentState persistentState,
                ResPile inputStorage, AreaInt storedOutputArea)
            {
                ulong maxPossibleAreaToMine = Algorithms.FindMaxOkValue
                (
                    minValue: 0,
                    maxValue: MyMathHelper.Min
                    (
                        parameters.NodeState.Area,
                        ConcreteBuildingParams.HypotheticOutputStorageArea(hypotheticBuildingArea: parameters.CurBuildingArea) - storedOutputArea
                    ).valueInMetSq,
                    isValueOk: tryMaxAreaToMineInMetSq =>
                    {
                        var tryMaxAreaToMine = AreaInt.CreateFromMetSq(valueInMetSq: tryMaxAreaToMineInMetSq);
                        var newPlanetArea = parameters.NodeState.Area - tryMaxAreaToMine;
                        var newBuildingArea = parameters.buildingImage.HypotheticalArea(hypotheticPlanetArea: newPlanetArea);
                        var newBuildingComponents = ResAndIndustryHelpers.CurNeededBuildingComponents
                        (
                            buildingComponentsToAmountPUBA: parameters.buildingComponentsToAmountPUBA,
                            curBuildingArea: newBuildingArea
                        );
                        AllResAmounts newNoLongerNeededBuildingComponents = persistentState.buildingResPile.Amount - newBuildingComponents;
                        return storedOutputArea + tryMaxAreaToMine + newNoLongerNeededBuildingComponents.Area() <= ConcreteBuildingParams.HypotheticOutputStorageArea(hypotheticBuildingArea: newBuildingArea);
                    }
                );

                var minedAreaCorrectorWithTarget = persistentState.minedAreaHistoricCorrector.WithTarget(target: parameters.AreaToMine().valueInMetSq);

                // Since will never mine more than requested, suggestion will never be smaller than parameters.AreaToSplit(), thus will always be >= 0.
                var maxAreaToMine = MyMathHelper.Min((ulong)minedAreaCorrectorWithTarget.suggestion, maxPossibleAreaToMine);
                bool isCapped = minedAreaCorrectorWithTarget.suggestion >= maxPossibleAreaToMine;
                return parameters.NodeState.Mine
                (
                    targetArea: AreaInt.CreateFromMetSq(valueInMetSq: maxAreaToMine)
                ).SwitchExpression<Result<MiningCycleState, TextErrors>>
                (
                    ok: miningRes =>
                    {
                        // Filter and RawMatComposition does the same thing here as planet contains only raw materials
                        AreaInt miningArea = miningRes.Amount.Filter<RawMaterial>().Area();
                        persistentState.minedAreaHistoricCorrector = isCapped ? new() : minedAreaCorrectorWithTarget.WithValue(value: miningArea.valueInMetSq);
                        return miningArea.IsZero switch
                        {
                            true => new(errors: new(UIAlgorithms.OutputStorageFullSoNoProduction)),
                            false => new
                            (
                                ok: new MiningCycleState
                                (
                                    buildingParams: parameters,
                                    buildingResPile: persistentState.buildingResPile,
                                    miningRes: miningRes,
                                    miningArea: miningArea
                                )
                            )
                        };
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
            private readonly Propor proporUtilized;

            private MechProdStats curMiningStats;
            private Propor donePropor;
            private Result<Propor, TextErrors> workingProporOrPauseReasons;

            private MiningCycleState(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ResPile miningRes, AreaInt miningArea)
            {
                this.buildingParams = buildingParams;
                this.buildingResPile = buildingResPile;
                this.miningRes = miningRes;
                miningMass = miningRes.Amount.Mass();
                this.miningArea = miningArea;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                proporUtilized = Propor.full;
                donePropor = Propor.empty;
            }

            public IBuildingImage BusyBuildingImage()
                => buildingParams.buildingImage;

            public Propor FrameStartAndReturnThroughputUtilization()
            {
                curMiningStats = buildingParams.CurMiningStats(miningMass: miningMass);
                ReqEnergy = ResAndIndustryHelpers.CurEnergy<ElectricalEnergy>(watts: curMiningStats.ReqWatts, proporUtilized: Propor.full, elapsed: CurWorldManager.Elapsed);
                return proporUtilized;
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingProporOrPauseReasons = ResAndIndustryHelpers.WorkingPropor(proporUtilized: proporUtilized, allocatedEnergy: electricalEnergy, reqEnergy: ReqEnergy);
            }

            /// <summary>
            /// This will not remove no longer needed building components until mining cycle is done since fix current mining volume
            /// and some other mining stats at the start of the mining cycle. 
            /// </summary>
            public Result<IIndustry?, TextErrors> Update(ResPile outputStorage)
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                // If mine less than could, probably shouldn't increase mining speed because of that
                // On the other hand that would be a very niche circumstance anyway - basically only when mining last resources of the planet
                (donePropor, var pauseReasons) = donePropor.UpdateDonePropor
                (
                    workingProporOrPauseReasons: workingProporOrPauseReasons,
                    producedAreaPerSecOrPauseReasons: curMiningStats.ProducedAreaPerSecOrPauseReasons,
                    elapsed: CurWorldManager.Elapsed,
                    areaInProduction: miningArea
                );

                if (donePropor.IsFull)
                {
                    outputStorage.TransferAllFrom(source: miningRes);
                    buildingParams.RemoveUnneededBuildingComponents(buildingResPile: buildingResPile, outputStorage: outputStorage);
                }
                return pauseReasons.Select<IIndustry?>(func: _ => null);
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
