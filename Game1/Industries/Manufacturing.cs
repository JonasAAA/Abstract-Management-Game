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
    [Serializable]
    public sealed class Manufacturing : IIndustry
    {
        [Serializable]
        public sealed class GeneralParams : IBuildingGeneralParams
        {
            public string Name { get; }
            public EfficientReadOnlyDictionary<IMaterialPurpose, Propor> BuildingComponentMaterialPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;
            public readonly GeneralProdAndMatAmounts buildingCostPropors;
            public readonly EnergyPriority energyPriority;
            public readonly Product.Params productParams;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors, EnergyPriority energyPriority, Product.Params productParams)
            {
                Name = name;
                BuildingComponentMaterialPropors = buildingCostPropors.materialPropors;
                buildingCostPropors = new GeneralProdAndMatAmounts(ingredProdToAmounts: buildingComponentPropors, ingredMatPurposeToUsefulAreas: new());
                if (buildingCostPropors.materialPropors[IMaterialPurpose.roofSurface].IsEmpty)
                    throw new ArgumentException();
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, color: ActiveUIManager.colorConfig.manufacturingBuildingColor);
                
                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.productParams = productParams;
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public Result<IBuildingConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
                => ResAndIndustryAlgos.BuildingComponentsToAmountPUBA
                (
                    buildingComponentPropors: buildingComponentPropors,
                    buildingMatChoices: neededBuildingMatChoices
                ).Select<IBuildingConcreteParams>
                (
                    buildingComponentsToAmountPUBA => new ConcreteBuildingParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        buildingImage: buildingImageParams.CreateImage(nodeState),
                        buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                        buildingMatChoices: neededBuildingMatChoices,
                        surfaceMaterial: neededBuildingMatChoices[IMaterialPurpose.roofSurface]
                    )
                );
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : IBuildingConcreteParams
        {
            public readonly string name;
            public readonly IIndustryFacingNodeState nodeState;
            public readonly DiskBuildingImage buildingImage;
            public readonly Material surfaceMaterial;
            public readonly EnergyPriority energyPriority;
            public readonly Product.Params productParams;

            /// <summary>
            /// Things depend on this rather than on building components target area as can say that if planet underneath building shrinks,
            /// building gets not enough space to operate at maximum efficiency
            /// </summary>
            private AreaDouble CurBuildingArea
                => buildingImage.Area;
            private readonly GeneralParams generalParams;
            private readonly EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA;
            private readonly MaterialChoices buildingMatChoices;
            private readonly AllResAmounts startingBuildingCost;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, DiskBuildingImage buildingImage,
                EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA,
                MaterialChoices buildingMatChoices, Material surfaceMaterial)
            {
                name = generalParams.Name;
                this.nodeState = nodeState;
                this.buildingImage = buildingImage;
                this.surfaceMaterial = surfaceMaterial;
                energyPriority = generalParams.energyPriority;
                productParams = generalParams.productParams; 

                this.generalParams = generalParams;
                this.buildingComponentsToAmountPUBA = buildingComponentsToAmountPUBA;
                this.buildingMatChoices = buildingMatChoices;
                startingBuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);
            }

            public ulong MaxProductAmount()
                => ResAndIndustryAlgos.MaxAmountInProduction
                (
                    areaInProduction: ResAndIndustryAlgos.AreaInProduction(buildingArea: CurBuildingArea),
                    itemUsefulArea: productParams.usefulArea
                );

            /// <param Name="productionMassIfFull">Mass of stuff in production if industry was fully operational</param>
            public CurProdStats CurProdStats(Mass productionMassIfFull)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.buildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature,
                    buildingArea: CurBuildingArea,
                    productionMass: productionMassIfFull
                );

            public void RemoveUnneededBuildingComponents(ResPile buildingResPile)
                => ResAndIndustryHelpers.RemoveUnneededBuildingComponents(nodeState: nodeState, buildingResPile: buildingResPile, buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);

            AllResAmounts IBuildingConcreteParams.BuildingCost
                => startingBuildingCost;

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IBuildingConcreteParams.CreateIndustry(ResPile buildingResPile)
                => new Manufacturing(buildingParams: this, buildingResPile: buildingResPile, productionParams: new(productParams: productParams));
        }

        [Serializable]
        public sealed class ConcreteProductionParams
        {
            /// <summary>
            /// In case of error, returns the needed but not yet set material purposes
            /// </summary>
            public Result<Product, TextErrors> CurProduct { get; private set; }

            private readonly Product.Params productParams;

            public ConcreteProductionParams(Product.Params productParams)
                : this(productParams: productParams, productMaterialChoices: MaterialChoices.empty)
            { }

            public ConcreteProductionParams(Product.Params productParams, MaterialChoices productMaterialChoices)
            {
                this.productParams = productParams;
                Update(productMaterialChoices: productMaterialChoices);
            }

            private void Update(MaterialChoices productMaterialChoices)
                => CurProduct = productParams.CreateProduct(materialChoices: productMaterialChoices).ConvertMissingMatPurpsIntoError();
        }

        [Serializable]
        private sealed class State
        {
            public static Result<State, TextErrors> Create(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ConcreteProductionParams productionParams)
                => productionParams.CurProduct.SelectMany
                (
                    product =>
                    {
                        ulong maxProductionAmount = buildingParams.MaxProductAmount();
                        var resInUseAndCount = ResPile.CreateMultipleIfHaveEnough
                        (
                            source: buildingParams.nodeState.StoredResPile,
                            amount: product.Recipe.ingredients,
                            maxCount: maxProductionAmount
                        );
                        return resInUseAndCount switch
                        {
                            (ResPile resInUse, ulong count) => new Result<State, TextErrors>
                            (
                                ok: new
                                (
                                    buildingParams: buildingParams,
                                    buildingResPile: buildingResPile,
                                    resInUse: resInUse,
                                    productRecipe: product.Recipe,
                                    productionAmount: count,
                                    maxProductionAmount: maxProductionAmount
                                )
                            ),
                            null => new(errors: new(UIAlgorithms.NotEnoughResourcesToStartProduction))
                        };
                    }
                );

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool IsDone
                => donePropor.IsFull;

            private readonly ConcreteBuildingParams buildingParams;
            private readonly ResPile buildingResPile, resInUse;
            private readonly ResRecipe recipe;
            private readonly Mass prodMassIfFull;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private readonly ulong productionAmount;
            private readonly Propor proporUtilized;

            private CurProdStats curProdStats;
            private Propor donePropor, workingPropor;

            private State(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ResPile resInUse, ResRecipe productRecipe, ulong productionAmount, ulong maxProductionAmount)
            {
                this.buildingParams = buildingParams;
                this.buildingResPile = buildingResPile;
                this.resInUse = resInUse;
                recipe = productRecipe * productionAmount;
                prodMassIfFull = productRecipe.ingredients.Mass() * maxProductionAmount;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.nodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                this.productionAmount = productionAmount;
                proporUtilized = Propor.Create(part: productionAmount, whole: maxProductionAmount)!.Value;
                donePropor = Propor.empty;
            }

            public void FrameStart()
            {
                curProdStats = buildingParams.CurProdStats(productionMassIfFull: prodMassIfFull);
#warning if production will be done this frame, could request just enough energy to complete it rather than the usual amount
                ReqEnergy = ElectricalEnergy.CreateFromJoules
                (
                    valueInJ: reqEnergyHistoricRounder.Round
                    (
                        value: (decimal)curProdStats.ReqWatts * (decimal)proporUtilized * (decimal)CurWorldManager.Elapsed.TotalSeconds,
                        curTime: CurWorldManager.CurTime
                    )
                );
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingPropor = proporUtilized * Propor.Create(part: electricalEnergy.ValueInJ, whole: ReqEnergy.ValueInJ)!.Value;
            }

            /// <summary>
            /// This will not remove no longer needed building components until production cycle is done since fix current max production amount
            /// and some other production stats at the start of production cycle
            /// </summary>
            public void Update()
            {
                buildingParams.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                AreaDouble areaProduced = AreaDouble.CreateFromMetSq(valueInMetSq: workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curProdStats.ProducedAreaPerSec),
                    areaInProduction = buildingParams.productParams.usefulArea.ToDouble() * productionAmount;
                donePropor = Propor.CreateByClamp((UDouble)donePropor + areaProduced.valueInMetSq / areaInProduction.valueInMetSq);
                if (donePropor.IsFull)
                {
                    buildingParams.nodeState.StoredResPile.TransformFrom(source: resInUse, recipe: recipe);
                    buildingParams.RemoveUnneededBuildingComponents(buildingResPile: buildingResPile);
                }
            }

            public void Delete()
            {
                buildingParams.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                buildingParams.nodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
                buildingParams.nodeState.StoredResPile.TransferAllFrom(source: resInUse);
            }
        }

        public string Name
            => buildingParams.name;

        public Material? SurfaceMaterial
            => buildingParams.surfaceMaterial;

        public IHUDElement UIElement
            => throw new NotImplementedException();
        
        public IEvent<IDeletedListener> Deleted
            => deleted;

        public IBuildingImage BuildingImage
            => buildingParams.buildingImage;

        private readonly ConcreteBuildingParams buildingParams;
        private readonly ResPile buildingResPile;
        private readonly ConcreteProductionParams productionParams;
        private readonly Event<IDeletedListener> deleted;
        private Result<State, TextErrors> stateOrReasonForNotStartingProduction;

        private Manufacturing(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ConcreteProductionParams productionParams)
        {
            this.buildingParams = buildingParams;
            this.buildingResPile = buildingResPile;
            this.productionParams = productionParams;
            deleted = new();
            stateOrReasonForNotStartingProduction = new(errors: new("Not yet initialized"));
        }

        public AllResAmounts TargetStoredResAmounts()
            => productionParams.CurProduct.SwitchExpression
            (
                ok: product => product.Recipe.ingredients * buildingParams.MaxProductAmount() * buildingParams.nodeState.MaxBatchDemResStored,
                error: _ => AllResAmounts.empty
            );

        public void FrameStartNoProduction(string error)
        {
            throw new NotImplementedException();
        }

        public void FrameStart()
        {
            stateOrReasonForNotStartingProduction = stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.IsDone ? State.Create(buildingParams: buildingParams, buildingResPile: buildingResPile, productionParams: productionParams) : new(ok: state),
                error: _ => State.Create(buildingParams: buildingParams, buildingResPile: buildingResPile, productionParams: productionParams)
            );
            stateOrReasonForNotStartingProduction.PerformAction
            (
                action: state => state.FrameStart()
            );
        }

        public IIndustry? Update()
        {
            stateOrReasonForNotStartingProduction.PerformAction(action: state => state.Update());
            return this;
        }

        private void Delete()
        {
            stateOrReasonForNotStartingProduction.PerformAction(action: state => state.Delete());
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        public string GetInfo()
        {
            throw new NotImplementedException();
        }

        EnergyPriority IEnergyConsumer.EnergyPriority
            => buildingParams.energyPriority;

        NodeID IEnergyConsumer.NodeID
            => buildingParams.nodeState.NodeID;

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.ReqEnergy,
                error: _ => ElectricalEnergy.zero
            );

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            => stateOrReasonForNotStartingProduction.SwitchStatement
            (
                ok: state => state.ConsumeElectricalEnergy(source: source, electricalEnergy: electricalEnergy),
                error: _ => Debug.Assert(electricalEnergy.IsZero)
            );
    }
}