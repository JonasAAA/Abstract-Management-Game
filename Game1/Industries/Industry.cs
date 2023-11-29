﻿using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    public static class Industry
    {
        public interface IConcreteBuildingParams<TConcreteProductionParams>
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public EnergyPriority EnergyPriority { get; }
            public IBuildingImage IdleBuildingImage { get; }

            public MaterialPalette? SurfaceMatPalette(bool productionInProgress);
            public SortedResSet<IResource> GetProducedResources(TConcreteProductionParams productionParams);
            // Consumed resources are not computed from MaxStoredInput directly as in ladfill you may need extra building components later on,
            // but at the beginning the building doesn't have enough area to store any non-zero amount of them
            public SortedResSet<IResource> GetConsumedResources(TConcreteProductionParams productionParams);
            public AllResAmounts MaxStoredInput(TConcreteProductionParams productionParams);
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
                ResPile inputStorage, AreaInt storedOutputArea);

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
        public IFunction<IHUDElement> NameVisual
            => buildingParams.NameVisual;

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
        private bool isDeleted;
        private readonly EnumDict<NeighborDir, EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>>> resNeighbors;
        private readonly ResPile inputStorage, outputStorage;
        private AllResAmounts resTravellingHere;
        private readonly UIRectVertPanel<IHUDElement> industryUI;
        private IHUDElement storedInputsUI, storedOutputsUI, resTravellingHereUI, demandUI;
        
        public Industry(TConcreteProductionParams productionParams, TConcreteBuildingParams buildingParams, TPersistentState persistentState)
        {
            this.productionParams = productionParams;
            this.buildingParams = buildingParams;
            this.persistentState = persistentState;
            stateOrReasonForNotStartingProduction = new(errors: new("Not yet initialized"));
            deleted = new();
            isDeleted = false;
            inputStorage = ResPile.CreateEmpty(thermalBody: buildingParams.NodeState.ThermalBody);
            outputStorage = ResPile.CreateEmpty(thermalBody: buildingParams.NodeState.ThermalBody);
            resTravellingHere = AllResAmounts.empty;

            CurWorldManager.EnergyDistributor.AddEnergyConsumer(energyConsumer: this);

            resNeighbors = IIndustry.CreateResNeighboursCollection
            (
                resources: neighborDir => neighborDir switch
                {
                    NeighborDir.In => buildingParams.GetConsumedResources(productionParams: productionParams),
                    NeighborDir.Out => buildingParams.GetProducedResources(productionParams: productionParams)
                }
            );
            RoutePanel = IIndustry.CreateRoutePanel(industry: this);

            storedInputsUI = ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: inputStorage.Amount);
            storedOutputsUI = ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: outputStorage.Amount);
            resTravellingHereUI = ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: resTravellingHere);
            demandUI = ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: GetResAmountsRequestToNeighbors(NeighborDir.In));
            industryUI = new UIRectVertPanel<IHUDElement>
            (
                childHorizPos: HorizPosEnum.Left,
                children: new List<IHUDElement>()
                {
                    new TextBox(text: "Industry UI Panel"),
                    new TextBox(text: "stored inputs"),
                    storedInputsUI,
                    new TextBox(text: "stored outputs"),
                    storedOutputsUI,
                    new TextBox(text: "res travelling here"),
                    resTravellingHereUI,
                    new TextBox(text: "demand"),
                    demandUI
                }
            );
        }

        public bool IsNeighborhoodPossible(NeighborDir neighborDir, IResource resource)
            => resNeighbors[neighborDir].ContainsKey(resource);

        public IReadOnlyCollection<IResource> GetResWithPotentialNeighborhood(NeighborDir neighborDir)
            => resNeighbors[neighborDir].Keys;

        public EfficientReadOnlyHashSet<IIndustry> GetResNeighbors(NeighborDir neighborDir, IResource resource)
            => new(set: resNeighbors[neighborDir][resource]);

        public AllResAmounts GetResAmountsRequestToNeighbors(NeighborDir neighborDir)
            => neighborDir switch
            {
                NeighborDir.In => (TProductionCycleState.IsRepeatable || !Busy) switch
                {
                    true => buildingParams.MaxStoredInput(productionParams: productionParams) - inputStorage.Amount - resTravellingHere,
                    false => AllResAmounts.empty
                },
                NeighborDir.Out => outputStorage.Amount,
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

        public void ToggleResNeighbor(NeighborDir neighborDir, IResource resource, IIndustry neighbor)
            => IIndustry.ToggleElement(set: resNeighbors[neighborDir][resource], element: neighbor);

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
                    storedOutputArea: outputStorage.Amount.Area()
                );
        }

        public IIndustry? UpdateImpl()
        {
            var childIndustry = stateOrReasonForNotStartingProduction.SwitchExpression
            (
                ok: state => state.Update(outputStorage: outputStorage),
                error: _ => null
            );

            if (childIndustry is not null)
            {
                stateOrReasonForNotStartingProduction = new(errors: new("construction is done"));
                return childIndustry;
            }
            return this;
        }

        public void UpdateUI()
        {
#warning Complete this: Add proper UI
            UpdateResAmountsUI(resAmountsUI: ref storedInputsUI, resAmounts: inputStorage.Amount);
            UpdateResAmountsUI(resAmountsUI: ref storedOutputsUI, resAmounts: outputStorage.Amount);
            UpdateResAmountsUI(resAmountsUI: ref resTravellingHereUI, resAmounts: resTravellingHere);
            UpdateResAmountsUI(resAmountsUI: ref demandUI, resAmounts: GetResAmountsRequestToNeighbors(NeighborDir.In));

            return;

            void UpdateResAmountsUI(ref IHUDElement resAmountsUI, AllResAmounts resAmounts)
                => industryUI.ReplaceChild
                (
                    oldChild: ref resAmountsUI,
                    newChild: ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: resAmounts)
                );
        }

        public bool Delete()
        {
            if (isDeleted)
                return false;
            if (!resTravellingHere.IsEmpty)
                throw new NotImplementedException("Need to wait for all resources travelling here to arrive");
            IIndustry.DeleteResNeighbors(industry: this);
#warning Implement a proper industry deletion strategy
            // For now, all building materials, unused input, production, and output materials are dumped inside the planet 
            outputStorage.TransferAllFrom(source: inputStorage);
            TProductionCycleState.DeletePersistentState(persistentState: persistentState, outputStorage: outputStorage);
            stateOrReasonForNotStartingProduction.PerformAction(action: state => state.Delete(outputStorage: outputStorage));
            IIndustry.DumpAllResIntoCosmicBody(nodeState: buildingParams.NodeState, resPile: outputStorage);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));

            isDeleted = true;
            return true;
        }

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
