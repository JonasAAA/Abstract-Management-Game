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
    public static class LightRedirection
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
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.lightRedirectionBuildingColor);
                NameVisual = UIAlgorithms.GetBuildingNameVisual(name: name);
                BuildingCostPropors = new BuildingCostPropors(ingredProdToAmounts: buildingComponentPropors);

                this.energyPriority = energyPriority;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            IHUDElement? IGeneralBuildingConstructionParams.CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                => null;

            public IConcreteBuildingConstructionParams CreateConcrete(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices)
            {
                if (!BuildingCostPropors.neededProductClasses.SetEquals(neededBuildingMatPaletteChoices.Choices.Keys))
                    throw new ArgumentException();

                return new ConcreteBuildingParams
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
            }

            IConcreteBuildingConstructionParams IGeneralBuildingConstructionParams.CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
                => CreateConcrete
                (
                    nodeState: nodeState,
                    neededBuildingMatPaletteChoices: neededBuildingMatPaletteChoices
                );

            IndustryFunctionVisualParams IGeneralBuildingConstructionParams.IncompleteFunctionVisualParams(ProductionChoice? productionChoice)
                => IncompleteFunctionVisualParams();
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<LightRedirectionParams>, IConcreteBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public readonly DiskBuildingImage buildingImage;

            private readonly AreaDouble buildingArea;
            private readonly BuildingCostPropors buildingCostPropors;
            private readonly MaterialPaletteChoices buildingMatPaletteChoices;
            private readonly AllResAmounts buildingCost;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA,
                MaterialPaletteChoices buildingMatPaletteChoices)
            {
                NameVisual = generalParams.NameVisual;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                EnergyPriority = generalParams.energyPriority;

                buildingArea = buildingImage.Area;
                buildingCostPropors = generalParams.BuildingCostPropors;
                this.buildingMatPaletteChoices = buildingMatPaletteChoices;

                buildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: buildingArea);
            }

            public LightRedirectionProdStats CurProdStats()
                => ResAndIndustryAlgos.CurLightRedirectionProdStats
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature,
                    buildingArea: buildingArea
                );

            AllResAmounts IConcreteBuildingConstructionParams.BuildingCost
                => buildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
            {
                var statsGraphsParams = (buildingMatPaletteChoices, buildingCostPropors);
                LightRedirectionParams lightRedirectionParams = new(targetCosmicBody: null);
                return new Industry<LightRedirectionParams, ConcreteBuildingParams, ResPile, LightRedirectionState>
                (
                    productionParams: lightRedirectionParams,
                    buildingParams: this,
                    persistentState: buildingResPile,
                    statsGraphsParams: statsGraphsParams,
                    customHUDElement: ResAndIndustryUIAlgos.CreateTargetCosmicBodyChoiceButton
                    (
                        targetChoiceSetter: new TargetChoiceSetter(lightRedirectionParams: lightRedirectionParams),
                        originCosmicBody: NodeState.NodeID,
                        buttonText: UIAlgorithms.ChooseTargetCosmicBody,
                        tooltipText: UIAlgorithms.ChooseTargetCosmicBodyTooltip,
                        chooseThisAsTarget: UIAlgorithms.ChooseThisAsTargetCosmicBody,
                        chooseThisAsTargetTooltip: UIAlgorithms.ChooseThisAsTargetCosmicBodyTooltip
                    )
                );
            }

            static bool Industry.IConcreteBuildingParams<LightRedirectionParams>.RequiresResources
                => false;

            static bool Industry.IConcreteBuildingParams<LightRedirectionParams>.ProducesResources
                => false;

            IBuildingImage Industry.IConcreteBuildingParams<LightRedirectionParams>.IdleBuildingImage
                => buildingImage;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<LightRedirectionParams>.GetProducedResources(LightRedirectionParams productionParams)
                => SortedResSet<IResource>.empty;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<LightRedirectionParams>.GetConsumedResources(LightRedirectionParams productionParams)
                => SortedResSet<IResource>.empty;

            AllResAmounts Industry.IConcreteBuildingParams<LightRedirectionParams>.MaxStoredInput(LightRedirectionParams productionParams)
                => AllResAmounts.empty;

            IndustryFunctionVisualParams Industry.IConcreteBuildingParams<LightRedirectionParams>.IndustryFunctionVisualParams(LightRedirectionParams productionParams)
                => IncompleteFunctionVisualParams();
        }

        [Serializable]
        public sealed class LightRedirectionParams
        {
            public NodeID? targetCosmicBody;

            public LightRedirectionParams(NodeID? targetCosmicBody)
                => this.targetCosmicBody = targetCosmicBody;
        }

        [Serializable]
        private sealed class TargetChoiceSetter(LightRedirectionParams lightRedirectionParams) : IItemChoiceSetter<NodeID>
        {
            void IItemChoiceSetter<NodeID>.SetChoice(NodeID item)
                => lightRedirectionParams.targetCosmicBody = item;
        }

        [Serializable]
        private sealed class LightRedirectionState : Industry.IProductionCycleState<LightRedirectionParams, ConcreteBuildingParams, ResPile, LightRedirectionState>
        {
            public static bool IsRepeatable
                => false;

            public static Result<LightRedirectionState, TextErrors> Create(LightRedirectionParams productionParams, ConcreteBuildingParams parameters, ResPile buildingResPile,
                ResPile inputStorage, AreaInt storedOutputArea)
                => new(ok: new(buildingParams: parameters, lightRedirectionParams: productionParams));

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool ShouldRestart
                => false;

            private readonly ConcreteBuildingParams buildingParams;
            private readonly LightRedirectionParams lightRedirectionParams;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;

            private RadiantEnergy energyToRedirectIfGetRequiredElectricity;
            private Result<NodeID, TextErrors> targetCosmicBody;
            private Result<RadiantEnergy, TextErrors> energyToRedirect;

            private LightRedirectionState(ConcreteBuildingParams buildingParams, LightRedirectionParams lightRedirectionParams)
            {
                this.buildingParams = buildingParams;
                this.lightRedirectionParams = lightRedirectionParams;
                energyToRedirectIfGetRequiredElectricity = RadiantEnergy.zero;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
            }

            public IBuildingImage BusyBuildingImage()
                => buildingParams.buildingImage;

            public Propor FrameStartAndReturnThroughputUtilization()
            {
                if (lightRedirectionParams.targetCosmicBody is NodeID nodeID)
                {
                    targetCosmicBody = new(ok: nodeID);
                    var curProdStats = buildingParams.CurProdStats();
                    var maxEnergyToRedirect = ResAndIndustryHelpers.CurEnergy<RadiantEnergy>
                    (
                        watts: curProdStats.RedirectWatts,
                        proporUtilized: Propor.full,
                        elapsed: CurWorldManager.Elapsed
                    );
                    energyToRedirectIfGetRequiredElectricity = MyMathHelper.Min
                    (
                        buildingParams.NodeState.RadiantEnergyPile.Amount,
                        maxEnergyToRedirect
                    );

                    // Can be more than 1 if this building is extremely inefficient
                    var proporOfEnergyToRedirectRequired = curProdStats.ReqWatts / curProdStats.RedirectWatts;
                    ReqEnergy = ElectricalEnergy.CreateFromJoules
                    (
                        valueInJ: Algorithms.ScaleEnergy
                        (
                            amount: energyToRedirectIfGetRequiredElectricity,
                            scale: proporOfEnergyToRedirectRequired
                        ).ValueInJ
                    );

                    return Propor.Create
                    (
                        part: energyToRedirectIfGetRequiredElectricity.ValueInJ,
                        whole: maxEnergyToRedirect.ValueInJ
                    )!.Value;
                }
                else
                {
                    targetCosmicBody = new(errors: new("No target chosen"));
                    energyToRedirectIfGetRequiredElectricity = RadiantEnergy.zero;
                    ReqEnergy = ElectricalEnergy.zero;
                    return Propor.empty;
                }
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                Debug.Assert(electricalEnergy <= ReqEnergy);
                energyToRedirect = (ReqEnergy.IsZero, electricalEnergy.IsZero) switch
                {
                    (true, true) => new(ok: energyToRedirectIfGetRequiredElectricity),
                    (true, false) => throw new InvalidStateException(),
                    (false, true) => new(errors: new(UIAlgorithms.GotNoElectricity)),
                    (false, false) => new
                    (
                        ok: Algorithms.ScaleEnergy
                        (
                            amount: energyToRedirectIfGetRequiredElectricity,
                            scale: (UDouble)electricalEnergy.ValueInJ / ReqEnergy.ValueInJ
                        )
                    )
                };
            }

            public Result<IIndustry?, TextErrors> Update(ResPile outputStorage)
            {
                buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                return targetCosmicBody.SwitchExpression
                (
                    ok: target => energyToRedirect.Select<IIndustry?>
                    (
                        func: energyToRedirect =>
                        {
                            var lightPile = buildingParams.NodeState.LaserToShine?.lightPile ?? EnergyPile<RadiantEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                            lightPile.TransferFrom(source: buildingParams.NodeState.RadiantEnergyPile, amount: energyToRedirect);
                            buildingParams.NodeState.LaserToShine =
                            (
                                lightPile: lightPile,
                                lightPerSec: lightPile.Amount.ValueInJ / (UDouble)CurWorldManager.Elapsed.TotalSeconds,
                                targetCosmicBody: target
                            );
                            return null;
                        }
                    ),
                    error: errors =>
                    {
                        buildingParams.NodeState.LaserToShine = null;
                        return new(errors: errors);
                    }
                );
            }

            public void Delete(ResPile outputStorage)
                => buildingParams.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

            public static void DeletePersistentState(ResPile buildingResPile, ResPile outputStorage)
                => outputStorage.TransferAllFrom(source: buildingResPile);
        }

        private static IndustryFunctionVisualParams IncompleteFunctionVisualParams()
             => new
             (
                 InputIcons: [IIndustry.starlightIcon],
                 OutputIcons: [IIndustry.starlightIcon]
             );

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Industry<LightRedirectionParams, ConcreteBuildingParams, ResPile, LightRedirectionState>)
            };
    }
}
