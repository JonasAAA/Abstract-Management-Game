using Game1.Delegates;
using Game1.Industries;
using Game1.Shapes;
using Game1.UI;
using Game1.Collections;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;
using static Game1.GameConfig;

namespace Game1
{
    [Serializable]
    public sealed record BuildIndustryButtonClickedListener(EfficientReadOnlyCollection<CosmicBody> CosmicBodies, Construction.GeneralParams ConstrGeneralParams) : IClickedListener
    {
        [Serializable]
        private sealed class BuildingConfigPanelManager : IItemChoiceSetter<MaterialPalette>, IItemChoiceSetter<ProductionChoice>
        {
            public static void StartBuildingConfig(EfficientReadOnlyCollection<CosmicBody> cosmicBodies, Construction.GeneralParams constrGeneralParams)
            {
                CurWorldManager.DeactivateWorldElements();
                CurWorldManager.DisableAllUIElements();
                BuildingConfigPanelManager buildingConfigPanelManager = new(cosmicBodies: cosmicBodies, constrGeneralParams: constrGeneralParams);
                CurWorldManager.AddHUDElement
                (
                    HUDElement: buildingConfigPanelManager.buildingConfigPanel,
                    position: new(HorizPosEnum.Right, VertPosEnum.Top)
                );
                foreach (var cosmicBodyBuildPanelManager in buildingConfigPanelManager.cosmicBodyBuildPanelManagers)
                    CurWorldManager.AddWorldHUDElement
                    (
                        worldHUDElement: cosmicBodyBuildPanelManager.CosmicBodyBuildPanel,
                        updateHUDPos: cosmicBodyBuildPanelManager.CosmicBodyPanelHUDPosUpdate
                    );
            }

            public CompleteBuildingConfig? CompleteBuildingConfigOrNull { get; private set; }

            private readonly Construction.GeneralParams constrGeneralParams;
            private readonly UIRectVertPanel<IHUDElement> buildingConfigPanel;
            private readonly List<CosmicBodyBuildPanelManager> cosmicBodyBuildPanelManagers;
            private readonly Dictionary<ProductClass, MaterialPalette> mutableBuildingMatPaletteChoices;
            private readonly Button cancelButton;
            private ProductionChoice? ProductionChoice;
            private readonly FunctionGraphImage<SurfaceGravity, Propor> overallNeededElectricityGraph;
            private readonly FunctionGraphImage<Temperature, Propor> overallThroughputGraph;

