﻿using Game1.Collections;
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

            public MaterialPalette? SurfaceMatPalette(bool productionInProgress);
            public EfficientReadOnlyCollection<IResource> GetProducedResources(TConcreteProductionParams productionParams);
            public AllResAmounts MaxStoredInput(TConcreteProductionParams productionParams);
            public Area MaxStoredOutputArea();
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
            
            public static abstract Result<TState, TextErrors> Create(TConcreteProductionParams productionParams, TConcreteBuildingParams buildingParams, TPersistentState persistentState,
                ResPile inputStorage, Area maxOutputArea);

            public bool ShouldRestart { get; }
            public ElectricalEnergy ReqEnergy { get; }
            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy);
            public void FrameStart();
            /// <summary>
            /// Returns child industry if finished construction, null otherwise
            /// </summary>
            public IIndustry? Update(ResPile outputStorage);
            public IBuildingImage BusyBuildingImage();
            /// <summary>
            /// Dump all stuff into <paramref name="outputStorage"/>
            /// </summary>
            public void Delete(ResPile outputStorage);

            public static abstract void DeletePersistentState(TPersistentState persistentState, ResPile outputStorage);
        }
    }

    [Serializable]
    public sealed class Industry<TConcreteProductionParams, TConcreteBuildingParams, TPersistentState, TProductionCycleState> : IIndustry, IEnergyConsumer
        where TConcreteBuildingParams : struct, Industry.IConcreteBuildingParams<TConcreteProductionParams>
        where TProductionCycleState : class, Industry.IProductionCycleState<TConcreteProductionParams, TConcreteBuildingParams, TPersistentState, TProductionCycleState>
    {
        public string Name
            => buildingParams.Name;

        public NodeID NodeID
            => buildingParams.NodeState.NodeID;

        public MaterialPalette? SurfaceMatPalette
            => buildingParams.SurfaceMatPalette(productionInProgress: Busy);

        public IHUDElement UIElement
            => industryUI;

        public IEvent<IDeletedListener> Deleted
            => deleted;

        public IBuildingImage BuildingImage
            => stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.BusyBuildingImage(),
                error: _ => buildingParams.IdleBuildingImage
            );
        // CURRENTLY this doesn't handle changes in res consumed and res produced. So if change produced material recipe, or choose to recycle different thing,
        // this will not be updated accordingly
        public IHUDElement RoutePanel { get; }

        private bool Busy
            => stateOrReasonForNotStartingProduction.isOk;
        private readonly TConcreteProductionParams productionParams;
        private readonly TConcreteBuildingParams buildingParams;
        private readonly TPersistentState persistentState;
        private Result<TProductionCycleState, TextErrors> stateOrReasonForNotStartingProduction;
        private readonly Event<IDeletedListener> deleted;
        private readonly EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> resSources, resDestins;
        private readonly ResPile inputStorage, outputStorage;
        private AllResAmounts resTravellingHere;
        private readonly TextBox industryUI;
        
        public Industry(TConcreteProductionParams productionParams, TConcreteBuildingParams buildingParams, TPersistentState persistentState)
        {
            this.productionParams = productionParams;
            this.buildingParams = buildingParams;
            this.persistentState = persistentState;
            stateOrReasonForNotStartingProduction = new(errors: new("Not yet initialized"));
            deleted = new();
            inputStorage = ResPile.CreateEmpty(thermalBody: buildingParams.NodeState.ThermalBody);
            outputStorage = ResPile.CreateEmpty(thermalBody: buildingParams.NodeState.ThermalBody);
            resTravellingHere = AllResAmounts.empty;

            CurWorldManager.EnergyDistributor.AddEnergyConsumer(energyConsumer: this);

            resSources = IIndustry.CreateRoutesLists(resources: buildingParams.MaxStoredInput(productionParams: productionParams).resList);
            resDestins = IIndustry.CreateRoutesLists(resources: buildingParams.GetProducedResources(productionParams: productionParams));
            RoutePanel = IIndustry.CreateRoutePanel
            (
                industry: this,
                resSources: resSources,
                resDestins: resDestins
            );
            industryUI = new();
        }

        public bool IsSourceOf(IResource resource)
            => resDestins.ContainsKey(resource);

        public bool IsDestinOf(IResource resource)
            => resSources.ContainsKey(resource);

        public IEnumerable<IResource> GetConsumedRes()
            => resSources.Keys;

        public IEnumerable<IResource> GetProducedRes()
            => resDestins.Keys;

        public EfficientReadOnlyHashSet<IIndustry> GetSources(IResource resource)
            => new(set: resSources[resource]);

        public EfficientReadOnlyHashSet<IIndustry> GetDestins(IResource resource)
            => new(set: resDestins[resource]);

        public AllResAmounts GetSupply()
            => outputStorage.Amount;

        public AllResAmounts GetDemand()
            => (TProductionCycleState.IsRepeatable || !Busy) switch
            {
                true => buildingParams.MaxStoredInput(productionParams: productionParams) - inputStorage.Amount - resTravellingHere,
                false => AllResAmounts.empty
            };

        public void TransportResTo(IIndustry destinIndustry, ResAmount<IResource> resAmount)
            => buildingParams.NodeState.TransportRes
            (
                source: outputStorage,
                destination: destinIndustry.NodeID,
                amount: new(resAmount: resAmount)
            );

        public void WaitForResFrom(IIndustry sourceIndustry, ResAmount<IResource> resAmount)
            => resTravellingHere += new AllResAmounts(resAmount: resAmount);

        public void Arrive(ResPile arrivingResPile)
        {
            resTravellingHere -= arrivingResPile.Amount;
            inputStorage.TransferAllFrom(source: arrivingResPile);
        }

        public void ToggleSource(IResource resource, IIndustry sourceIndustry)
            => IIndustry.ToggleElement(set: resSources[resource], element: sourceIndustry);

        public void ToggleDestin(IResource resource, IIndustry destinIndustry)
            => IIndustry.ToggleElement(set: resDestins[resource], element: destinIndustry);

        public void FrameStart()
        {
            stateOrReasonForNotStartingProduction = stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.ShouldRestart ? CreateProductionCycleState() : new(ok: state),
                error: _ => CreateProductionCycleState()
            );
            stateOrReasonForNotStartingProduction.PerformAction
            (
                action: state => state.FrameStart()
            );

            Result<TProductionCycleState, TextErrors> CreateProductionCycleState()
                => TProductionCycleState.Create
                (
                    productionParams: productionParams,
                    buildingParams: buildingParams,
                    persistentState: persistentState,
                    inputStorage: inputStorage,
                    maxOutputArea: buildingParams.MaxStoredOutputArea() - outputStorage.Amount.UsefulArea()
                );
        }

        public IIndustry? Update()
        {
#warning Complete this
            industryUI.Text = $"""
                Industry UI Panel
                stored inputs {inputStorage.Amount}
                stored outputs {outputStorage.Amount}
                demand {GetDemand()}
                """;
            var childIndustry = stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.Update(outputStorage: outputStorage),
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
            // Need to wait for all resources travelling here to arrive
            throw new NotImplementedException();
#warning Implement a proper industry deletion strategy
            // For now, all building materials, unused input, production, and output materials are dumped inside the planet 
            outputStorage.TransferAllFrom(source: inputStorage);
            TProductionCycleState.DeletePersistentState(persistentState: persistentState, outputStorage: outputStorage);
            stateOrReasonForNotStartingProduction.PerformAction(action: state => state.Delete(outputStorage: outputStorage));
            IIndustry.DumpAllResIntoCosmicBody(nodeState: buildingParams.NodeState, resPile: outputStorage);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        public string GetInfo()
            => throw new NotImplementedException();

        EnergyPriority IEnergyConsumer.EnergyPriority
            => buildingParams.EnergyPriority;

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
