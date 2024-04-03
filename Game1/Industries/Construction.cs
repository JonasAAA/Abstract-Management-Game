using Game1.Collections;
using Game1.ContentNames;
using Game1.Delegates;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.GlobalTypes.GameConfig;

namespace Game1.Industries
{
    public static class Construction
    {
        [Serializable]
        public sealed class GeneralParams
        {
            public readonly IFunction<IHUDElement> nameVisual;
            public readonly IGeneralBuildingConstructionParams buildingGeneralParams;
            public readonly EnergyPriority energyPriority;
            public readonly IFunction<IHUDElement> buildButtonNameVisual;
            public readonly ITooltip toopltip;
            public readonly EfficientReadOnlyDictionary<ProductClass, Propor> neededProductClassPropors;

            public GeneralParams(IGeneralBuildingConstructionParams buildingGeneralParams, EnergyPriority energyPriority)
            {
                nameVisual = UIAlgorithms.GetConstructionNameVisual(childIndustryNameVisual: buildingGeneralParams.NameVisual);
                this.buildingGeneralParams = buildingGeneralParams;
                this.energyPriority = energyPriority;
                buildButtonNameVisual = buildingGeneralParams.NameVisual;
                toopltip = new ImmutableTextTooltip(text: UIAlgorithms.ConstructionTooltip(constrGeneralParams: this));
                neededProductClassPropors = buildingGeneralParams.BuildingCostPropors.neededProductClassPropors;
            }

            /// <summary>
            /// Return null if no production choice is needed
            /// </summary>
            public IHUDElement? CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                => buildingGeneralParams.CreateProductionChoicePanel(productionChoiceSetter: productionChoiceSetter);

            public bool SufficientBuildingMatPalettes(MaterialPaletteChoices curBuildingMatPaletteChoices)
                => buildingGeneralParams.BuildingCostPropors.neededProductClasses.IsSubsetOf(other: curBuildingMatPaletteChoices.Choices.Keys);

            public ConcreteParams CreateConcrete(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
                => new
                (
                    nodeState: nodeState,
                    generalParams: this,
                    concreteBuildingParams: buildingGeneralParams.CreateConcrete
                    (
                        nodeState: nodeState,
                        neededBuildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                        productionChoice: productionChoice
                    )
                );

            public IHUDElement GetIncompleteBuildingComponentVisual(EfficientReadOnlyDictionary<ProductClass, MaterialPalette> incompleteMatPaletteChoices)
                => ResAndIndustryUIAlgos.CreateIndustryFunctionInputOrOutputVisual
                (
                    icons:
                        from prodParamsAndAmount in buildingGeneralParams.BuildingCostPropors.ingredProdToAmounts
                        let prodParams = prodParamsAndAmount.prodParams
                        select incompleteMatPaletteChoices.TryGetValue(key: prodParams.productClass, out var matPalette) switch
                        {
                            true => prodParams.GetProduct(materialPalette: matPalette).SmallIcon,
                            false => prodParams.smallIcon.WithMatPaletteNotYetChosenBackgroundColor()
                        }
                );

            public IndustryFunctionVisualParams IncompleteBuildingFunctionVisualParams(ProductionChoice? productionChoice)
                => buildingGeneralParams.IncompleteFunctionVisualParams(productionChoice: productionChoice);
        }

        [Serializable]
        public readonly struct ConcreteParams : Industry.IConcreteBuildingParams<UnitType>
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public readonly AllResAmounts buildingCost;
            public readonly AreaInt buildingComponentsArea;

            private readonly IConcreteBuildingConstructionParams concreteBuildingParams;

            public ConcreteParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, IConcreteBuildingConstructionParams concreteBuildingParams)
            {
                NameVisual = generalParams.nameVisual;
                NodeState = nodeState;
                EnergyPriority = generalParams.energyPriority;
                buildingCost = concreteBuildingParams.BuildingCost;
                buildingComponentsArea = buildingCost.Area();

                this.concreteBuildingParams = concreteBuildingParams;
            }

            public IBuildingImage IncompleteBuildingImage(Propor donePropor)
                => concreteBuildingParams.IncompleteBuildingImage(donePropor: donePropor);

            public IIndustry CreateIndustry()
                => new Industry<UnitType, ConcreteParams, UnitType, ConstructionState>
                (
                    productionParams: UnitType.value,
                    buildingParams: this,
                    persistentState: UnitType.value,
                    statsGraphsParams: null
                );

            public IIndustry CreateChildIndustry(ResPile buildingResPile)
                => concreteBuildingParams.CreateIndustry(buildingResPile: buildingResPile);

            public MechProdStats CurConstrStats()
                => ResAndIndustryAlgos.CurConstrStats
                (
                    buildingCost: buildingCost,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature,
                    worldSecondsInGameSecond: CurWorldConfig.worldSecondsInGameSecond
                );