            private BuildingConfigPanelManager(EfficientReadOnlyCollection<CosmicBody> cosmicBodies, Construction.GeneralParams constrGeneralParams)
            {
                this.constrGeneralParams = constrGeneralParams;
                mutableBuildingMatPaletteChoices = new();
                ProductionChoice = null;
                CompleteBuildingConfigOrNull = null;

                cancelButton = new
                (
                    shape: new MyRectangle(width: CurGameConfig.standardUIElementWidth, height: CurGameConfig.UILineHeight),
                    tooltip: new ImmutableTextTooltip(text: UIAlgorithms.CancelBuilding),
                    text: "Cancel",
                    color: colorConfig.deleteButtonColor
                );

                // Need to initialize all references so that when this gets copied, the fields are already initialized
                buildingConfigPanel = new
                (
                    childHorizPos: HorizPosEnum.Right,
                    children: Enumerable.Empty<IHUDElement>()
                );
                cosmicBodyBuildPanelManagers = new();
                cancelButton.clicked.Add(listener: new CancelBuildingButtonListener(BuildingConfigPanelManager: this));

                overallNeededElectricityGraph = IndustryUIAlgos.CreateGravityFunctionGraph(func: null);
                overallThroughputGraph = IndustryUIAlgos.CreateTemperatureFunctionGraph(func: null);

                var productionChoicePanel = constrGeneralParams.CreateProductionChoicePanel(productionChoiceSetter: this);
                if (productionChoicePanel is null)
                    SetProductionChoice(productionChoice: new ProductionChoice(Choice: new UnitType()));

                buildingConfigPanel.Reinitialize
                (
                    newChildren: new List<IHUDElement>
                    {
                        new TextBox(text: "Material Choices"),
                        new UIRectHorizPanel<IHUDElement>
                        (
                            childVertPos: VertPosEnum.Middle,
                            children: new List<IHUDElement>()
                            {
                                new TextBox(text: "needed\nelectricity"),
                                new TextBox(text: "throughput")
                            }
                        )
                    }.Concat
                    (
                        constrGeneralParams.neededProductClassPropors.Select
                        (
                            args =>
                            {
                                var (productClass, propor) = args;
                                return new UIRectHorizPanel<IHUDElement>
                                (
                                    childVertPos: VertPosEnum.Middle,
                                    children: new List<IHUDElement>()
                                    {
                                        new TextBox(text: $"{productClass} "),
                                        IndustryUIAlgos.CreateMatPaletteChoiceDropdown
                                        (
                                            matPaletteChoiceSetter: this,
                                            productClass: productClass,
                                            additionalInfos:
                                            (
                                                empty: MaterialPalette.CreateEmptyProdStatsInfluenceVisual(),
                                                item: static matPalette => matPalette.CreateProdStatsInfluenceVisual()
                                            )
                                        ),
                                        IndustryUIAlgos.CreateStandardVertProporBar(propor: propor)
                                    }
                                );
                            }
                        )
                    ).Append
                    (
                        new UIRectHorizPanel<IHUDElement>
                        (
                            childVertPos: VertPosEnum.Middle,
                            children: new List<IHUDElement>()
                            {
                                IndustryUIAlgos.CreateNeededElectricityAndThroughputPanel
                                (
                                    neededElectricity: overallNeededElectricityGraph,
                                    throughput: overallThroughputGraph
                                ),
                                IndustryUIAlgos.CreateStandardVertProporBar(propor: Propor.full)
                            }
                        )
                    ).Concat
                    (
                        new List<IHUDElement>()
                        {
                            new TextBox(text: "Production config"),
                            productionChoicePanel ?? new TextBox(text: UIAlgorithms.NothingToConfigure),
                            cancelButton
                        }
                    )
                );

                cosmicBodyBuildPanelManagers.AddRange
                (
                    collection: cosmicBodies.Where
                    (
                        cosmicBody => !cosmicBody.HasIndustry
                    ).Select
                    (
                        cosmicBody =>
                        {
                            IHUDElement buildingStatsGraphs = IndustryUIAlgos.CreateNeededElectricityAndThroughputPanel
                            (
                                neededElectricity: new FunctionGraphWithHighlighImage<SurfaceGravity, Propor>
                                (
                                    functionGraph: overallNeededElectricityGraph,
                                    highlightInterval: new CosmicBodyGravityInterval(CosmicBody: cosmicBody)
                                ),
                                throughput: new FunctionGraphWithHighlighImage<Temperature, Propor>
                                (
                                    functionGraph: overallThroughputGraph,
                                    highlightInterval: new CosmicBodyTemperatureInterval(CosmicBody: cosmicBody)
                                )
                            );
                            Button buildButton = new
                            (
                                shape: new MyRectangle(width: CurGameConfig.standardUIElementWidth, height: CurGameConfig.UILineHeight),
                                tooltip: new ImmutableTextTooltip(text: UIAlgorithms.BuildHereTooltip),
                                text: "Build here"
                            )
                            {
                                PersonallyEnabled = false
                            };
                            buildButton.clicked.Add
                            (
                                listener: new BuildOnCosmicBodyButtonListener
                                (
                                    BuildingConfigPanelManager: this,
                                    CosmicBody: cosmicBody
                                )
                            );
                            UIRectVertPanel<IHUDElement> cosmicBodyBuildPanel = new
                            (
                                childHorizPos: HorizPosEnum.Left,
                                children: new List<IHUDElement>()
                                {
                                    buildingStatsGraphs,
                                    buildButton
                                }
                            );
                            return new CosmicBodyBuildPanelManager
                            (
                                CosmicBody: cosmicBody,
                                CosmicBodyBuildPanel: cosmicBodyBuildPanel,
                                CosmicBodyPanelHUDPosUpdate: new HUDElementPosUpdater
                                (
                                    HUDElement: cosmicBodyBuildPanel,
                                    baseWorldObject: cosmicBody,
                                    HUDElementOrigin: new(HorizPosEnum.Middle, VertPosEnum.Top),
                                    anchorInBaseWorldObject: new(HorizPosEnum.Middle, VertPosEnum.Middle)
                                ),
                                BuildButton: buildButton
                            );
                        }
                    )
                );
            }

