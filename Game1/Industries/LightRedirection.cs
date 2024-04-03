using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class LightRedirection : IIndustry
    {
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                NameVisual = UIAlgorithms.GetBuildingNameVisual(name: name);
                BuildingCostPropors = new(ingredProdToAmounts: buildingComponentPropors);

                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.lightRedirectionBuildingColor);
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public IHUDElement? CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                // Should not require target when the building is yet to be constructed.
                // As then all planets would show "Build here" AND "Choose this as target" buttons at the same time.
                => null;

            public ConcreteBuildingParams CreateConcrete(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, NodeID? targetCosmicBody)
            {
                if (!BuildingCostPropors.neededProductClasses.SetEquals(neededBuildingMatPaletteChoices.Choices.Keys))
                    throw new ArgumentException();

                return new
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
                    targetCosmicBody: targetCosmicBody
                );
            }

            IConcreteBuildingConstructionParams IGeneralBuildingConstructionParams.CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
                => CreateConcrete(nodeState: nodeState, neededBuildingMatPaletteChoices: neededBuildingMatPaletteChoices, targetCosmicBody: null);
            
            IndustryFunctionVisualParams IGeneralBuildingConstructionParams.IncompleteFunctionVisualParams(ProductionChoice? productionChoice)
                => IncompleteFunctionVisualParams();
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : IConcreteBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public AllResAmounts BuildingCost { get; }
            public readonly DiskBuildingImage buildingImage;
            public readonly BuildingCostPropors buildingCostPropors;
            public readonly MaterialPaletteChoices buildingMatPaletteChoices;

            private readonly AreaDouble buildingArea;
            private readonly NodeID? targetCosmicBody;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA,
                MaterialPaletteChoices buildingMatPaletteChoices, NodeID? targetCosmicBody)
            {
                NameVisual = generalParams.NameVisual;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                // Building area is used in BuildingCost calculation, thus needs to be computed first
                buildingArea = buildingImage.Area;
                buildingCostPropors = generalParams.BuildingCostPropors;
                this.buildingMatPaletteChoices = buildingMatPaletteChoices;
                this.targetCosmicBody = targetCosmicBody;
                BuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: buildingArea);
            }

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new LightRedirection
                (
                    lightRedirectionParams: new(targetCosmicBody: targetCosmicBody),
                    buildingParams: this,
                    buildingResPile: buildingResPile
                );
        }

        [Serializable]
        public sealed class LightRedirectionParams
        {
            public NodeID? targetCosmicBody;

            public LightRedirectionParams(NodeID? targetCosmicBody)
                => this.targetCosmicBody = targetCosmicBody;
        }

        [Serializable]
        private sealed class TargetChoiceSetter(LightRedirection lightRedirection) : IItemChoiceSetter<NodeID>
        {
            void IItemChoiceSetter<NodeID>.SetChoice(NodeID item)
                => lightRedirection.lightRedirectionParams.targetCosmicBody = item;
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
                typeof(LightRedirection)
            };

        public IFunction<IHUDElement> NameVisual
            => buildingParams.NameVisual;

        public NodeID NodeID
            => buildingParams.NodeState.NodeID;

        public IHUDElement UIElement
            => lightRedirectionUI;

        public IEvent<IDeletedListener> Deleted
            => deleted;

        public IBuildingImage BuildingImage
            => buildingParams.buildingImage;

        public IHUDElement RoutePanel { get; }

        public IHUDElement? IndustryFunctionVisual { get; }

        private readonly LightRedirectionParams lightRedirectionParams;
        private readonly ConcreteBuildingParams buildingParams;
        private readonly ResPile buildingResPile;
        private readonly Event<IDeletedListener> deleted;
        private bool isDeleted;
        private readonly UIRectVertPanel<IHUDElement> lightRedirectionUI;

        private LightRedirection(LightRedirectionParams lightRedirectionParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile)
        {
            this.lightRedirectionParams = lightRedirectionParams;
            this.buildingParams = buildingParams;
            this.buildingResPile = buildingResPile;
            deleted = new();
            isDeleted = false;

            RoutePanel = IIndustry.CreateRoutePanel(industry: this);
            IndustryFunctionVisual = IncompleteFunctionVisualParams().CreateIndustryFunctionVisual();

            lightRedirectionUI = new
            (
                childHorizPos: HorizPosEnum.Left,
                children:
                [
                    buildingParams.NameVisual.Invoke(),
                    ResAndIndustryUIAlgos.CreateBuildingStatsHeaderRow(),
                    ResAndIndustryUIAlgos.CreateNeededElectricityAndThroughputPanel
                    (
                        nodeState: buildingParams.NodeState,
                        neededElectricityGraph: ResAndIndustryUIAlgos.CreateGravityFunctionGraph
                        (
                            func: gravity => ResAndIndustryAlgos.TentativeNeededElectricity
                            (
                                gravity: gravity,
                                chosenTotalPropor: Propor.full,
                                matPaletteChoices: buildingParams.buildingMatPaletteChoices.Choices,
                                buildingProdClassPropors: buildingParams.buildingCostPropors.neededProductClassPropors
                            )
                        ),
                        throughputGraph: ResAndIndustryUIAlgos.CreateTemperatureFunctionGraph
                        (
                            func: temperature => ResAndIndustryAlgos.TentativeThroughput
                            (
                                temperature: temperature,
                                chosenTotalPropor: Propor.full,
                                matPaletteChoices: buildingParams.buildingMatPaletteChoices.Choices,
                                buildingProdClassPropors: buildingParams.buildingCostPropors.neededProductClassPropors
                            )
                        )
                    ),
                    new TextBox(text: "THIS BUILDING DOES NOT\nDEPEND ON THE ABOVE CURRENTLY"),
                    ResAndIndustryUIAlgos.CreateTargetCosmicBodyChoiceButton
                    (
                        targetChoiceSetter: new TargetChoiceSetter(lightRedirection: this),
                        originCosmicBody: buildingParams.NodeState.NodeID,
                        buttonText: UIAlgorithms.ChooseTargetCosmicBody,
                        tooltipText: UIAlgorithms.ChooseTargetCosmicBodyTooltip,
                        chooseThisAsTarget: UIAlgorithms.ChooseThisAsTargetCosmicBody,
                        chooseThisAsTargetTooltip: UIAlgorithms.ChooseThisAsTargetCosmicBodyTooltip
                    )
                ]
            );
        }

        public bool IsNeighborhoodPossible(NeighborDir neighborDir, IResource resource)
            => false;

        public IReadOnlyCollection<IResource> GetResWithPotentialNeighborhood(NeighborDir neighborDir)
            => [];

        public EfficientReadOnlyHashSet<IIndustry> GetResNeighbors(NeighborDir neighborDir, IResource resource)
            => [];

        public AllResAmounts GetResAmountsRequestToNeighbors(NeighborDir neighborDir)
            => AllResAmounts.empty;

        private const string mustTransportNothingMessage = "Light Redirection building must transport nothing";

        public void TransportResTo(IIndustry destinIndustry, ResAmount<IResource> resAmount)
        {
            if (resAmount.amount is not 0)
                throw new ArgumentException(mustTransportNothingMessage);
        }

        public void WaitForResFrom(IIndustry sourceIndustry, ResAmount<IResource> resAmount)
        {
            if (resAmount.amount is not 0)
                throw new ArgumentException(mustTransportNothingMessage);
        }

        public void Arrive(ResPile arrivingResPile)
        {
            if (!arrivingResPile.IsEmpty)
                throw new ArgumentException(mustTransportNothingMessage);
        }

        public void ToggleResNeighbor(NeighborDir neighborDir, IResource resource, IIndustry neighbor)
            => throw new InvalidOperationException(mustTransportNothingMessage);

        public void FrameStart()
        { }

        public IIndustry? UpdateImpl()
        {
            if (lightRedirectionParams.targetCosmicBody is NodeID target)
            {
                var lightPile = buildingParams.NodeState.LaserToShine?.lightPile ?? EnergyPile<RadiantEnergy>.CreateEmpty(locationCounters: buildingParams.NodeState.LocationCounters);
                lightPile.TransferAllFrom(buildingParams.NodeState.RadiantEnergyPile);
                buildingParams.NodeState.LaserToShine =
                (
                    lightPile: lightPile,
                    lightPerSec: lightPile.Amount.ValueInJ / (UDouble)CurWorldManager.Elapsed.TotalSeconds,
                    targetCosmicBody: target
                );
            }
            else
                buildingParams.NodeState.LaserToShine = null;
            return this;
        }

        public void UpdateUI()
        { }

        public bool Delete()
        {
            if (isDeleted)
                return false;
            IIndustry.DeleteResNeighbors(industry: this);
#warning Implement a proper industry deletion strategy
            IIndustry.DumpAllResIntoCosmicBody(nodeState: buildingParams.NodeState, resPile: buildingResPile);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
            isDeleted = true;
            return true;
        }
    }
}
