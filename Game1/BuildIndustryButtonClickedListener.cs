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
    public sealed class BuildIndustryButtonClickedListener(EfficientReadOnlyCollection<CosmicBody> cosmicBodies, Construction.GeneralParams constrGeneralParams) : IClickedListener
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
                foreach (var cosmicBodyBuildPanelManager in buildingConfigPanelManager.cosmicBodyBuildPanelManagers.Values)
                    CurWorldManager.AddWorldHUDElement
                    (
                        worldHUDElement: cosmicBodyBuildPanelManager.CosmicBodyBuildPanel,
                        updateHUDPos: cosmicBodyBuildPanelManager.CosmicBodyPanelHUDPosUpdate
                    );

                CurWorldManager.SetOneUseClickedNowhereResponse(new StopBuildingConfig(buildingConfigPanelManager: buildingConfigPanelManager));
            }

            public CompleteBuildingConfig? CompleteBuildingConfigOrNull { get; private set; }

            private readonly Construction.GeneralParams constrGeneralParams;
            private readonly UIRectVertPanel<IHUDElement> buildingConfigPanel;
            private readonly Dictionary<CosmicBody, CosmicBodyBuildPanelManager> cosmicBodyBuildPanelManagers;
            private readonly Dictionary<ProductClass, MaterialPalette> mutableBuildingMatPaletteChoices;
            private readonly Button<TextBox> doneButton;
            private ProductionChoice? ProductionChoice;
            private readonly FunctionGraphImage<SurfaceGravity, Propor> overallNeededElectricityGraph;
            private readonly FunctionGraphImage<Temperature, Propor> overallThroughputGraph;

            private BuildingConfigPanelManager(EfficientReadOnlyCollection<CosmicBody> cosmicBodies, Construction.GeneralParams constrGeneralParams)
            {
                this.constrGeneralParams = constrGeneralParams;
                mutableBuildingMatPaletteChoices = [];
                ProductionChoice = null;
                CompleteBuildingConfigOrNull = null;

                doneButton = new
                (
                    shape: new MyRectangle(width: CurGameConfig.standardUIElementWidth, height: CurGameConfig.UILineHeight),
                    visual: new TextBox(text: "Done", textColor: colorConfig.buttonTextColor),
                    tooltip: new ImmutableTextTooltip(text: UIAlgorithms.DoneChoosingNewBuildings)
                );

                // Need to initialize all references so that when this gets copied, the fields are already initialized
                buildingConfigPanel = new
                (
                    childHorizPos: HorizPosEnum.Right,
                    children: Enumerable.Empty<IHUDElement>()
                );
                cosmicBodyBuildPanelManagers = [];
                doneButton.clicked.Add(listener: new DoneBuildingButtonListener(buildingConfigPanelManager: this));

                overallNeededElectricityGraph = ResAndIndustryUIAlgos.CreateGravityFunctionGraph(func: null);
                overallThroughputGraph = ResAndIndustryUIAlgos.CreateTemperatureFunctionGraph(func: null);

                var productionChoicePanel = constrGeneralParams.CreateProductionChoicePanel(productionChoiceSetter: this);
                if (productionChoicePanel is null)
                    SetProductionChoice(productionChoice: new ProductionChoice(Choice: new UnitType()));

                buildingConfigPanel.Reinitialize
                (
                    newChildren: new List<IHUDElement>
                    {
                        new TextBox(text: "Material Choices"),
                        ResAndIndustryUIAlgos.CreateBuildingStatsHeaderRow()
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
                                        ResAndIndustryUIAlgos.CreateMatPaletteChoiceDropdown
                                        (
                                            matPaletteChoiceSetter: this,
                                            productClass: productClass,
                                            additionalInfos:
                                            (
                                                empty: MaterialPalette.CreateEmptyProdStatsInfluenceVisual(),
                                                item: static matPalette => matPalette.CreateProdStatsInfluenceVisual()
                                            )
                                        ),
                                        ResAndIndustryUIAlgos.CreateStandardVertProporBar(propor: propor)
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
                                ResAndIndustryUIAlgos.CreateNeededElectricityAndThroughputPanel
                                (
                                    neededElectricityGraph: overallNeededElectricityGraph,
                                    throughputGraph: overallThroughputGraph,
                                    nodeState: null
                                ),
                                ResAndIndustryUIAlgos.CreateStandardVertProporBar(propor: Propor.full)
                            }
                        )
                    ).Concat
                    (
                        new List<IHUDElement>()
                        {
                            new TextBox(text: "Production config"),
                            productionChoicePanel ?? new TextBox(text: UIAlgorithms.NothingToConfigure),
                            doneButton
                        }
                    )
                );

                var addCosmicBodyBuildPanelManagers = cosmicBodies.Where
                (
                    cosmicBody => !cosmicBody.HasIndustry
                ).Select
                (
                    cosmicBody =>
                    {
                        IHUDElement buildingStatsGraphs = ResAndIndustryUIAlgos.CreateNeededElectricityAndThroughputPanel
                        (
                            nodeState: cosmicBody.NodeState,
                            neededElectricityGraph: overallNeededElectricityGraph,
                            throughputGraph: overallThroughputGraph
                        );
                        Button<TextBox> buildButton = new
                        (
                            shape: new MyRectangle(width: CurGameConfig.standardUIElementWidth, height: CurGameConfig.UILineHeight),
                            visual: new(text: "Build here", textColor: colorConfig.buttonTextColor),
                            tooltip: new ImmutableTextTooltip(text: UIAlgorithms.BuildHereTooltip)
                        )
                        {
                            PersonallyEnabled = false
                        };
                        buildButton.clicked.Add
                        (
                            listener: new BuildOnCosmicBodyButtonListener
                            (
                                buildingConfigPanelManager: this,
                                cosmicBody: cosmicBody
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
                        return
                        (
                            cosmicBody,
                            new CosmicBodyBuildPanelManager
                            (
                                CosmicBodyBuildPanel: cosmicBodyBuildPanel,
                                CosmicBodyPanelHUDPosUpdate: new HUDElementPosUpdater
                                (
                                    HUDElement: cosmicBodyBuildPanel,
                                    baseWorldObject: cosmicBody,
                                    HUDElementOrigin: new(HorizPosEnum.Middle, VertPosEnum.Top),
                                    anchorInBaseWorldObject: new(HorizPosEnum.Middle, VertPosEnum.Middle)
                                ),
                                BuildButton: buildButton
                            )
                        );
                    }
                );
                foreach (var (cosmicBody, buildPanelManager) in addCosmicBodyBuildPanelManagers)
                    cosmicBodyBuildPanelManagers.Add(key: cosmicBody, value: buildPanelManager);
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

                foreach (var cosmicBodyPanelManager in cosmicBodyBuildPanelManagers.Values)
                    cosmicBodyPanelManager.BuildButton.PersonallyEnabled = CompleteBuildingConfigOrNull is not null;
            }

            public void RemoveCosmicBodyBuildPanelManager(CosmicBody cosmicBody)
            {
                var cosmicBodyBuildPanel = cosmicBodyBuildPanelManagers[cosmicBody].CosmicBodyBuildPanel;
                cosmicBodyBuildPanelManagers.Remove(cosmicBody);
                CurWorldManager.RemoveWorldHUDElement(worldHUDElement: cosmicBodyBuildPanel);
            }

            public void StopBuildingConfig()
            {
                CurWorldManager.RemoveHUDElement(HUDElement: buildingConfigPanel);
                foreach (var cosmicBodyPanelManager in cosmicBodyBuildPanelManagers.Values)
                    CurWorldManager.RemoveWorldHUDElement(worldHUDElement: cosmicBodyPanelManager.CosmicBodyBuildPanel);
                CurWorldManager.EnableAllUIElements();
            }
        }

        [Serializable]
        private sealed class StopBuildingConfig(BuildingConfigPanelManager buildingConfigPanelManager) : IAction
        {
            void IAction.Invoke()
                => buildingConfigPanelManager.StopBuildingConfig();
        }

        [Serializable]
        private sealed class DoneBuildingButtonListener(BuildingConfigPanelManager buildingConfigPanelManager) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => buildingConfigPanelManager.StopBuildingConfig();
        }

        [Serializable]
        private readonly record struct CosmicBodyBuildPanelManager(UIRectVertPanel<IHUDElement> CosmicBodyBuildPanel, IAction CosmicBodyPanelHUDPosUpdate, Button<TextBox> BuildButton);

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
        private sealed class BuildOnCosmicBodyButtonListener(BuildingConfigPanelManager buildingConfigPanelManager, CosmicBody cosmicBody) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                cosmicBody.StartConstruction
                (
                    constrConcreteParams: buildingConfigPanelManager.CompleteBuildingConfigOrNull!.Value.CreateConcreteConstrParams(cosmicBody: cosmicBody)
                );
                buildingConfigPanelManager.RemoveCosmicBodyBuildPanelManager(cosmicBody: cosmicBody);
            }
        }

        void IClickedListener.ClickedResponse()
            => BuildingConfigPanelManager.StartBuildingConfig(cosmicBodies: cosmicBodies, constrGeneralParams: constrGeneralParams);
    }   
}