            static bool Industry.IConcreteBuildingParams<UnitType>.RequiresResources
                => true;

            static bool Industry.IConcreteBuildingParams<UnitType>.ProducesResources
                => false;

            IBuildingImage Industry.IConcreteBuildingParams<UnitType>.IdleBuildingImage
                => IncompleteBuildingImage(donePropor: Propor.empty);
            
            SortedResSet<IResource> Industry.IConcreteBuildingParams<UnitType>.GetProducedResources(UnitType productionParams)
                => SortedResSet<IResource>.empty;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<UnitType>.GetConsumedResources(UnitType productionParams)
                => buildingCost.ResSet;

            AllResAmounts Industry.IConcreteBuildingParams<UnitType>.MaxStoredInput(UnitType productionParams)
                => buildingCost;

            IndustryFunctionVisualParams Industry.IConcreteBuildingParams<UnitType>.IndustryFunctionVisualParams(UnitType productionParams)
                => new
                (
                    InputIcons:
                        (from res in buildingCost.ResSet
                         select res.SmallIcon).Append(IIndustry.electricityIcon),
                    OutputIcons: [IIndustry.buildingIcon]
                );
        }

        [Serializable]
        private sealed class ConstructionState : Industry.IProductionCycleState<UnitType, ConcreteParams, UnitType, ConstructionState>
        {
            public static bool IsRepeatable
                => false;

            public static Result<ConstructionState, TextErrors> Create(UnitType productionParams, ConcreteParams parameters, UnitType persistentState,
                ResPile inputStorage, AreaInt storedOutputArea)
            {
                var buildingResPile = ResPile.CreateIfHaveEnough
                (
                    source: inputStorage,
                    amount: parameters.buildingCost
                );
                if (buildingResPile is null)
                    return new(errors: new("not enough resources to start construction"));
                return new(ok: new ConstructionState(buildingResPile: buildingResPile, parameters: parameters));
            }

            public ElectricalEnergy ReqEnergy { get; private set; }
            public bool ShouldRestart
                => false;

            private readonly ConcreteParams parameters;
            private readonly ResPile buildingResPile;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private MechProdStats curConstrStats;
            private Propor donePropor;
            private Result<Propor, TextErrors> workingProporOrPauseReasons;

            private ConstructionState(ResPile buildingResPile, ConcreteParams parameters)
            {
                this.buildingResPile = buildingResPile;
                this.parameters = parameters;
                donePropor = Propor.empty;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: parameters.NodeState.LocationCounters);
            }

            public IBuildingImage BusyBuildingImage()
                => parameters.IncompleteBuildingImage(donePropor: donePropor);

            public void FrameStart()
            {
                curConstrStats = parameters.CurConstrStats();
                ReqEnergy = ResAndIndustryHelpers.CurEnergy<ElectricalEnergy>(watts: curConstrStats.ReqWatts, proporUtilized: Propor.full, elapsed: CurWorldManager.Elapsed);
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingProporOrPauseReasons = ResAndIndustryHelpers.WorkingPropor(proporUtilized: Propor.full, allocatedEnergy: electricalEnergy, reqEnergy: ReqEnergy);
            }

            /// <summary>
            /// Returns child industry if finished construction, null otherwise
            /// </summary>
            public Result<IIndustry?, TextErrors> Update(ResPile outputStorage)
            {
                parameters.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                (donePropor, var pauseReasons) = donePropor.UpdateDonePropor
                (
                    workingProporOrPauseReasons: workingProporOrPauseReasons,
                    producedAreaPerSecOrPauseReasons: curConstrStats.ProducedAreaPerSecOrPauseReasons,
                    elapsed: CurWorldManager.Elapsed,
                    areaInProduction: parameters.buildingComponentsArea
                );

                if (donePropor.IsFull)
                {
                    var childIndustry = parameters.CreateChildIndustry(buildingResPile: buildingResPile);
                    CurWorldManager.PublishMessage
                    (
                        message: new BasicMessage
                        (
                            nodeID: parameters.NodeState.NodeID,
                            message: UIAlgorithms.GetConstructionComplete(buildingNameVisual: childIndustry.NameVisual)
                        )
                    );
                    return new(ok: childIndustry);
                }
                return pauseReasons.Select<IIndustry?>(func: _ => null);
            }

            public void Delete(ResPile outputStorage)
            {
                parameters.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                outputStorage.TransferAllFrom(source: buildingResPile);
            }

            public static void DeletePersistentState(UnitType persistentState, ResPile outputStorage)
            { }
        }

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Industry<UnitType, ConcreteParams, UnitType, ConstructionState>)
            };
    }
}