            void IItemChoiceSetter<ProductionChoice>.SetChoice(ProductionChoice item)
                => SetProductionChoice(productionChoice: item);

            private void SetProductionChoice(ProductionChoice productionChoice)
            {
                ProductionChoice = productionChoice;
                UpdateCompleteBuildingConfigOrNull();
            }

            void IItemChoiceSetter<MaterialPalette>.SetChoice(MaterialPalette item)
                => SetMatPaletteChoice(materialPalette: item);

            private void SetMatPaletteChoice(MaterialPalette materialPalette)
            {
                mutableBuildingMatPaletteChoices[materialPalette.productClass] = materialPalette;
#warning Complete this. In case all material choices are made, show player the stats of the to-be-constructed building
                UpdateCompleteBuildingConfigOrNull();
            }

            private void UpdateCompleteBuildingConfigOrNull()
            {
                CompleteBuildingConfigOrNull = CompleteBuildingConfig.Create
                (
                    constrGeneralParams: constrGeneralParams,
                    buildingMatPaletteChoices: MaterialPaletteChoices.CreateOrThrow
                    (
                        choices: new(dict: mutableBuildingMatPaletteChoices)
                    ),
                    ProductionChoice: ProductionChoice
                );

                var chosenTotalPropor = (Propor)mutableBuildingMatPaletteChoices.Keys.Sum(prodClass => (UDouble)constrGeneralParams.neededProductClassPropors[prodClass]);

                overallNeededElectricityGraph.SetFunction
                (
                    func: mutableBuildingMatPaletteChoices.Count switch
                    {
                        0 => null,
                        not 0 => gravity => ResAndIndustryAlgos.TentativeNeededElectricity
                        (
                            gravity: gravity,
                            chosenTotalPropor: chosenTotalPropor,
                            matPaletteChoices: new(dict: mutableBuildingMatPaletteChoices),
                            buildingProdClassPropors: constrGeneralParams.neededProductClassPropors
                        )
                    }
                );

                overallThroughputGraph.SetFunction
                (
                    func: mutableBuildingMatPaletteChoices.Count switch
                    {
                        0 => null,
                        not 0 => temperature => ResAndIndustryAlgos.TentativeThroughput
                        (
                            temperature: temperature,
                            chosenTotalPropor: chosenTotalPropor,
                            matPaletteChoices: new(dict: mutableBuildingMatPaletteChoices),
                            buildingProdClassPropors: constrGeneralParams.neededProductClassPropors
                        )
                    }
                );

                foreach (var cosmicBodyPanelManager in cosmicBodyBuildPanelManagers)
                    cosmicBodyPanelManager.BuildButton.PersonallyEnabled = CompleteBuildingConfigOrNull is not null;
            }

            public void StopBuildingConfig()
            {
                CurWorldManager.RemoveHUDElement(HUDElement: buildingConfigPanel);
                foreach (var cosmicBodyPanelManager in cosmicBodyBuildPanelManagers)
                    CurWorldManager.RemoveWorldHUDElement(worldHUDElement: cosmicBodyPanelManager.CosmicBodyBuildPanel);
                CurWorldManager.EnableAllUIElements();
            }
        }

