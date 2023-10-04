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
#pragma warning disable IDE0001 // Otherwise it says to use StorageProductionChoice instead of IResource everywhere
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public string Name { get; }
            public BuildingCostPropors BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                Name = name;
                BuildingCostPropors = new BuildingCostPropors(ingredProdToAmounts: buildingComponentPropors);

                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: CurWorldConfig.diskBuildingHeight, color: ActiveUIManager.colorConfig.manufacturingBuildingColor);
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public IHUDElement? CreateProductionChoicePanel(IItemChoiceSetter<ProductionChoice> productionChoiceSetter)
                => IndustryUIAlgos.CreateRresourceChoiceDropdown(resChoiceSetter: productionChoiceSetter.Convert<StorageChoice>());

            public ConcreteBuildingParams CreateConcrete(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, StorageChoice storageChoice)
            {
                if (!BuildingCostPropors.neededProductClasses.SetEquals(neededBuildingMatPaletteChoices.choices.Keys))
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
                    surfaceMatPalette: neededBuildingMatPaletteChoices[IProductClass.roof]
                );
            }

            IConcreteBuildingConstructionParams IGeneralBuildingConstructionParams.CreateConcreteImpl(IIndustryFacingNodeState nodeState, MaterialPaletteChoices neededBuildingMatPaletteChoices, ProductionChoice productionChoice)
                => CreateConcrete(nodeState: nodeState, neededBuildingMatPaletteChoices: neededBuildingMatPaletteChoices, storageChoice: (StorageChoice)productionChoice.Choice);
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : IConcreteBuildingConstructionParams
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public MaterialPalette SurfaceMatPalette { get; }
            public AllResAmounts BuildingCost { get; }
            public readonly DiskBuildingImage buildingImage;

            private readonly AreaDouble buildingArea;
            // generalParams and buildingMatPaletteChoices will be used if/when storage industry depends on material choices.
            // Probably the only possible dependance is how much weight it can hold.
            private readonly GeneralBuildingParams generalParams;
            private readonly MaterialPaletteChoices buildingMatPaletteChoices;
            private readonly StorageChoice storageChoice;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA,
                MaterialPaletteChoices buildingMatPaletteChoices, StorageChoice storageChoice, MaterialPalette surfaceMatPalette)
            {
                Name = generalParams.Name;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                SurfaceMatPalette = surfaceMatPalette;
                // Building area is used in BuildingCost calculation, thus needs to be computed first
                buildingArea = buildingImage.Area;
                this.generalParams = generalParams;
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

            string IConcreteBuildingConstructionParams.GetBuildingStats()
                => "Storage stats TODO";

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

            public StorageParams()
                => CurStoredRes = new(errors: new(UIAlgorithms.NoResourceIsChosen));

            public StorageParams(StorageChoice storageChoice)
                => CurStoredRes = new(ok: storageChoice);
        }

        public static HashSet<Type> GetKnownTypes()
            => new()
            {
                typeof(Storage)
            };

        public string Name
            => buildingParams.Name;

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

        private readonly StorageParams storageParams;
        private readonly ConcreteBuildingParams buildingParams;
        private readonly ResPile buildingResPile, storage;
        private readonly Event<IDeletedListener> deleted;
        private bool isDeleted;
        private readonly EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> resSources, resDestins;
        private AllResAmounts resTravellingHere;
        private readonly TextBox storageUI;

        private Storage(StorageParams storageParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile)
        {
            this.storageParams = storageParams;
            this.buildingParams = buildingParams;
            this.buildingResPile = buildingResPile;
            deleted = new();
            isDeleted = false;
            storage = ResPile.CreateEmpty(thermalBody: buildingParams.NodeState.ThermalBody);
            resTravellingHere = AllResAmounts.empty;

            resSources = IIndustry.CreateRoutesLists(resources: storageParams.StoredResources);
            resDestins = IIndustry.CreateRoutesLists(resources: storageParams.StoredResources);
            RoutePanel = IIndustry.CreateRoutePanel
            (
                industry: this,
                resSources: resSources,
                resDestins: resDestins
            );

            storageUI = new();
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
            => storage.Amount;

        public AllResAmounts GetDemand()
            => storageParams.CurStoredRes.SwitchExpression
            (
                ok: storedRes => buildingParams.MaxStored(storedRes: storedRes) - storage.Amount - resTravellingHere,
                error: _ => AllResAmounts.empty
            );

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

        public void ToggleSource(IResource resource, IIndustry sourceIndustry)
            => IIndustry.ToggleElement(set: resSources[resource], element: sourceIndustry);

        public void ToggleDestin(IResource resource, IIndustry destinIndustry)
            => IIndustry.ToggleElement(set: resDestins[resource], element: destinIndustry);

        public void FrameStart()
        { }

        public IIndustry? UpdateImpl()
        {
#warning Complete this
            storageUI.Text = $"""
                Storage UI Panel
                stored {storage.Amount}
                """;
            return this;
        }

        public bool Delete()
        {
            if (isDeleted)
                return false;
            // Need to wait for all resources travelling here to arrive
            throw new NotImplementedException();
            IIndustry.DeleteSourcesAndDestins(industry: this);
#warning Implement a proper industry deletion strategy
            storage.TransferAllFrom(source: buildingResPile);
            IIndustry.DumpAllResIntoCosmicBody(nodeState: buildingParams.NodeState, resPile: storage);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
            isDeleted = true;
            return true;
        }
#pragma warning restore IDE0001
    }
}
