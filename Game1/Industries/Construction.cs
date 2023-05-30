using Game1.Collections;
using Game1.Delegates;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Construction : IIndustry
    {
        [Serializable]
        public sealed class GeneralParams
        {
            public readonly IConstructedIndustryGeneralParams childIndustryGenParams;
            public readonly EnergyPriority energyPriority;

            public GeneralParams(IConstructedIndustryGeneralParams childIndustryGenParams, EnergyPriority energyPriority)
            {
                this.childIndustryGenParams = childIndustryGenParams;
                this.energyPriority = energyPriority;
            }

            public Result<ConcreteParams, TextErrors> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices buildingMatChoices)
            {
                MaterialChoices neededMaterialChoices = buildingMatChoices.FilterOutUnneededMaterials(ingredients: childIndustryGenParams.BuildingCostPropors);
                return ResAndIndustryAlgos.BuildingCost
                (
                    buildingCostPropors: childIndustryGenParams.BuildingCostPropors,
                    buildingMatChoices: neededMaterialChoices,
                    surfaceLength: nodeState.SurfaceLength
                ).Select
                (
                    buildingCost => new ConcreteParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        buildingMatChoices: neededMaterialChoices,
                        buildingCost: buildingCost
                    )
                );
            }
        }

        [Serializable]
        public readonly struct ConcreteParams
        {
            public readonly IIndustryFacingNodeState nodeState;
            public readonly EnergyPriority energyPriority;
            public readonly SomeResAmounts<IResource> buildingCost;
            public readonly UDouble buildingArea;
            public readonly Color childIndustryColor;

            private readonly GeneralParams generalParams;
            private readonly MaterialChoices buildingMatChoices;
            private readonly IConstructedIndustryGeneralParams childIndustryGenParams;
            
            public ConcreteParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, MaterialChoices buildingMatChoices, SomeResAmounts<IResource> buildingCost)
            {
                this.nodeState = nodeState;
                energyPriority = generalParams.energyPriority;
                this.buildingCost = buildingCost;
                buildingArea = ResAndIndustryAlgos.BuildingArea(surfaceLength: nodeState.SurfaceLength);
                childIndustryColor = generalParams.childIndustryGenParams.Color;

                this.generalParams = generalParams;
                this.buildingMatChoices = buildingMatChoices;
                childIndustryGenParams = generalParams.childIndustryGenParams;
            }

            public Construction CreateIndustry()
                => new(parameters: this);

            public IIndustry CreateChildIndustry(ResPile buildingResPile)
                => childIndustryGenParams.CreateIndustry(nodeState: nodeState, buildingMatChoices: buildingMatChoices, buildingResPile: buildingResPile);

            public CurProdStats CurConstrStats()
                => ResAndIndustryAlgos.CurConstrStats
                (
                    buildingCostPropors: generalParams.childIndustryGenParams.BuildingCostPropors,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature
                );
        }

        //[Serializable]
        //public readonly record struct CurConstrStats(UDouble ReqWatts, UDouble ConstructedAreaPerSec);

        [Serializable]
        private sealed class State : Disk.IParams
        {
            public static Result<State, TextErrors> Create(ConcreteParams parameters)
            {
                var buildingResPile = ResPile.CreateIfHaveEnough
                (
                    source: parameters.nodeState.StoredResPile,
                    amount: parameters.buildingCost
                );
                if (buildingResPile is null)
                    return new(errors: new("not enough resources to start construction"));
                return new(ok: new State(buildingResPile: buildingResPile, parameters: parameters));
            }

            public ElectricalEnergy ReqEnergy { get; private set; }
            public readonly LightBlockingDisk lightBlockingDisk;

            private readonly ConcreteParams parameters;
            private readonly ResPile buildingResPile;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private CurProdStats curConstrStats;
            private UDouble donePropor;
            private Propor workingPropor;

            private State(ResPile buildingResPile, ConcreteParams parameters)
            {
                lightBlockingDisk = new(parameters: this);

                this.buildingResPile = buildingResPile;
                this.parameters = parameters;
                donePropor = 0;
                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: parameters.nodeState.LocationCounters);
                reqEnergyHistoricRounder = new();
            }

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
                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                UDouble areaConstructed = workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curConstrStats.ProducedAreaPerSec;
                donePropor += areaConstructed / parameters.buildingArea;

                if (donePropor >= 1)
                {
                    donePropor = 1;
                    var childIndustry = parameters.CreateChildIndustry(buildingResPile: buildingResPile);
                    if (childIndustry is IEnergyConsumer energyConsumer)
                        CurWorldManager.EnergyDistributor.AddEnergyConsumer(energyConsumer: energyConsumer);
                    return childIndustry;
                }
                return null;
            }

            public void Delete()
            {
                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
                parameters.nodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
            }

            public void Draw(Color otherColor, Propor otherColorPropor)
                => lightBlockingDisk.Draw
                (
                    baseColor: parameters.childIndustryColor,
                    otherColor: otherColor,
                    otherColorPropor: otherColorPropor
                );

            MyVector2 Disk.IParams.Center
                => parameters.nodeState.Position;

            UDouble Disk.IParams.Radius
                => MyMathHelper.Sqrt(parameters.nodeState.Area.valueInMetSq + donePropor * parameters.buildingArea / MyMathHelper.pi);
        }

        [Serializable]
        public readonly record struct IndustryOutlineParams(ConcreteParams Parameters) : Disk.IParams
        {
            public MyVector2 Center
                => Parameters.nodeState.Position;

            public UDouble Radius
                => Parameters.nodeState.Radius + ResAndIndustryAlgos.BuildingHeight;
        }

        public ILightBlockingObject? LightBlockingObject
            => stateOrReasonForNotStartingConstr.SwitchExpression<ILightBlockingObject?>
            (
                ok: state => state.lightBlockingDisk,
                error: _ => null
            );

        public Material? SurfaceMaterial
            => stateOrReasonForNotStartingConstr.SwitchExpression<Material?>
            (
                // NEW: May want reflexivity and the other number be some combination of planet reflexivity, final building reflexivity, and building
                // raw material internals reflexivity. E.g. first third is mix of planet and building internals, middle third is just building internals,
                // and the last third is mix of building internals and final building
                //
                // would want to return the mix of materials that the building consists of.
                // COULD also always return null or the surface material of the finished building, but that doesn't make much sense
                // though is simple to understand and to implement
                ok: state => throw new NotImplementedException(),
                error: _ => null
            );

        public IHUDElement UIElement => throw new NotImplementedException();

        public IEvent<IDeletedListener> Deleted
            => deleted;

        private readonly ConcreteParams parameters;
        private readonly Event<IDeletedListener> deleted;
        private Result<State, TextErrors> stateOrReasonForNotStartingConstr;
        private readonly Disk industryOutline;

        private Construction(ConcreteParams parameters)
        {
            this.parameters = parameters;
            stateOrReasonForNotStartingConstr = new(errors: new("Not yet initialized"));
            deleted = new();
            industryOutline = new(parameters: new IndustryOutlineParams(Parameters: parameters));

            CurWorldManager.EnergyDistributor.AddEnergyConsumer(energyConsumer: this);
        }

        public SomeResAmounts<IResource> TargetStoredResAmounts()
            => stateOrReasonForNotStartingConstr.SwitchExpression
            (
                ok: state => parameters.buildingCost,
                error: _ => SomeResAmounts<IResource>.empty
            );

        public void FrameStartNoProduction(string error)
        {
            throw new NotImplementedException();
        }

        public void FrameStart()
        {
            stateOrReasonForNotStartingConstr = stateOrReasonForNotStartingConstr.SwitchExpression
            (
                ok: state => new(ok: state),
                error: _ => State.Create(parameters: parameters)
            );

            stateOrReasonForNotStartingConstr.PerformAction(action: state => state.FrameStart());
        }

        public IIndustry? Update()
        {
            var childIndustry = stateOrReasonForNotStartingConstr.SwitchExpression
            (
                ok: state => state.Update(),
                error: _ => null
            );

            if (childIndustry is not null)
            {
                stateOrReasonForNotStartingConstr = new(errors: new("construction is done"));
                Delete();
                return childIndustry;
            }
            return this;
        }

        private void Delete()
        {
            stateOrReasonForNotStartingConstr.PerformAction(action: state => state.Delete());
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        public string GetInfo()
        {
            throw new NotImplementedException();
        }

        public void Draw(Color otherColor, Propor otherColorPropor)
        {
            Propor transparency = (Propor).25;
            industryOutline.Draw(baseColor: parameters.childIndustryColor * (float)transparency, otherColor: otherColor * (float)transparency, otherColorPropor: otherColorPropor * transparency);
            stateOrReasonForNotStartingConstr.PerformAction(action: state => state.Draw(otherColor: otherColor, otherColorPropor: otherColorPropor));
        }

        EnergyPriority IEnergyConsumer.EnergyPriority
            => parameters.energyPriority;

        NodeID IEnergyConsumer.NodeID
            => parameters.nodeState.NodeID;

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => stateOrReasonForNotStartingConstr.SwitchExpression
            (
                ok: state => state.ReqEnergy,
                error: _ => ElectricalEnergy.zero
            );

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
            => stateOrReasonForNotStartingConstr.SwitchStatement
            (
                ok: state => state.ConsumeElectricalEnergy(source: source, electricalEnergy: electricalEnergy),
                error: _ => Debug.Assert(electricalEnergy.IsZero)
            );
    }
}
