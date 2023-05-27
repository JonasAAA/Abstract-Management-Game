using Game1.Collections;
using Game1.Delegates;
using Game1.Lighting;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Manufacturing : IIndustry
    {
        [Serializable]
        public sealed class GeneralParams : IConstructedIndustryGeneralParams
        {
            public string Name { get; }
            public GeneralProdAndMatAmounts BuildingCostPropors { get; }
            
            public readonly EnergyPriority energyPriority;
            public readonly Product.Params productParams;

            public GeneralParams(string name, GeneralProdAndMatAmounts buildingCostPropors, EnergyPriority energyPriority, Product.Params productParams)
            {
                Name = name;
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
                this.nodeState = nodeState;
                this.surfaceMaterial = surfaceMaterial;
                this.buildingResPile = buildingResPile;
                this.generalParams = generalParams;
                this.buildingMatChoices = buildingMatChoices;
                energyPriority = generalParams.energyPriority;
                productParams = generalParams.productParams;
                productAmount = ResAndIndustryAlgos.AmountInProduction
                (
                    surfaceLength: nodeState.ApproxSurfaceLength,
                    itemTargetArea: productParams.TargetArea
                );
            }

            /// <param Name="productionMass">Mass of stuff in production</param>
            public CurProdStats CurProdStats(Mass productionMass)
            {
                UDouble relevantMassPUS = ResAndIndustryAlgos.RelevantMassPUS
                (
                    buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
                    buildingMatChoices: buildingMatChoices,
                    productionMassPUS: productionMass.valueInKg / nodeState.ApproxSurfaceLength
                );

                UDouble maxMechThroughputPUS = ResAndIndustryAlgos.MaxMechThroughputPUS
                (
                    buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature,
                    relevantMassPUS: relevantMassPUS
                );

                UDouble maxElectricalPowerPUS = ResAndIndustryAlgos.MaxElectricalPowerPUS
                (
                    buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
                    buildingMatChoices: buildingMatChoices,
                    temperature: nodeState.Temperature
                );

                UDouble electricalEnergyPerUnitArea = ResAndIndustryAlgos.ElectricalEnergyPerUnitAreaPhys
                (
                    buildingMatPropors: generalParams.BuildingCostPropors.materialPropors,
                    buildingMatChoices: buildingMatChoices,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature,
                    relevantMassPUS: relevantMassPUS
                );

                UDouble
                    reqWattsPUS = MyMathHelper.Min(maxElectricalPowerPUS, maxMechThroughputPUS * electricalEnergyPerUnitArea),
                    reqWatts = reqWattsPUS * nodeState.ApproxSurfaceLength,
                    producedAreaPerSec = reqWatts / electricalEnergyPerUnitArea;

                return new
                (
                    ReqWatts: reqWatts,
                    ProducedAreaPerSec: producedAreaPerSec
                );
            }

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
        public readonly record struct CurProdStats(UDouble ReqWatts, UDouble ProducedAreaPerSec);

        [Serializable]
        private sealed class Production
        {
            public static Result<Production, TextErrors> Create(ResPile source, ResRecipe recipe)
            {
                var resInUse = ResPile.CreateIfHaveEnough(source: source, amount: recipe.ingredients);
                if (resInUse is null)
                    return new(errors: new("not enough resources to start production"));
                return new(ok: new(resInUse: resInUse, recipe: recipe));
            }

            public Propor DonePropor
                => (Propor)donePropor;

            public bool IsDone
                => donePropor >= 1;

            public readonly Mass mass;

            private readonly ResPile resInUse;
            private readonly ResRecipe recipe;
            private UDouble donePropor;

            private Production(ResPile resInUse, ResRecipe recipe)
            {
                this.resInUse = resInUse;
                this.recipe = recipe;
                mass = recipe.ingredients.Mass();
                donePropor = 0;
            }

            public void Update(in ConcreteBuildingParams buildingParams, in CurProdStats curProdStats, Propor workingPropor)
            {
                UDouble areaProduced = workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curProdStats.ProducedAreaPerSec,
                    areaInProduction = buildingParams.productParams.TargetArea.valueInMetSq * buildingParams.productAmount;
                donePropor += areaProduced / areaInProduction;
                if (donePropor >= 1)
                {
                    buildingParams.nodeState.StoredResPile.TransformFrom(source: resInUse, recipe: recipe);
                    donePropor = 1;
                }
            }

            public void Delete(ResPile resDestin)
                => resDestin.TransferAllFrom(resInUse);
        }

        private readonly record struct State(Production Production, CurProdStats CurProdStats, ElectricalEnergy ReqEnergy);

        public ILightBlockingObject? LightBlockingObject
            => throw new NotImplementedException();

        public Material? SurfaceMaterial
            => buildingParams.surfaceMaterial;

        public IHUDElement UIElement
            => throw new NotImplementedException();
        
        public IEvent<IDeletedListener> Deleted
            => deleted;

        private readonly ConcreteBuildingParams buildingParams;
        private readonly ConcreteProductionParams productionParams;
        private readonly HistoricRounder reqEnergyHistoricRounder;
        private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
        private readonly Event<IDeletedListener> deleted;
        private Result<State, TextErrors> stateOrReasonForNotStartingProduction;
        private Propor workingPropor;

        private Manufacturing(ConcreteBuildingParams buildingParams, ConcreteProductionParams productionParams)
        {
            this.buildingParams = buildingParams;
            this.productionParams = productionParams;
            reqEnergyHistoricRounder = new();
            electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: buildingParams.nodeState.LocationCounters);
            stateOrReasonForNotStartingProduction = new(errors: new("Not yet initialized"));
            deleted = new();

            CurWorldManager.EnergyDistributor.AddEnergyConsumer(energyConsumer: this);
        }

        public void DrawAfterPlanet()
        {
            throw new NotImplementedException();
        }

        public void DrawBeforePlanet(Color otherColor, Propor otherColorPropor)
        {
            throw new NotImplementedException();
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
            Result<Production, TextErrors> productionOrErr = stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.Production.IsDone ? CreateProduction() : new(ok: state.Production),
                error: _ => CreateProduction()
            );
            stateOrReasonForNotStartingProduction = productionOrErr.Select
            (
                production =>
                {
                    var curProdStats = buildingParams.CurProdStats
                    (
                        productionMass: production.mass
                    );
                    return new State
                    (
                        Production: production,
                        CurProdStats: curProdStats,
#warning if production will be done this frame, could request just enough energy to complete it rather than the usual amount
                        ReqEnergy: ElectricalEnergy.CreateFromJoules
                        (
                            valueInJ: reqEnergyHistoricRounder.Round
                            (
                                value: (decimal)curProdStats.ReqWatts * (decimal)CurWorldManager.Elapsed.TotalSeconds,
                                curTime: CurWorldManager.CurTime
                            )
                        )
                    );
                }
            );

            return;

            Result<Production, TextErrors> CreateProduction()
                => productionParams.CurProduct.SelectMany
                (
                    product => Production.Create
                    (
                        source: buildingParams.nodeState.StoredResPile,
                        recipe: product.Recipe * buildingParams.productAmount
                    )
                );
        }

        public IIndustry? Update()
        {
            stateOrReasonForNotStartingProduction.PerformAction
            (
                action: state =>
                {
                    state.Production.Update(buildingParams: in buildingParams, curProdStats: state.CurProdStats, workingPropor: workingPropor);
                    buildingParams.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                }
            );
            return this;
        }

        private void Delete()
        {
            stateOrReasonForNotStartingProduction.PerformAction
            (
                action: state =>
                {
                    buildingParams.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                    buildingParams.nodeState.StoredResPile.TransferAllFrom(source: buildingParams.buildingResPile);
                    state.Production.Delete(resDestin: buildingParams.nodeState.StoredResPile);
                }
            );
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
        {
            electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
            workingPropor = stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => Propor.Create(part: electricalEnergy.ValueInJ, whole: state.ReqEnergy.ValueInJ)!.Value,
                error: _ => Propor.empty
            );
        }
    }
}