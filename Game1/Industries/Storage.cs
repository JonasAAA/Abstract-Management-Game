using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    // So that if change StorageProductionChoice, will get compilation errors about giving player something to choose in UI and using something different in code
    using StorageChoice = IResource;
    [Serializable]
    public sealed class Storage : IIndustry
    {
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                NameVisual = UIAlgorithms.GetBuildingNameVisual(name: name);
                BuildingCostPropors = new(ingredProdToAmounts: buildingComponentPropors);

                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.storageBuildingColor);
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public IHUDElement? CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                => ResAndIndustryUIAlgos.CreateResourceChoiceDropdown(resChoiceSetter: productionChoiceSetter.Convert<StorageChoice>());

            public ConcreteBuildingParams CreateConcrete(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, StorageChoice storageChoice)
            {
                if (!BuildingCostPropors.neededProductClasses.SetEquals(neededBuildingMatPaletteChoices.Choices.Keys))
                    throw new ArgumentException();

                return new
                (
                    nodeState: nodeState,
                    generalParams: this,
                    buildingImage: buildingImageParams.CreateImage(nodeState),
                    buildingComponentsToAmountPUBA: ResAndIndustryHelpers.BuildingComponentsToAmountPUBA
                    (
                        buildingComponentPropors: buildingComponentPropors,
                        buildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                        buildingComponentsProporOfBuildingArea: CurWorldConfig.buildingComponentsProporOfBuildingArea
                    ),
                    buildingMatPaletteChoices: neededBuildingMatPaletteChoices,
                    storageChoice: storageChoice,
                    surfaceMatPalette: neededBuildingMatPaletteChoices[ProductClass.roof]
                );
            }

            IConcreteBuildingConstructionParams IGeneralBuildingConstructionParams.CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
                => CreateConcrete(nodeState: nodeState, neededBuildingMatPaletteChoices: neededBuildingMatPaletteChoices, storageChoice: (StorageChoice)productionChoice.Choice);

            IndustryFunctionVisualParams IGeneralBuildingConstructionParams.IncompleteFunctionVisualParams(ProductionChoice? productionChoice)
                => IncompleteFunctionVisualParams(storageParams: new((StorageChoice?)productionChoice?.Choice));
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : IConcreteBuildingConstructionParams
        {
            public IFunction<IHUDElement> NameVisual { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public MaterialPalette SurfaceMatPalette { get; }
            public AllResAmounts BuildingCost { get; }
            public readonly DiskBuildingImage buildingImage;
            // Probably the only possible dependance on material choices is how much weight it can hold.
            public readonly BuildingCostPropors buildingCostPropors;
            public readonly MaterialPaletteChoices buildingMatPaletteChoices;

            private readonly AreaDouble buildingArea;
            private readonly StorageChoice storageChoice;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA,
                MaterialPaletteChoices buildingMatPaletteChoices, StorageChoice storageChoice, MaterialPalette surfaceMatPalette)
            {
                NameVisual = generalParams.NameVisual;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                SurfaceMatPalette = surfaceMatPalette;
                // Building area is used in BuildingCost calculation, thus needs to be computed first
                buildingArea = buildingImage.Area;
                buildingCostPropors = generalParams.BuildingCostPropors;
                this.buildingMatPaletteChoices = buildingMatPaletteChoices;
                this.storageChoice = storageChoice;
                BuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: buildingArea);
            }

            public AllResAmounts MaxStored(IResource storedRes)
                => new
                (
                    res: storedRes,
                    amount: ResAndIndustryAlgos.MaxAmount
                    (
                        availableArea: buildingArea * CurWorldConfig.storageProporOfBuildingAreaForStorageIndustry,
                        // Could use Area here instead, if decide to have such a thing
                        itemArea: storedRes.Area
                    )
                );

            public IIndustry CreateFilledStorage(ResPile buildingResPile, ResPile storedResSource)
            {
                var storageIndustry = new Storage
                (
                    storageParams: new(storageChoice: storageChoice),
                    buildingParams: this,
                    buildingResPile: buildingResPile
                );
                storageIndustry.storage.TransferFrom(source: storedResSource, amount: MaxStored(storedRes: storageChoice));
                return storageIndustry;
            }

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new Storage
                (
                    storageParams: new(storageChoice: storageChoice),
                    buildingParams: this,
                    buildingResPile: buildingResPile
                );
        }

        [Serializable]
        public sealed class StorageParams
        {
            public SortedResSet<IResource> StoredResources { get; private set; }

            /// <summary>
            /// Eiher material, or error saying no material was chosen
            /// </summary>
            public Result<IResource, TextErrors> CurStoredRes
            {
                get => curStoredRes;
                private set
                {
                    curStoredRes = value;
                    StoredResources = value.SwitchExpression
                    (
                        ok: material => new SortedResSet<IResource>(res: material),
                        error: errors => SortedResSet<IResource>.empty
                    );
                }
            }

            /// <summary>
            /// NEVER use this directly. Always use CurMaterial instead
            /// </summary>
            private Result<IResource, TextErrors> curStoredRes;

            public StorageParams(StorageChoice? storageChoice)
                => CurStoredRes = storageChoice switch
                {
                    not null => new(ok: storageChoice),
                    null => new(errors: new(UIAlgorithms.NoResourceIsChosen))
                };
        }

        private static IndustryFunctionVisualParams IncompleteFunctionVisualParams(StorageParams storageParams)
            => storageParams.CurStoredRes.SwitchExpression<IndustryFunctionVisualParams>
            (
                ok: res => new
                (
                    InputIcons: [res.SmallIcon],
                    OutputIcons: [res.SmallIcon]
                ),
                error: _ => new
                (
                    InputIcons: [IIndustry.resIcon],
                    OutputIcons: [IIndustry.resIcon]
                )
            );

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Storage)
            };

        public IFunction<IHUDElement> NameVisual
            => buildingParams.NameVisual;

        public NodeID NodeID
            => buildingParams.NodeState.NodeID;

        public MaterialPalette? SurfaceMatPalette
            => buildingParams.SurfaceMatPalette;

        public IHUDElement UIElement
            => storageUI;

        public IEvent<IDeletedListener> Deleted
            => deleted;

        public IBuildingImage BuildingImage
            => buildingParams.buildingImage;

        // CURRENTLY this doesn't handle changes in res consumed and res produced. So if choose to store a different thing later on (e.g. iron instead of nothing),
        // this will not be updated accordingly
        public IHUDElement RoutePanel { get; }

        // CURRENTLY this doesn't handle changes in res consumed and res produced. So if change produced material recipe, or choose to recycle different thing,
        // this will not be updated accordingly
        public IHUDElement IndustryFunctionVisual { get; }

        private readonly StorageParams storageParams;
        private readonly ConcreteBuildingParams buildingParams;
        private readonly ResPile buildingResPile, storage;
        private readonly Event<IDeletedListener> deleted;
        private bool isDeleted;
        private readonly EnumDict<NeighborDir, EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>>> resNeighbors;
        private AllResAmounts resTravellingHere;
        private readonly UIRectVertPanel<IHUDElement> storageUI;
        private IHUDElement storedAmountsUI, unusedAmountsUI;

        private Storage(StorageParams storageParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile)
        {
            this.storageParams = storageParams;
            this.buildingParams = buildingParams;
            this.buildingResPile = buildingResPile;
            deleted = new();
            isDeleted = false;
            storage = ResPile.CreateEmpty(thermalBody: buildingParams.NodeState.ThermalBody);
            resTravellingHere = AllResAmounts.empty;

            resNeighbors = IIndustry.CreateResNeighboursCollection(resources: _ => storageParams.StoredResources);
            RoutePanel = IIndustry.CreateRoutePanel(industry: this);
            IndustryFunctionVisual = IncompleteFunctionVisualParams(storageParams: storageParams).CreateIndustryFunctionVisual();

            storedAmountsUI = ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: storage.Amount);
            unusedAmountsUI = CreateNewUnusedAmountsUI();

            storageUI = new
            (
                childHorizPos: HorizPosEnum.Left,
                children:
                [
                    buildingParams.NameVisual.Invoke(),
                    ResAndIndustryUIAlgos.CreateBuildingStatsHeaderRow(),
                    ResAndIndustryUIAlgos.CreateNeededElectricityAndThroughputPanel
                    (
                        nodeState: buildingParams.NodeState,
                        neededElectricityGraph: ResAndIndustryUIAlgos.CreateGravityFunctionGraph
                        (
                            func: gravity => ResAndIndustryAlgos.TentativeNeededElectricity
                            (
                                gravity: gravity,
                                chosenTotalPropor: Propor.full,
                                matPaletteChoices: buildingParams.buildingMatPaletteChoices.Choices,
                                buildingProdClassPropors: buildingParams.buildingCostPropors.neededProductClassPropors
                            )
                        ),
                        throughputGraph: ResAndIndustryUIAlgos.CreateTemperatureFunctionGraph
                        (
                            func: temperature => ResAndIndustryAlgos.TentativeThroughput
                            (
                                temperature: temperature,
                                chosenTotalPropor: Propor.full,
                                matPaletteChoices: buildingParams.buildingMatPaletteChoices.Choices,
                                buildingProdClassPropors: buildingParams.buildingCostPropors.neededProductClassPropors
                            )
                        )
                    ),
                    new TextBox(text: "THIS BUILDING DOES NOT\nDEPEND ON THE ABOVE CURRENTLY"),
                    new TextBox(text: "stored"),
                    storedAmountsUI,
                    new TextBox(text: "unused space"),
                    unusedAmountsUI
                ]
            );
        }

        private IHUDElement CreateNewUnusedAmountsUI()
            => ResAndIndustryUIAlgos.ResAmountsHUDElement
            (
                resAmounts: buildingParams.MaxStored(storageParams.CurStoredRes.UnwrapOrThrow()) - storage.Amount
            );

        public bool IsNeighborhoodPossible(NeighborDir neighborDir, IResource resource)
            => resNeighbors[neighborDir].ContainsKey(resource);

        public IReadOnlyCollection<IResource> GetResWithPotentialNeighborhood(NeighborDir neighborDir)
            => resNeighbors[neighborDir].Keys;

        public EfficientReadOnlyHashSet<IIndustry> GetResNeighbors(NeighborDir neighborDir, IResource resource)
            => new(set: resNeighbors[neighborDir][resource]);

        public AllResAmounts GetResAmountsRequestToNeighbors(NeighborDir neighborDir)
            => neighborDir switch
            {
                NeighborDir.In => storageParams.CurStoredRes.SwitchExpression
                (
                    ok: storedRes => buildingParams.MaxStored(storedRes: storedRes) - storage.Amount - resTravellingHere,
                    error: _ => AllResAmounts.empty
                ),
                NeighborDir.Out => storage.Amount,
            };

        public void TransportResTo(IIndustry destinIndustry, ResAmount<IResource> resAmount)
            => buildingParams.NodeState.TransportRes
            (
                source: storage,
                destination: destinIndustry.NodeID,
                amount: new(resAmount: resAmount)
            );

        public void WaitForResFrom(IIndustry sourceIndustry, ResAmount<IResource> resAmount)
            => resTravellingHere += new AllResAmounts(resAmount: resAmount);

        public void Arrive(ResPile arrivingResPile)
        {
            resTravellingHere -= arrivingResPile.Amount;
            storage.TransferAllFrom(source: arrivingResPile);
        }

        public void ToggleResNeighbor(NeighborDir neighborDir, IResource resource, IIndustry neighbor)
            => IIndustry.ToggleElement(set: resNeighbors[neighborDir][resource], element: neighbor);

        public void FrameStart()
        { }

        public IIndustry? UpdateImpl()
            => this;

        public void UpdateUI()
        {
            storageUI.ReplaceChild
            (
                oldChild: ref storedAmountsUI,
                newChild: ResAndIndustryUIAlgos.ResAmountsHUDElement(resAmounts: storage.Amount)
            );
            storageUI.ReplaceChild
            (
                oldChild: ref unusedAmountsUI,
                newChild: CreateNewUnusedAmountsUI()
            );
        }

        public bool Delete()
        {
            if (isDeleted)
                return false;
            // Need to wait for all resources travelling here to arrive
            throw new NotImplementedException();
            IIndustry.DeleteResNeighbors(industry: this);
#warning Implement a proper industry deletion strategy
            storage.TransferAllFrom(source: buildingResPile);
            IIndustry.DumpAllResIntoCosmicBody(nodeState: buildingParams.NodeState, resPile: storage);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
            isDeleted = true;
            return true;
        }
    }
}
