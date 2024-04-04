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
    public static class PowerPlant
    {
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly EnergyPriority energyPriority;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.powerPlantbuildingColor);
                NameVisual = UIAlgorithms.GetBuildingNameVisual(name: name);
                BuildingCostPropors = new BuildingCostPropors(ingredProdToAmounts: buildingComponentPropors);

                energyPriority = EnergyPriority.mostImportant;
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
        public readonly struct ConcreteBuildingParams : Industry.IConcreteBuildingParams<UnitType>, IConcreteBuildingConstructionParams
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

            public PowerPlantProdStats CurProdStats()
                => ResAndIndustryAlgos.CurPowerPlantProdStats
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
                return new Industry<UnitType, ConcreteBuildingParams, ResPile, PowerProductionState>
                (
                    productionParams: UnitType.value,
                    buildingParams: this,
                    persistentState: buildingResPile,
                    statsGraphsParams: statsGraphsParams
                );
            }

            static bool Industry.IConcreteBuildingParams<UnitType>.RequiresResources
                => false;

            static bool Industry.IConcreteBuildingParams<UnitType>.ProducesResources
                => false;

            IBuildingImage Industry.IConcreteBuildingParams<UnitType>.IdleBuildingImage
                => buildingImage;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<UnitType>.GetProducedResources(UnitType productionParams)
                => SortedResSet<IResource>.empty;

            SortedResSet<IResource> Industry.IConcreteBuildingParams<UnitType>.GetConsumedResources(UnitType productionParams)
                => SortedResSet<IResource>.empty;

            AllResAmounts Industry.IConcreteBuildingParams<UnitType>.MaxStoredInput(UnitType productionParams)
                => AllResAmounts.empty;

            IndustryFunctionVisualParams Industry.IConcreteBuildingParams<UnitType>.IndustryFunctionVisualParams(UnitType productionParams)
                => IncompleteFunctionVisualParams();
        }

        [Serializable]
        private sealed class PowerProductionState : Industry.IProductionCycleState<UnitType, ConcreteBuildingParams, ResPile, PowerProductionState>, IEnergyProducer
        {
            public static bool IsRepeatable
                => false;

            public static Result<PowerProductionState, TextErrors> Create(UnitType productionParams, ConcreteBuildingParams parameters, ResPile buildingResPile,
                ResPile inputStorage, AreaInt storedOutputArea)
                => new(ok: new(buildingParams: parameters));

            public ElectricalEnergy ReqEnergy
                => ElectricalEnergy.zero;

            public bool ShouldRestart
                => false;

            private readonly ConcreteBuildingParams buildingParams;

            private RadiantEnergy energyToTransform;

            private PowerProductionState(ConcreteBuildingParams buildingParams)
            {
                this.buildingParams = buildingParams;
                energyToTransform = RadiantEnergy.zero;

                CurWorldManager.AddEnergyProducer(energyProducer: this);
            }

            public IBuildingImage BusyBuildingImage()
                => buildingParams.buildingImage;

            public Propor FrameStartAndReturnThroughputUtilization()
            {
                var curProdStats = buildingParams.CurProdStats();
                var energyProporUsed = Propor.Create(part: curProdStats.ReqWatts, curProdStats.ProdWatts);
                var maxEnergyToUseAndTranform = ResAndIndustryHelpers.CurEnergy<RadiantEnergy>
                (
                    watts: curProdStats.ProdWatts,
                    proporUtilized: Propor.full,
                    elapsed: CurWorldManager.Elapsed
                );
                var energyToUseAndTransform = MyMathHelper.Min
                (
                    buildingParams.NodeState.RadiantEnergyPile.Amount,
                    maxEnergyToUseAndTranform
                );
                (energyToTransform, Propor proporUtilized) = energyProporUsed switch
                {
                    not null =>
                    (
                        Algorithms.ScaleEnergy
                        (
                            amount: energyToUseAndTransform,
                            scale: (UDouble)energyProporUsed.Value.Opposite()
                        ),
                        Propor.Create
                        (
                            part: energyToUseAndTransform.ValueInJ,
                            whole: maxEnergyToUseAndTranform.ValueInJ
                        )!.Value
                    ),
                    null => (RadiantEnergy.zero, Propor.empty)
                };

                return proporUtilized;
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
                => Debug.Assert(electricalEnergy.IsZero);

            public Result<IIndustry?, TextErrors> Update(ResPile outputStorage)
                => energyToTransform.IsZero switch
                {
                    true => new(errors: new("Receive not enough light")),
                    false => new(ok: null)
                };

            public void Delete(ResPile outputStorage)
                => CurWorldManager.RemoveEnergyProducer(energyProducer: this);

            public static void DeletePersistentState(ResPile buildingResPile, ResPile outputStorage)
                => outputStorage.TransferAllFrom(source: buildingResPile);

            void IEnergyProducer.ProduceEnergy(EnergyPile<ElectricalEnergy> destin)
                => buildingParams.NodeState.RadiantEnergyPile.TransformTo
                (
                    destin: destin,
                    amount: energyToTransform
                );

            void IEnergyProducer.TakeBackUnusedEnergy(EnergyPile<ElectricalEnergy> source, ElectricalEnergy amount)
                => source.TransformTo
                (
                    destin: buildingParams.NodeState.RadiantEnergyPile,
                    amount: amount
                );
        }

        private static IndustryFunctionVisualParams IncompleteFunctionVisualParams()
            => new
            (
                InputIcons: [IIndustry.starlightIcon],
                OutputIcons: [IIndustry.electricityIcon]
            );

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Industry<UnitType, ConcreteBuildingParams, ResPile, PowerProductionState>)
            };
    }
}
