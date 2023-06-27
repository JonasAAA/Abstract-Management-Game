using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
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

            public GeneralParams(string name, GeneralProdAndMatAmounts buildingCostPropors, EnergyPriority energyPriority, Product.Params productParams)
            {
                Name = name;
                BuildingComponentMaterialPropors = buildingCostPropors.materialPropors;

                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, color: ActiveUIManager.colorConfig.manufacturingBuildingColor);
                if (buildingCostPropors.materialPropors[IMaterialPurpose.roofSurface].IsEmpty)
                    throw new ArgumentException();
                this.buildingCostPropors = buildingCostPropors;
                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.productParams = productParams;
            }

            public Result<IBuildingConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
            {
                var buildingImage = buildingImageParams.CreateImage(nodeState);
                return ResAndIndustryAlgos.BuildingCost
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatChoices: neededBuildingMatChoices,
                    buildingArea: buildingImage.Area
                ).Select<IBuildingConcreteParams>
                (
                    buildingCost => new ConcreteBuildingParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        buildingImage: buildingImage,
                        buildingCost: buildingCost,
                        buildingMatChoices: neededBuildingMatChoices,
                        surfaceMaterial: neededBuildingMatChoices[IMaterialPurpose.roofSurface]
                    )
                );
            }

            //public IIndustry CreateIndustry(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices, ResPile buildingResPile)
            //    => new ConcreteParams
            //    (
            //        nodeState: nodeState,
            //        surfaceMaterial: buildingMatChoices[IMaterialPurpose.roofSurface],
            //        buildingResPile: buildingResPile,
            //        generalParams: this,
            //        buildingMatChoices: buildingMatChoices
            //    ).CreateIndustry
            //    (
            //        productionParams: new(productParams)
            //    );
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : IBuildingConcreteParams
        {
            public SomeResAmounts<IResource> BuildingCost { get; }

            public readonly string name;
            public readonly IIndustryFacingNodeState nodeState;
            public readonly DiskBuildingImage buildingImage;
            public readonly Material surfaceMaterial;
            public readonly EnergyPriority energyPriority;
            public readonly Product.Params productParams;
            public readonly ulong maxProductAmount;

            private readonly AreaDouble buildingArea;
            private readonly GeneralParams generalParams;
            private readonly MaterialChoices buildingMatChoices;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, DiskBuildingImage buildingImage, SomeResAmounts<IResource> buildingCost, MaterialChoices buildingMatChoices, Material surfaceMaterial)
            {
                BuildingCost = buildingCost;

                name = generalParams.Name;
                this.nodeState = nodeState;
                this.buildingImage = buildingImage;
                this.surfaceMaterial = surfaceMaterial;
                energyPriority = generalParams.energyPriority;
                productParams = generalParams.productParams;
                buildingArea = buildingImage.Area;
                maxProductAmount = ResAndIndustryAlgos.MaxAmountInProduction
                (
                    areaInProduction: ResAndIndustryAlgos.AreaInProduction(buildingArea: buildingArea),
                    itemTargetArea: productParams.targetArea
                );

                this.generalParams = generalParams;
                this.buildingMatChoices = buildingMatChoices;
            }

            /// <param Name="productionMassIfFull">Mass of stuff in production if industry was fully operational</param>
            public CurProdStats CurProdStats(Mass productionMassIfFull)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.buildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature,
                    buildingArea: buildingArea,
                    productionMass: productionMassIfFull
                );

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            ///// <param Name="productionMassIfFull">Mass of stuff in production</param>
            //public CurMechProdStats CurMechProdStats(Mass productionMassIfFull)
            //{
            //    UDouble relevantMassPUS = ResAndIndustryAlgos.RelevantMassPUS
            //    (
            //        buildingMatPropors: generalParams.buildingCostPropors.buildingMaterialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        productionMassPUS: productionMassIfFull.valueInKg / nodeState.SurfaceLength
            //    );

            //    UDouble maxMechThroughputPUS = ResAndIndustryAlgos.MaxMechThroughputPUS
            //    (
            //        buildingMatPropors: generalParams.buildingCostPropors.buildingMaterialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        gravity: nodeState.SurfaceGravity,
            //        temperature: nodeState.Temperature,
            //        relevantMassPUS: relevantMassPUS
            //    );

            //    UDouble maxElectricalPowerPUS = ResAndIndustryAlgos.MaxElectricalPowerPUS
            //    (
            //        buildingMatPropors: generalParams.buildingCostPropors.buildingMaterialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        temperature: nodeState.Temperature
            //    );

            //    UDouble electricalEnergyPerUnitArea = ResAndIndustryAlgos.ElectricalEnergyPerUnitAreaPhys
            //    (
            //        buildingMatPropors: generalParams.buildingCostPropors.buildingMaterialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        gravity: nodeState.SurfaceGravity,
            //        temperature: nodeState.Temperature,
            //        relevantMassPUS: relevantMassPUS
            //    );

            //    UDouble
            //        reqWattsPUS = MyMathHelper.Min(maxElectricalPowerPUS, maxMechThroughputPUS * electricalEnergyPerUnitArea),
            //        reqWatts = reqWattsPUS * nodeState.SurfaceLength,
            //        producedAreaPerSec = reqWatts / electricalEnergyPerUnitArea;

            //    return new
            //    (
            //        ReqWatts: reqWatts,
            //        ProducedAreaPerSec: producedAreaPerSec
            //    );
            //}

            public IIndustry CreateIndustry(ResPile buildingResPile)
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
                        var resInUseAndCount = ResPile.CreateMultipleIfHaveEnough
                        (
                            source: buildingParams.nodeState.StoredResPile,
                            amount: product.Recipe.ingredients,
                            maxCount: buildingParams.maxProductAmount
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
                                    productionAmount: count
                                )
                            ),
                            null => new(errors: new(UIAlgorithms.NotEnoughResourcesToStartProduction))
                        };
                    }
                );
            // This is if want to only allow to start production if the building would be fully untilized
            //=> productionParams.CurProduct.SelectMany
            //(
            //    product =>
            //    {
            //        ResRecipe recipe = product.Recipe * buildingParams.maxProductAmount;
            //        var resInUse = ResPile.CreateIfHaveEnough
            //        (
            //            source: buildingParams.nodeState.StoredResPile,
            //            amount: recipe.ingredients
            //        );
            //        if (resInUse is null)
            //            return new(errors: new(UIAlgorithms.NotEnoughResourcesToStartProduction));
            //        return new Result<State, TextErrors>
            //        (
            //            ok: new
            //            (
            //                buildingParams: buildingParams,
            //                buildingResPile: buildingResPile,
            //                resInUse: resInUse,
            //                recipe: recipe
            //            )
            //        );
            //    }
            //);

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool IsDone
                => donePropor.IsFull;

            private readonly ConcreteBuildingParams buildingParams;
            private readonly ResPile buildingResPile;
            private readonly ResPile resInUse;
            private readonly ResRecipe recipe;
            private readonly Mass prodMassIfFull;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private readonly ulong productionAmount;
            private readonly Propor proporUtilized;

            private CurProdStats curProdStats;
            private Propor donePropor, workingPropor;

            private State(ConcreteBuildingParams buildingParams, ResPile buildingResPile, ResPile resInUse, ResRecipe productRecipe, ulong productionAmount)
            {
                this.buildingParams = buildingParams;
                this.buildingResPile = buildingResPile;
                this.resInUse = resInUse;
                recipe = productRecipe * productionAmount;
                prodMassIfFull = productRecipe.ingredients.Mass() * buildingParams.maxProductAmount;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.nodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                this.productionAmount = productionAmount;
                proporUtilized = Propor.Create(part: productionAmount, whole: buildingParams.maxProductAmount)!.Value;
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

            public void Update()
            {
                buildingParams.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                AreaDouble areaProduced = AreaDouble.CreateFromMetSq(valueInMetSq: workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curProdStats.ProducedAreaPerSec),
                    areaInProduction = buildingParams.productParams.targetArea.ToDouble() * productionAmount;
                donePropor = Propor.CreateByClamp((UDouble)donePropor + areaProduced.valueInMetSq / areaInProduction.valueInMetSq);
                if (donePropor.IsFull)
                    buildingParams.nodeState.StoredResPile.TransformFrom(source: resInUse, recipe: recipe);
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

        public SomeResAmounts<IResource> TargetStoredResAmounts()
            => productionParams.CurProduct.SwitchExpression
            (
                ok: product => product.Recipe.ingredients * buildingParams.maxProductAmount * buildingParams.nodeState.MaxBatchDemResStored,
                error: _ => SomeResAmounts<IResource>.empty
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