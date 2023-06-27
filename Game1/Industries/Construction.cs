using Game1.Collections;
using Game1.Delegates;
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

                //MaterialChoices neededMaterialChoices = buildingMatChoices.FilterOutUnneededMaterials(ingredients: buildingGeneralParams.buildingCostPropors);
                //return ResAndIndustryAlgos.BuildingCost
                //(
                //    buildingCostPropors: buildingGeneralParams.buildingCostPropors,
                //    neededBuildingMatChoices: neededMaterialChoices,
                //    surfaceLength: nodeState.SurfaceLength
                //).Select
                //(
                //    buildingCost => new ConcreteParams
                //    (
                //        nodeState: nodeState,
                //        generalParams: this,
                //        buildingConcreteParams: ,
                //        neededBuildingMatChoices: neededMaterialChoices,
                //        buildingCost: buildingCost
                //    )
                //);
        }

        [Serializable]
        public readonly struct ConcreteParams
        {
            public readonly string name;
            public readonly IIndustryFacingNodeState nodeState;
            public readonly EnergyPriority energyPriority;
            public readonly SomeResAmounts<IResource> buildingCost;
            public readonly AreaDouble buildingComponentsTargetArea;

            private readonly IBuildingConcreteParams buildingConcreteParams;
            /// <summary>
            /// Keys contain ALL material purposes, not just used ones
            /// </summary>
            private readonly EfficientReadOnlyDictionary<IMaterialPurpose, Propor> buildingMaterialPropors;

            public ConcreteParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, IBuildingConcreteParams buildingConcreteParams)
            {
                name = generalParams.name;
                this.nodeState = nodeState;
                energyPriority = generalParams.energyPriority;
                buildingCost = buildingConcreteParams.BuildingCost;
                buildingComponentsTargetArea = ResAndIndustryAlgos.BuildingComponentTargetArea
                (
                    buildingArea: buildingConcreteParams.IncompleteBuildingImage(donePropor: Propor.full).Area
                );

                this.buildingConcreteParams = buildingConcreteParams;
                buildingMaterialPropors = generalParams.buildingGeneralParams.BuildingComponentMaterialPropors;
            }

            public IBuildingImage IncompleteBuildingImage(Propor donePropor)
                => buildingConcreteParams.IncompleteBuildingImage(donePropor: donePropor);

            public Construction CreateIndustry()
                => new(parameters: this);

            public IIndustry CreateChildIndustry(ResPile buildingResPile)
                => buildingConcreteParams.CreateIndustry(buildingResPile: buildingResPile);

            public CurProdStats CurConstrStats()
                => ResAndIndustryAlgos.CurConstrStats
                (
                    buildingMaterialPropors: buildingMaterialPropors,
                    gravity: nodeState.SurfaceGravity,
                    temperature: nodeState.Temperature
                );
        }

        [Serializable]
        private sealed class State
        {
            //private sealed class CurBuildingImage : IBuildingImage
            //{
            //    private readonly State state;

            //    public CurBuildingImage(State state)
            //    {
            //        this.state = state;
            //    }

            //    AngleArc.Params ILightBlockingObject.BlockedAngleArcParams(MyVector2 lightPos)
            //        => state.parameters.incompleteBuildingImage

            //    void IBuildingImage.Draw(Color otherColor, Propor otherColorPropor)
            //        => state.parameters.incompleteBuildingImage.DrawIncomplete(donePropor: state.donePropor, otherColor: otherColor, otherColorPropor: otherColorPropor);
            //}

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
            public IBuildingImage IncompleteBuildingImage
                => parameters.IncompleteBuildingImage(donePropor: donePropor);

            private readonly ConcreteParams parameters;
            private readonly ResPile buildingResPile;
            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
            private readonly HistoricRounder reqEnergyHistoricRounder;
            private CurProdStats curConstrStats;
            private Propor donePropor, workingPropor;

            private State(ResPile buildingResPile, ConcreteParams parameters)
            {
                this.buildingResPile = buildingResPile;
                this.parameters = parameters;
                donePropor = Propor.empty;
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
                AreaDouble areaConstructed = AreaDouble.CreateFromMetSq(valueInMetSq: workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curConstrStats.ProducedAreaPerSec);
                donePropor = Propor.CreateByClamp(value: (UDouble)donePropor + areaConstructed.valueInMetSq / parameters.buildingComponentsTargetArea.valueInMetSq);

                if (donePropor.IsFull)
                {
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

            //public void Draw(Color otherColor, Propor otherColorPropor)
            //    => parameters.DrawBuilding(donePropor: donePropor, otherColor: otherColor, otherColorPropor: otherColorPropor);

            //MyVector2 Disk.IParams.Center
            //    => parameters.nodeState.Position;

            //UDouble Disk.IParams.radius
            //    => MyMathHelper.Sqrt(parameters.nodeState.Area.valueInMetSq + donePropor * parameters.buildingComponentsTargetArea / MyMathHelper.pi);
        }

        public string Name
            => parameters.name;

        public IBuildingImage BuildingImage
            => stateOrReasonForNotStartingConstr.SwitchExpression
            (
                ok: state => state.IncompleteBuildingImage,
                error: _ => parameters.IncompleteBuildingImage(donePropor: Propor.empty)
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

        private Construction(ConcreteParams parameters)
        {
            this.parameters = parameters;
            stateOrReasonForNotStartingConstr = new(errors: new("Not yet initialized"));
            deleted = new();

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
                CurWorldManager.PublishMessage
                (
                    message: new BasicMessage
                    (
                        nodeID: parameters.nodeState.NodeID,
                        message: UIAlgorithms.ConstructionComplete(buildingName: childIndustry.Name)
                    )
                );
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
