using Game1.Collections;
using Game1.Delegates;
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
            public readonly IBuildingGeneralParams buildingGeneralParams;
            public readonly EnergyPriority energyPriority;

            public GeneralParams(IBuildingGeneralParams buildingGeneralParams, EnergyPriority energyPriority)
            {
                name = UIAlgorithms.ConstructionName(childIndustryName: buildingGeneralParams.Name);
                this.buildingGeneralParams = buildingGeneralParams;
                this.energyPriority = energyPriority;
            }

            public Result<ConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices)
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
                        buildingConcreteParams: buildingConcreteParams
                    )
                );
        }

        [Serializable]
        public readonly struct ConcreteParams : Industry.IConcreteBuildingParams<UnitType>
        {
            public readonly string Name { get; }
            public readonly IIndustryFacingNodeState NodeState { get; }
            public readonly EnergyPriority EnergyPriority { get; }
            public readonly AllResAmounts buildingCost;
            public readonly AreaDouble buildingComponentsUsefulArea;

            private readonly IBuildingConcreteParams buildingConcreteParams;
            /// <summary>
            /// Keys contain ALL material purposes, not just used ones
            /// </summary>
            private readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMaterialPropors;

            public ConcreteParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, IBuildingConcreteParams buildingConcreteParams)
            {
                Name = generalParams.name;
                this.NodeState = nodeState;
                EnergyPriority = generalParams.energyPriority;
                buildingCost = buildingConcreteParams.BuildingCost;
                buildingComponentsUsefulArea = ResAndIndustryAlgos.BuildingComponentUsefulArea
                (
                    buildingArea: buildingConcreteParams.IncompleteBuildingImage(donePropor: Propor.full).Area
                );

                this.buildingConcreteParams = buildingConcreteParams;
                buildingMaterialPropors = generalParams.buildingGeneralParams.BuildingComponentMaterialPropors;
            }

            public IBuildingImage IncompleteBuildingImage(Propor donePropor)
                => buildingConcreteParams.IncompleteBuildingImage(donePropor: donePropor);

            //public Construction CreateIndustry()
            //    => new(parameters: this);

            public IIndustry CreateChildIndustry(ResPile buildingResPile)
                => buildingConcreteParams.CreateIndustry(buildingResPile: buildingResPile);

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
                    // NEW: May want reflexivity and the other number be some combination of planet reflexivity, final building reflexivity, and building
                    // raw material internals reflexivity. E.g. first third is mix of planet and building internals, middle third is just building internals,
                    // and the last third is mix of building internals and final building
                    //
                    // would want to return the mix of materials that the building consists of.
                    // COULD also always return null or the surface material of the finished building, but that doesn't make much sense
                    // though is simple to understand and to implement
                    true => throw new NotImplementedException(),
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

            public void FrameStart()
            {
                curConstrStats = parameters.CurConstrStats();
                ReqEnergy = ElectricalEnergy.CreateFromJoules
                (
                    valueInJ: reqEnergyHistoricRounder.Round
                    (
                        value: (decimal)curConstrStats.ReqWatts * (decimal)CurWorldManager.Elapsed.TotalSeconds,
                        curTime: CurWorldManager.CurTime
                    )
                );
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingPropor = Propor.Create(part: electricalEnergy.ValueInJ, whole: ReqEnergy.ValueInJ)!.Value;
            }

            /// <summary>
            /// Returns child industry if finished construction, null otherwise
            /// </summary>
            public IIndustry? Update()
            {
                parameters.NodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                AreaDouble areaConstructed = AreaDouble.CreateFromMetSq(valueInMetSq: workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curConstrStats.ProducedAreaPerSec);
                donePropor = Propor.CreateByClamp(value: (UDouble)donePropor + areaConstructed.valueInMetSq / parameters.buildingComponentsUsefulArea.valueInMetSq);

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
