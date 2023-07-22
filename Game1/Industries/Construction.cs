using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    public static class Construction
    {
        [Serializable]
        public sealed class GeneralParams
        {
            public readonly string name;
            public readonly IGeneralBuildingConstructionParams buildingGeneralParams;
            public readonly EnergyPriority energyPriority;

            public GeneralParams(IGeneralBuildingConstructionParams buildingGeneralParams, EnergyPriority energyPriority)
            {
                name = UIAlgorithms.ConstructionName(childIndustryName: buildingGeneralParams.Name);
                this.buildingGeneralParams = buildingGeneralParams;
                this.energyPriority = energyPriority;
            }

            [Serializable]
            private readonly struct MaterialChoicePanelManager
            {
                public readonly UIRectVertPanel<IHUDElement> materialChoicePanel;

                private readonly GeneralParams constrGeneralParams;
                private readonly IIndustryFacingNodeState nodeState;
                private readonly Dictionary<IMaterialPurpose, Material> materialChoices;
                private readonly Button buildButton, cancelButton;

                public MaterialChoicePanelManager(GeneralParams constrGeneralParams, IIndustryFacingNodeState nodeState, IAction<IIndustry> setIndustry, IClickedListener cancelButtonListener)
                // IClickedListener buildButtonListener
                {
                    this.constrGeneralParams = constrGeneralParams;
                    this.nodeState = nodeState;
                    materialChoicePanel = new(childHorizPos: HorizPos.Left);
                    materialChoicePanel.AddChild(child: new TextBox() { Text = "Material Choices" });
                    EfficientReadOnlyHashSet<IMaterialPurpose> neededMatPurposes = constrGeneralParams.CreateConcrete(nodeState, buildingMatChoices: new()).SwitchExpression
                    (
                        ok: _ =>
                        {
                            Debug.Fail("All buildings need some materials to choose");
                            return new();
                        },
                        error: missingMatPurposes => missingMatPurposes
                    );
                    materialChoices = new();
                    foreach (var materialPurpose in neededMatPurposes)
                    {
                        UIRectHorizPanel<IHUDElement> materialChoiceLine = new(childVertPos: VertPos.Middle);
                        materialChoiceLine.AddChild(child: new TextBox() { Text = materialPurpose.Name + " " });
                        Button startMaterialChoice = new
                        (
                            shape: new MyRectangle(),
                            tooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMaterialChoiceForPurposeTooltip(materialPurpose: materialPurpose)),
                            text: "+"
                        );
                        startMaterialChoice.clicked.Add
                        (
                            listener: new StartMaterialChoiceListener
                            (
                                MaterialChoicePanelManager: this,
                                StartMaterialChoice: startMaterialChoice,
                                MaterialPurpose: materialPurpose
                            )
                        );
                        materialChoiceLine.AddChild(child: startMaterialChoice);
                        materialChoicePanel.AddChild(child: materialChoiceLine);
                    }
                    buildButton = new
                    (
                        shape: new MyRectangle(),
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.FinalizeBuildingMaterialChoices),
                        text: "Build this"
                    );
                    buildButton.clicked.Add(listener: buildButtonListener);
                    buildButton.PersonallyEnabled = false;
                    materialChoicePanel.AddChild(child: buildButton);
                    cancelButton = new
                    (
                        shape: new MyRectangle(),
                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.CancelMaterialChoiceForBuilding),
                        text: "Cancel",
                        color: ActiveUIManager.colorConfig.deleteButtonColor
                    );
                    cancelButton.clicked.Add(listener: cancelButtonListener);
                    materialChoicePanel.AddChild(child: cancelButton);
                }

                public void SetMatChoice(IMaterialPurpose materialPurpose, Material material)
                {
                    materialChoices[materialPurpose] = material;
                    //constrGeneralParams.CreateConcrete(nodeState: nodeState, buildingMatChoices: materialChoices).SwitchStatement
                    //(
                    //    ok: _ => buildButton.PersonallyEnabled = true,
                    //    error: _ => buildButton.PersonallyEnabled = false
                    //);
                    buildButton.PersonallyEnabled = constrGeneralParams.CreateConcrete(nodeState: nodeState, buildingMatChoices: materialChoices).isOk;
                    // In case all material choices are made, show player the stats of the to-be-constructed building
                    throw new NotImplementedException()
                }
            }

            public IHUDElement CreateNextUIStepPanel(IIndustryFacingNodeState nodeState, IClickedListener buildButtonListener, IClickedListener cancelButtonListener)
            {
                MaterialChoicePanelManager materialChoicePanelManager = new(constrGeneralParams: this, nodeState: nodeState, buildButtonListener: buildButtonListener, cancelButtonListener: cancelButtonListener);
                return materialChoicePanelManager.materialChoicePanel;
                //UIRectVertPanel<IHUDElement> materialChoicePanel = new(childHorizPos: HorizPos.Left);
                //materialChoicePanel.AddChild(child: new TextBox() { Text = "Material Choices" });
                //EfficientReadOnlyHashSet<IMaterialPurpose> neededMatPurposes = CreateConcrete(nodeState, buildingMatChoices: new()).SwitchExpression
                //(
                //    ok: _ =>
                //    {
                //        Debug.Fail("All buildings need some materials to choose");
                //        return new();
                //    },
                //    error: missingMatPurposes => missingMatPurposes
                //);
                //Dictionary<IMaterialPurpose, Material> materialChoices = new();
                //foreach (var materialPurpose in neededMatPurposes)
                //{
                //    UIRectHorizPanel<IHUDElement> materialChoiceLine = new(childVertPos: VertPos.Middle);
                //    materialChoiceLine.AddChild(child: new TextBox() { Text = materialPurpose.Name + " " });
                //    Button startMaterialChoice = new
                //    (
                //        shape: new MyRectangle(),
                //        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.StartMaterialChoiceForPurposeTooltip(materialPurpose: materialPurpose)),
                //        text: "+"
                //    );
                //    startMaterialChoice.clicked.Add
                //    (
                //        listener: new StartMaterialChoiceListener
                //        (
                //            StartMaterialChoice: startMaterialChoice,
                //            MaterialChoices: materialChoices,
                //            MaterialPurpose: materialPurpose
                //        )
                //    );
                //    materialChoiceLine.AddChild(child: startMaterialChoice);
                //    materialChoicePanel.AddChild(child: materialChoiceLine);
                //}
                //Button buildButton = new
                //(
                //    shape: new MyRectangle(),
                //    tooltip: new ImmutableTextTooltip(text: UIAlgorithms.FinalizeBuildingMaterialChoices),
                //    text: "Build this"
                //);
                //buildButton.clicked.Add(listener: buildButtonListener);
                //buildButton.PersonallyEnabled = false;
                //materialChoicePanel.AddChild(child: buildButton);
                //Button cancelButton = new
                //(
                //    shape: new MyRectangle(),
                //    tooltip: new ImmutableTextTooltip(text: UIAlgorithms.CancelMaterialChoiceForBuilding),
                //    text: "Cancel",
                //    color: ActiveUIManager.colorConfig.deleteButtonColor
                //);
                //cancelButton.clicked.Add(listener: cancelButtonListener);
                //materialChoicePanel.AddChild(child: cancelButton);
                //return materialChoicePanel;
            }

            [Serializable]
            private sealed record StartMaterialChoiceListener(MaterialChoicePanelManager MaterialChoicePanelManager, Button StartMaterialChoice, IMaterialPurpose MaterialPurpose) : IClickedListener
            {
                void IClickedListener.ClickedResponse()
                {
                    UIRectVertPanel<IHUDElement> materialChoicePopup = new(childHorizPos: Shapes.HorizPos.Middle);
                    foreach (var material in CurResConfig.GetCurRes<Material>())
                    {
                        Button chooseMatButton = new
                        (
                            shape: new MyRectangle(),
                            tooltip: new ImmutableTextTooltip(MaterialPurpose.TooltipTextFor(material: material)),
                            text: material.Name
                        );
                        chooseMatButton.clicked.Add
                        (
                            listener: new MaterialChoiceListener
                            (
                                MaterialChoicePanelManager: MaterialChoicePanelManager,
                                MaterialChoicePopup: materialChoicePopup,
                                StartMaterialChoice: StartMaterialChoice,
                                MaterialPurpose: MaterialPurpose,
                                Material: material
                            )
                        );
                        materialChoicePopup.AddChild(child: chooseMatButton);
                    }

                    CurWorldManager.AddHUDPopup(HUDElement: materialChoicePopup);
                }
            }

            [Serializable]
            private sealed record MaterialChoiceListener(MaterialChoicePanelManager MaterialChoicePanelManager, IHUDElement MaterialChoicePopup, Button StartMaterialChoice, IMaterialPurpose MaterialPurpose, Material Material) : IClickedListener
            {
                void IClickedListener.ClickedResponse()
                {
                    StartMaterialChoice.Text = Material.Name;
                    MaterialChoicePanelManager.SetMatChoice(materialPurpose: MaterialPurpose, material: Material);
                    CurWorldManager.RemoveHUDPopup(HUDElement: MaterialChoicePopup);
                }
            }

            public Result<ConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices) // IReadOnlyDictionary<IMaterialPurpose, Material> buildingMatChoices)
                => buildingGeneralParams.CreateConcrete
                (
                    nodeState: nodeState,
                    neededBuildingMatChoices: buildingMatChoices.FilterOutUnneededMaterials(materialPropors: buildingGeneralParams.BuildingComponentMaterialPropors)
                ).Select
                (
                    buildingConcreteParams => new ConcreteParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        concreteBuildingParams: buildingConcreteParams
                    )
                );
        }

        [Serializable]
        public readonly struct ConcreteParams : Industry.IConcreteBuildingParams<UnitType>
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public readonly AllResAmounts buildingCost;
            public readonly AreaDouble buildingComponentsUsefulArea;

            private readonly IConcreteBuildingConstructionParams concreteBuildingParams;
            /// <summary>
            /// Keys contain ALL material purposes, not just used ones
            /// </summary>
            private readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMaterialPropors;

            public ConcreteParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, IConcreteBuildingConstructionParams concreteBuildingParams)
            {
                Name = generalParams.name;
                this.NodeState = nodeState;
                EnergyPriority = generalParams.energyPriority;
                buildingCost = concreteBuildingParams.BuildingCost;
                buildingComponentsUsefulArea = ResAndIndustryAlgos.BuildingComponentUsefulArea
                (
                    buildingArea: concreteBuildingParams.IncompleteBuildingImage(donePropor: Propor.full).Area
                );

                this.concreteBuildingParams = concreteBuildingParams;
                buildingMaterialPropors = generalParams.buildingGeneralParams.BuildingComponentMaterialPropors;
            }

            public IIndustry CreateIndustry()
                => new Industry<UnitType, ConcreteParams, UnitType, ConstructionState>(productionParams: new(), buildingParams: this, persistentState: new());

            public IBuildingImage IncompleteBuildingImage(Propor donePropor)
                => concreteBuildingParams.IncompleteBuildingImage(donePropor: donePropor);

            public IIndustry CreateChildIndustry(ResPile buildingResPile)
                => concreteBuildingParams.CreateIndustry(buildingResPile: buildingResPile);

            public CurProdStats CurConstrStats()
                => ResAndIndustryAlgos.CurConstrStats
                (
                    buildingMaterialPropors: buildingMaterialPropors,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature
                );

            IBuildingImage Industry.IConcreteBuildingParams<UnitType>.IdleBuildingImage
                => IncompleteBuildingImage(donePropor: Propor.empty);

            Material? Industry.IConcreteBuildingParams<UnitType>.SurfaceMaterial(bool productionInProgress)
                => productionInProgress switch
                {
                    true => concreteBuildingParams.SurfaceMaterial,
                    false => null
                };

            AllResAmounts Industry.IConcreteBuildingParams<UnitType>.TargetStoredResAmounts(UnitType productionParams)
                => buildingCost;
        }

        [Serializable]
        private sealed class ConstructionState : Industry.IProductionCycleState<UnitType, ConcreteParams, UnitType, ConstructionState>
        {
            public static bool IsRepeatable
                => false;

            public static Result<ConstructionState, TextErrors> Create(UnitType productionParams, ConcreteParams parameters, UnitType persistentState)
            {
                var buildingResPile = ResPile.CreateIfHaveEnough
                (
                    source: parameters.NodeState.StoredResPile,
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
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private CurProdStats curConstrStats;
            private Propor donePropor, workingPropor;

            private ConstructionState(ResPile buildingResPile, ConcreteParams parameters)
            {
                this.buildingResPile = buildingResPile;
                this.parameters = parameters;
                donePropor = Propor.empty;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: parameters.NodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
            }

            public IBuildingImage BusyBuildingImage()
                => parameters.IncompleteBuildingImage(donePropor: donePropor);

            public void FrameStartNoProduction()
            { }

            public void FrameStart()
            {
                curConstrStats = parameters.CurConstrStats();
                ReqEnergy = reqEnergyHistoricRounder.CurEnergy<ElectricalEnergy>(watts: curConstrStats.ReqWatts, proporUtilized: Propor.full, elapsed: CurWorldManager.Elapsed);
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingPropor = ResAndIndustryHelpers.WorkingPropor(proporUtilized: Propor.full, allocatedEnergy: electricalEnergy, reqEnergy: ReqEnergy);
            }

            /// <summary>
            /// Returns child industry if finished construction, null otherwise
            /// </summary>
            public IIndustry? Update()
            {
                parameters.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                donePropor = donePropor.UpdateDonePropor
                (
                    workingPropor: workingPropor,
                    producedAreaPerSec: curConstrStats.ProducedAreaPerSec,
                    elapsed: CurWorldManager.Elapsed,
                    areaInProduction: parameters.buildingComponentsUsefulArea
                );

                if (donePropor.IsFull)
                {
                    var childIndustry = parameters.CreateChildIndustry(buildingResPile: buildingResPile);
                    CurWorldManager.PublishMessage
                    (
                        message: new BasicMessage
                        (
                            nodeID: parameters.NodeState.NodeID,
                            message: UIAlgorithms.ConstructionComplete(buildingName: childIndustry.Name)
                        )
                    );
                    return childIndustry;
                }
                return null;
            }

            public void Delete()
            {
                parameters.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                parameters.NodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
            }
        }
    }
}
