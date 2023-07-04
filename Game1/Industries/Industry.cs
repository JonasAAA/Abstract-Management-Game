using Game1.Delegates;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    public static class Industry
    {
        public interface IConcreteBuildingParams<TConcreteProductionParams>
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public IBuildingImage IdleBuildingImage { get; }

            public Material? SurfaceMaterial(bool productionInProgress);
            public AllResAmounts TargetStoredResAmounts(TConcreteProductionParams productionParams);
        }

        public interface IProductionCycleState<TConcreteProductionParams, TConcreteBuildingParams, TPersistentState, TState>
            where TConcreteBuildingParams : struct, IConcreteBuildingParams<TConcreteProductionParams>
            where TState : class, IProductionCycleState<TConcreteProductionParams, TConcreteBuildingParams, TPersistentState, TState>
        {
            /// <summary>
            /// Says if multiple production cycles can happen
            /// E.g. in Manufacturing they can, in Construction they can't
            /// </summary>
            public static abstract bool IsRepeatable { get; }
            
            public static abstract Result<TState, TextErrors> Create(TConcreteProductionParams productionParams, TConcreteBuildingParams buildingParams, TPersistentState persistentState);

            public bool ShouldRestart { get; }
            public ElectricalEnergy ReqEnergy { get; }
            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy);
            /// <summary>
            /// No need to adjust ReqEnergy here, as it's done in Industry implementation
            /// </summary>
            public void FrameStartNoProduction();
            public void FrameStart();
            /// <summary>
            /// Returns child industry if finished construction, null otherwise
            /// </summary>
            public IIndustry? Update();
            public IBuildingImage BusyBuildingImage();
            public void Delete();
        }
    }

    [Serializable]
    public class Industry<TConcreteProductionParams, TConcreteBuildingParams, TPersistentState, TProductionCycleState> : IIndustry, IEnergyConsumer
        where TConcreteBuildingParams : struct, Industry.IConcreteBuildingParams<TConcreteProductionParams>
        where TProductionCycleState : class, Industry.IProductionCycleState<TConcreteProductionParams, TConcreteBuildingParams, TPersistentState, TProductionCycleState>
    {
        public string Name
            => buildingParams.Name;

        public Material? SurfaceMaterial
            => buildingParams.SurfaceMaterial(productionInProgress: Busy);

        public IHUDElement UIElement
            => throw new NotImplementedException();

        public IEvent<IDeletedListener> Deleted
            => deleted;

        public IBuildingImage BuildingImage
            => stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.BusyBuildingImage(),
                error: _ => buildingParams.IdleBuildingImage
            );

        private bool Busy
            => stateOrReasonForNotStartingProduction.isOk;
        private readonly TConcreteProductionParams productionParams;
        private readonly TConcreteBuildingParams buildingParams;
        private readonly TPersistentState persistentState;
        private Result<TProductionCycleState, TextErrors> stateOrReasonForNotStartingProduction;
        private readonly Event<IDeletedListener> deleted;
        private bool paused;

        public Industry(TConcreteProductionParams productionParams, TConcreteBuildingParams buildingParams, TPersistentState persistentState)
        {
            this.productionParams = productionParams;
            this.buildingParams = buildingParams;
            this.persistentState = persistentState;
            stateOrReasonForNotStartingProduction = new(errors: new("Not yet initialized"));
            deleted = new();
            paused = false;

            CurWorldManager.EnergyDistributor.AddEnergyConsumer(energyConsumer: this);
        }

        public AllResAmounts TargetStoredResAmounts()
            => (TProductionCycleState.IsRepeatable || !Busy) switch
            {
                true => buildingParams.TargetStoredResAmounts(productionParams: productionParams),
                false => AllResAmounts.empty
            };

        public void FrameStartNoProduction(string error)
        {
            paused = true;
            stateOrReasonForNotStartingProduction.PerformAction(action: state => state.FrameStartNoProduction());
#warning Complete this
        }

        public void FrameStart()
        {
            paused = false;
            stateOrReasonForNotStartingProduction = stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.ShouldRestart ? TProductionCycleState.Create(productionParams: productionParams, buildingParams: buildingParams, persistentState: persistentState) : new(ok: state),
                error: _ => TProductionCycleState.Create(productionParams: productionParams, buildingParams: buildingParams, persistentState: persistentState)
            );
            stateOrReasonForNotStartingProduction.PerformAction
            (
                action: state => state.FrameStart()
            );
        }

        public IIndustry? Update()
        {
            var childIndustry = stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.Update(),
                error: _ => null
            );

            if (childIndustry is not null)
            {
                stateOrReasonForNotStartingProduction = new(errors: new("construction is done"));
                Delete();
                return childIndustry;
            }
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
            => buildingParams.EnergyPriority;

        NodeID IEnergyConsumer.NodeID
            => buildingParams.NodeState.NodeID;

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => paused switch
            {
                true => ElectricalEnergy.zero,
                false => stateOrReasonForNotStartingProduction.SwitchExpression
                (
                    ok: state => state.ReqEnergy,
                    error: _ => ElectricalEnergy.zero
                )
            };

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
        {
            if (paused)
                return;
            stateOrReasonForNotStartingProduction.SwitchStatement
            (
                ok: state => state.ConsumeElectricalEnergy(source: source, electricalEnergy: electricalEnergy),
                error: _ => Debug.Assert(electricalEnergy.IsZero)
            );
        }
    }
}