        [Serializable]
        private sealed record CancelBuildingButtonListener(BuildingConfigPanelManager BuildingConfigPanelManager) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => BuildingConfigPanelManager.StopBuildingConfig();
        }

        [Serializable]
        private sealed record CosmicBodyGravityInterval(CosmicBody CosmicBody) : FunctionGraphWithHighlighImage<SurfaceGravity, Propor>.IHighlightInterval
        {
            (SurfaceGravity start, SurfaceGravity stop, Color highlightColor) FunctionGraphWithHighlighImage<SurfaceGravity, Propor>.IHighlightInterval.GetHighlightInterval()
                =>
                (
                    start: CosmicBody.NodeState.SurfaceGravity,
                    stop: CosmicBody.NodeState.SurfaceGravity,
                    highlightColor: colorConfig.functionGraphHighlightColor
                );
        }

        [Serializable]
        private sealed record CosmicBodyTemperatureInterval(CosmicBody CosmicBody) : FunctionGraphWithHighlighImage<Temperature, Propor>.IHighlightInterval
        {
            (Temperature start, Temperature stop, Color highlightColor) FunctionGraphWithHighlighImage<Temperature, Propor>.IHighlightInterval.GetHighlightInterval()
                =>
                (
                    start: CosmicBody.NodeState.Temperature,
                    stop: CosmicBody.NodeState.Temperature,
                    highlightColor: colorConfig.functionGraphHighlightColor
                );
        }

        [Serializable]
        private readonly record struct CosmicBodyBuildPanelManager(CosmicBody CosmicBody, UIRectVertPanel<IHUDElement> CosmicBodyBuildPanel, IAction CosmicBodyPanelHUDPosUpdate, Button BuildButton);

        [Serializable]
        private readonly record struct CompleteBuildingConfig
        {
            public static CompleteBuildingConfig? Create(Construction.GeneralParams constrGeneralParams, MaterialPaletteChoices buildingMatPaletteChoices, ProductionChoice? ProductionChoice)
                => (ProductionChoice, constrGeneralParams.SufficientBuildingMatPalettes(curBuildingMatPaletteChoices: buildingMatPaletteChoices)) switch
                {
                    (ProductionChoice productionChoice, true) => new CompleteBuildingConfig
                    (
                        constrGeneralParams: constrGeneralParams,
                        neededBuildingMatPaletteChoices: buildingMatPaletteChoices,
                        productionChoice: productionChoice
                    ),
                    _ => null
                };

            private readonly Construction.GeneralParams constrGeneralParams;
            private readonly MaterialPaletteChoices neededBuildingMatPaletteChoices;
            private readonly ProductionChoice productionChoice;

            private CompleteBuildingConfig(Construction.GeneralParams constrGeneralParams, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
            {
                this.constrGeneralParams = constrGeneralParams;
                this.neededBuildingMatPaletteChoices = neededBuildingMatPaletteChoices;
                this.productionChoice = productionChoice;
            }

            public Construction.ConcreteParams CreateConcreteConstrParams(CosmicBody cosmicBody)
                => constrGeneralParams.CreateConcrete
                (
                    nodeState: cosmicBody.NodeState,
                    neededBuildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                    productionChoice: productionChoice
                );
        }

        [Serializable]
        private sealed record BuildOnCosmicBodyButtonListener(BuildingConfigPanelManager BuildingConfigPanelManager, CosmicBody CosmicBody) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                CosmicBody.StartConstruction
                (
                    constrConcreteParams: BuildingConfigPanelManager.CompleteBuildingConfigOrNull!.Value.CreateConcreteConstrParams(cosmicBody: CosmicBody)
                );
                BuildingConfigPanelManager.StopBuildingConfig();
            }
        }

        void IClickedListener.ClickedResponse()
            => BuildingConfigPanelManager.StartBuildingConfig(cosmicBodies: CosmicBodies, constrGeneralParams: ConstrGeneralParams);
    }   
}
