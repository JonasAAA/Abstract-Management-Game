using Game1.Collections;
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
            public readonly string buildButtonName;
            public readonly ITooltip toopltip;
            public readonly EfficientReadOnlyHashSet<IMaterialPurpose> neededMaterialPurposes;

            public GeneralParams(IGeneralBuildingConstructionParams buildingGeneralParams, EnergyPriority energyPriority)
            {
                name = UIAlgorithms.ConstructionName(childIndustryName: buildingGeneralParams.Name);
                this.buildingGeneralParams = buildingGeneralParams;
                this.energyPriority = energyPriority;
                buildButtonName = buildingGeneralParams.Name;
                toopltip = new ImmutableTextTooltip(text: UIAlgorithms.ConstructionTooltip(constrGeneralParams: this));
                neededMaterialPurposes = buildingGeneralParams.BuildingCostPropors.neededMaterialPurposes;
            }

            public bool SufficientbuildingMaterials(MaterialChoices curBuildingMaterialChoices)
                => neededMaterialPurposes.IsSubsetOf(other: curBuildingMaterialChoices.Keys);

            public Result<ConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices)
                => buildingGeneralParams.CreateConcrete
                (
                    nodeState: nodeState,
                    neededBuildingMatChoices: buildingMatChoices.FilterOutUnneededMaterials(neededMaterialPurposes: neededMaterialPurposes)
                ).Select
                (
                    buildingConcreteParams => new ConcreteParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        concreteBuildingParams: buildingConcreteParams
                    )
                );

            //public ConcreteParams CreateConcreteOrThrow(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices)
            //    => CreateConcrete(nodeState: nodeState, buildingMatChoices: buildingMatChoices).UnwrapOrThrow(exception: ;
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
                NodeState = nodeState;
                EnergyPriority = generalParams.energyPriority;
                buildingCost = concreteBuildingParams.BuildingCost;
                buildingComponentsUsefulArea = concreteBuildingParams.IncompleteBuildingImage(donePropor: Propor.full).Area * CurWorldConfig.buildingComponentsProporOfBuildingArea;

                this.concreteBuildingParams = concreteBuildingParams;
                buildingMaterialPropors = generalParams.buildingGeneralParams.BuildingCostPropors.materialPropors;
            }

            public IBuildingImage IncompleteBuildingImage(Propor donePropor)
                => concreteBuildingParams.IncompleteBuildingImage(donePropor: donePropor);

            public IIndustry CreateIndustry()
                => new Industry<UnitType, ConcreteParams, UnitType, ConstructionState>
                (
                    productionParams: new(),
                    buildingParams: this,
                    persistentState: new()
                );

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

            EfficientReadOnlyCollection<IResource> Industry.IConcreteBuildingParams<UnitType>.PotentiallyNotNeededBuildingComponents
                => EfficientReadOnlyCollection<IResource>.empty;

            Material? Industry.IConcreteBuildingParams<UnitType>.SurfaceMaterial(bool productionInProgress)
                => productionInProgress switch
                {
                    true => concreteBuildingParams.SurfaceMaterial,
                    false => null
                };
            
            EfficientReadOnlyCollection<IResource> Industry.IConcreteBuildingParams<UnitType>.GetProducedResources(UnitType productionParams)
                => EfficientReadOnlyCollection<IResource>.empty;
            
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

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Industry<UnitType, ConcreteParams, UnitType, ConstructionState>)
            };
    }
}
