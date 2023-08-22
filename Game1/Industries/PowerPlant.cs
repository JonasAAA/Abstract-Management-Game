﻿using Game1.Collections;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    /// <summary>
    /// Responds properly to planet shrinking, but NOT to planet widening
    /// </summary>
    public static class PowerPlant
    {
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public string Name { get; }
            public GeneralProdAndMatAmounts BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, color: ActiveUIManager.colorConfig.miningBuildingColor);
                Name = name;
                BuildingCostPropors = new GeneralProdAndMatAmounts(ingredProdToAmounts: buildingComponentPropors, ingredMatPurposeToUsefulAreas: new());
                if (BuildingCostPropors.materialPropors[IMaterialPurpose.roofSurface].IsEmpty)
                    throw new ArgumentException();

                energyPriority = EnergyPriority.mostImportant;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public Result<IConcreteBuildingConstructionParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
                => ResAndIndustryAlgos.BuildingComponentsToAmountPUBA
                (
                    buildingComponentPropors: buildingComponentPropors,
                    buildingMatChoices: neededBuildingMatChoices
                ).Select<IConcreteBuildingConstructionParams>
                (
                    buildingComponentsToAmountPUBA => new ConcreteBuildingParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        buildingImage: buildingImageParams.CreateImage(nodeShapeParams: nodeState),
                        buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                        buildingMatChoices: neededBuildingMatChoices,
                        surfaceMaterial: neededBuildingMatChoices[IMaterialPurpose.roofSurface]
                    )
                );
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<UnitType>, IConcreteBuildingConstructionParams
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public Material SurfaceMaterial { get; }
            public readonly DiskBuildingImage buildingImage;

            /// <summary>
            /// Things depend on this rather than on building components target area as can say that if planet underneath building shrinks,
            /// building gets not enough space to operate at maximum efficiency
            /// </summary>
            private AreaDouble CurBuildingArea
                => buildingImage.Area;

            private readonly GeneralBuildingParams generalParams;
            private readonly EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA;
            private readonly MaterialChoices buildingMatChoices;
            private readonly AllResAmounts startingBuildingCost;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA,
                MaterialChoices buildingMatChoices, Material surfaceMaterial)
            {
                Name = generalParams.Name;
                this.NodeState = nodeState;
                this.buildingImage = buildingImage;
                this.SurfaceMaterial = surfaceMaterial;
                EnergyPriority = generalParams.energyPriority;

                this.generalParams = generalParams;
                this.buildingComponentsToAmountPUBA = buildingComponentsToAmountPUBA;
                this.buildingMatChoices = buildingMatChoices;

                startingBuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);
            }

            public UDouble WattsToProduce(UDouble incidentWatts)
                => ResAndIndustryAlgos.CurProducedWatts
                (
                    buildingCostPropors: generalParams.BuildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: NodeState.SurfaceGravity,
                    temperature: NodeState.Temperature,
                    buildingArea: CurBuildingArea,
                    incidentWatts: incidentWatts
                );

            public void RemoveUnneededBuildingComponents(ResPile buildingResPile)
                => ResAndIndustryHelpers.RemoveUnneededBuildingComponents(nodeState: NodeState, buildingResPile: buildingResPile, buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);

            AllResAmounts IConcreteBuildingConstructionParams.BuildingCost
                => startingBuildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new Industry<UnitType, ConcreteBuildingParams, ResPile, PowerProductionState>(productionParams: new(), buildingParams: this, persistentState: buildingResPile);

            IBuildingImage Industry.IConcreteBuildingParams<UnitType>.IdleBuildingImage
                => buildingImage;

            EfficientReadOnlyCollection<IResource> Industry.IConcreteBuildingParams<UnitType>.PotentiallyNotNeededBuildingComponents
                => startingBuildingCost.resList;

            Material? Industry.IConcreteBuildingParams<UnitType>.SurfaceMaterial(bool productionInProgress)
                => SurfaceMaterial;

            EfficientReadOnlyCollection<IResource> Industry.IConcreteBuildingParams<UnitType>.GetProducedResources(UnitType productionParams)
                => EfficientReadOnlyCollection<IResource>.empty;

            AllResAmounts Industry.IConcreteBuildingParams<UnitType>.TargetStoredResAmounts(UnitType productionParams)
                => AllResAmounts.empty;
        }

        [Serializable]
        private sealed class PowerProductionState : Industry.IProductionCycleState<UnitType, ConcreteBuildingParams, ResPile, PowerProductionState>, IEnergyProducer
        {
            public static bool IsRepeatable
                => false;

            public static Result<PowerProductionState, TextErrors> Create(UnitType productionParams, ConcreteBuildingParams parameters, ResPile buildingResPile)
                => new(ok: new(buildingParams: parameters, buildingResPile: buildingResPile));

            public ElectricalEnergy ReqEnergy
                => ElectricalEnergy.zero;

            public bool ShouldRestart
                => false;

            private readonly ConcreteBuildingParams buildingParams;
            private readonly ResPile buildingResPile;
            private readonly HistoricRounder prodEnergyHistoricRounder;

            private RadiantEnergy energyToTransform;

            private PowerProductionState(ConcreteBuildingParams buildingParams, ResPile buildingResPile)
            {
                this.buildingParams = buildingParams;
                this.buildingResPile = buildingResPile;
                prodEnergyHistoricRounder = new();
                energyToTransform = RadiantEnergy.zero;

                CurWorldManager.AddEnergyProducer(energyProducer: this);
            }

            public IBuildingImage BusyBuildingImage()
                => buildingParams.buildingImage;

            public void FrameStartNoProduction()
                => energyToTransform = RadiantEnergy.zero;

            public void FrameStart()
            {
                UDouble wattsToProduce = buildingParams.WattsToProduce
                (
                    incidentWatts: buildingParams.NodeState.RadiantEnergyPile.Amount.ValueInJ / (UDouble)CurWorldManager.Elapsed.TotalSeconds
                );
                energyToTransform = MyMathHelper.Min(buildingParams.NodeState.RadiantEnergyPile.Amount, prodEnergyHistoricRounder.CurEnergy<RadiantEnergy>(watts: wattsToProduce, proporUtilized: Propor.full, elapsed: CurWorldManager.Elapsed));
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                Debug.Assert(electricalEnergy.IsZero);
            }

            /// <summary>
            /// This will not remove no longer needed building components until mining cycle is done since fix current mining volume
            /// and some other mining stats at the start of the mining cycle. 
            /// </summary>
            public IIndustry? Update()
            {
                buildingParams.RemoveUnneededBuildingComponents(buildingResPile: buildingResPile);
                return null;
            }

            public void Delete()
            {
                buildingParams.NodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
                CurWorldManager.RemoveEnergyProducer(energyProducer: this);
            }

            void IEnergyProducer.ProduceEnergy(EnergyPile<ElectricalEnergy> destin)
                => buildingParams.NodeState.RadiantEnergyPile.TransformTo
                (
                    destin: destin,
                    amount: energyToTransform
                );
        }
    }
}
