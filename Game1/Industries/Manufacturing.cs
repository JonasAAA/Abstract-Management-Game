using Game1.Collections;
using Game1.Delegates;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Manufacturing : IIndustry, Disk.IParams
    {
        [Serializable]
        public sealed class GeneralParams : IConstructedIndustryGeneralParams
        {
            public string Name { get; }
            public Color Color { get; }
            public GeneralProdAndMatAmounts BuildingCostPropors { get; }
            
            public readonly EnergyPriority energyPriority;
            public readonly Product.Params productParams;

            public GeneralParams(string name, Color color, GeneralProdAndMatAmounts buildingCostPropors, EnergyPriority energyPriority, Product.Params productParams)
            {
                Name = name;
                Color = color;
                if (buildingCostPropors.materialPropors[IMaterialPurpose.roofSurface] == Propor.empty)
                    throw new ArgumentException();
                BuildingCostPropors = buildingCostPropors;
                if (energyPriority == EnergyPriority.mostImportant)
                    throw new ArgumentException("Only power plants can have highest energy priority");
                this.energyPriority = energyPriority;
                this.productParams = productParams;
            }

            //public Result<ConcreteBuildingParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices)
            //{
            //    throw new NotImplementedException();
            //}

            public IIndustry CreateIndustry(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices, ResPile buildingResPile)
                => new ConcreteBuildingParams
                (
                    nodeState: nodeState,
                    surfaceMaterial: buildingMatChoices[IMaterialPurpose.roofSurface],
                    buildingResPile: buildingResPile,
                    generalParams: this,
                    buildingMatChoices: buildingMatChoices
                ).CreateIndustry
                (
                    productionParams: new(productParams)
                );
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams
        {
            public readonly Color color;
            public readonly IIndustryFacingNodeState nodeState;
            public readonly Material surfaceMaterial;
            public readonly EnergyPriority energyPriority;
            public readonly Product.Params productParams;
            public readonly ulong productAmount;
            public readonly ResPile buildingResPile;

            private readonly GeneralParams generalParams;
            private readonly MaterialChoices buildingMatChoices;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, Material surfaceMaterial, ResPile buildingResPile, GeneralParams generalParams, MaterialChoices buildingMatChoices)
            {
                color = generalParams.Color;
                this.nodeState = nodeState;
                this.surfaceMaterial = surfaceMaterial;
                energyPriority = generalParams.energyPriority;
                productParams = generalParams.productParams;
                productAmount = ResAndIndustryAlgos.AmountInProduction
                (
                    areaInProduction: ResAndIndustryAlgos.AreaInProduction(surfaceLength: nodeState.SurfaceLength),
                    itemTargetArea: productParams.targetArea
                );
                this.buildingResPile = buildingResPile;

                this.generalParams = generalParams;
                this.buildingMatChoices = buildingMatChoices;
            }

            /// <param Name="productionMass">Mass of stuff in production</param>
            public CurProdStats CurProdStats(Mass productionMass)
                => ResAndIndustryAlgos.CurMechProdStats
                (
                    buildingCostPropors: generalParams.BuildingCostPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature,
                    surfaceLength: nodeState.SurfaceLength,
                    productionMass: productionMass
                );

            ///// <param Name="productionMass">Mass of stuff in production</param>
            //public CurMechProdStats CurMechProdStats(Mass productionMass)
            //{
            //    UDouble relevantMassPUS = ResAndIndustryAlgos.RelevantMassPUS
            //    (
            //        buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        productionMassPUS: productionMass.valueInKg / nodeState.SurfaceLength
            //    );

            //    UDouble maxMechThroughputPUS = ResAndIndustryAlgos.MaxMechThroughputPUS
            //    (
            //        buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        gravity: nodeState.SurfaceGravity,
            //        temperature: nodeState.Temperature,
            //        relevantMassPUS: relevantMassPUS
            //    );

            //    UDouble maxElectricalPowerPUS = ResAndIndustryAlgos.MaxElectricalPowerPUS
            //    (
            //        buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
            //        buildingMatChoices: buildingMatChoices,
            //        temperature: nodeState.Temperature
            //    );

            //    UDouble electricalEnergyPerUnitArea = ResAndIndustryAlgos.ElectricalEnergyPerUnitAreaPhys
            //    (
            //        buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
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

            public Manufacturing CreateIndustry(ConcreteProductionParams productionParams)
                => new(buildingParams: this, productionParams: productionParams);
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
            public static Result<State, TextErrors> Create(ConcreteBuildingParams buildingParams, ConcreteProductionParams productionParams)
                => productionParams.CurProduct.SelectMany
                (
                    product =>
                    {
                        ResRecipe recipe = product.Recipe * buildingParams.productAmount;
                        var resInUse = ResPile.CreateIfHaveEnough
                        (
                            source: buildingParams.nodeState.StoredResPile,
                            amount: recipe.ingredients
                        );
                        if (resInUse is null)
                            return new(errors: new("not enough resources to start production"));
                        return new Result<State, TextErrors>(ok: new(buildingParams: buildingParams, resInUse: resInUse, recipe: recipe));
                    }
                );

            public ElectricalEnergy ReqEnergy { get; private set; }

            public bool IsDone
                => donePropor >= 1;

            private readonly ConcreteBuildingParams buildingParams;
            private readonly ResPile resInUse;
            private readonly ResRecipe recipe;
            private readonly Mass prodMass;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;

            private CurProdStats curProdStats;
            private UDouble donePropor;
            private Propor workingPropor;

            private State(ConcreteBuildingParams buildingParams, ResPile resInUse, ResRecipe recipe)
            {
                this.buildingParams = buildingParams;
                this.resInUse = resInUse;
                this.recipe = recipe;
                prodMass = recipe.ingredients.Mass();
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.nodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
                donePropor = 0;
            }

            public void FrameStart()
            {
                curProdStats = buildingParams.CurProdStats(productionMass: prodMass);
#warning if production will be done this frame, could request just enough energy to complete it rather than the usual amount
                ReqEnergy = ElectricalEnergy.CreateFromJoules
                (
                    valueInJ: reqEnergyHistoricRounder.Round
                    (
                        value: (decimal)curProdStats.ReqWatts * (decimal)CurWorldManager.Elapsed.TotalSeconds,
                        curTime: CurWorldManager.CurTime
                    )
                );
            }

            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            {
                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
                workingPropor = Propor.Create(part: electricalEnergy.ValueInJ, whole: ReqEnergy.ValueInJ)!.Value;
            }

            public void Update()
            {
                buildingParams.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

                UDouble areaProduced = workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curProdStats.ProducedAreaPerSec,
                    areaInProduction = buildingParams.productParams.targetArea.valueInMetSq * buildingParams.productAmount;
                donePropor += areaProduced / areaInProduction;
                if (donePropor >= 1)
                {
                    buildingParams.nodeState.StoredResPile.TransformFrom(source: resInUse, recipe: recipe);
                    donePropor = 1;
                }
            }

            public void Delete()
            {
                buildingParams.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                buildingParams.nodeState.StoredResPile.TransferAllFrom(source: buildingParams.buildingResPile);
                buildingParams.nodeState.StoredResPile.TransferAllFrom(source: resInUse);
            }
        }

        public ILightBlockingObject? LightBlockingObject
            => lightBlockingDisk;

        public Material? SurfaceMaterial
            => buildingParams.surfaceMaterial;

        public IHUDElement UIElement
            => throw new NotImplementedException();
        
        public IEvent<IDeletedListener> Deleted
            => deleted;

        private readonly ConcreteBuildingParams buildingParams;
        private readonly ConcreteProductionParams productionParams;
        private Result<State, TextErrors> stateOrReasonForNotStartingProduction;
        private readonly Event<IDeletedListener> deleted;
        private readonly LightBlockingDisk lightBlockingDisk;

        private Manufacturing(ConcreteBuildingParams buildingParams, ConcreteProductionParams productionParams)
        {
            this.buildingParams = buildingParams;
            this.productionParams = productionParams;
            stateOrReasonForNotStartingProduction = new(errors: new("Not yet initialized"));
            deleted = new();
            lightBlockingDisk = new(parameters: this);
        }


        public SomeResAmounts<IResource> TargetStoredResAmounts()
            => productionParams.CurProduct.SwitchExpression
            (
                ok: product => product.Recipe.ingredients * buildingParams.productAmount * buildingParams.nodeState.MaxBatchDemResStored,
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
                ok: state => state.IsDone ? State.Create(buildingParams: buildingParams, productionParams: productionParams) : new(ok: state),
                error: _ => State.Create(buildingParams: buildingParams, productionParams: productionParams)
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

        public void Draw(Color otherColor, Propor otherColorPropor)
            => lightBlockingDisk.Draw(baseColor: buildingParams.color, otherColor: otherColor, otherColorPropor: otherColorPropor);

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

        MyVector2 Disk.IParams.Center
            => buildingParams.nodeState.Position;

        UDouble Disk.IParams.Radius
            => buildingParams.nodeState.Radius + ResAndIndustryAlgos.BuildingHeight;
    }
}