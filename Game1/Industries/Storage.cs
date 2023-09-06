﻿using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public sealed class Storage : IIndustry
    {
        [Serializable]
        public sealed class GeneralBuildingParams : IGeneralBuildingConstructionParams
        {
            public string Name { get; }
            public GeneralProdAndMatAmounts BuildingCostPropors { get; }

            public readonly DiskBuildingImage.Params buildingImageParams;

            private readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors;

            public GeneralBuildingParams(string name, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> buildingComponentPropors)
            {
                Name = name;
                BuildingCostPropors = new GeneralProdAndMatAmounts(ingredProdToAmounts: buildingComponentPropors, ingredMatPurposeToUsefulAreas: new());
                if (BuildingCostPropors.materialPropors[IMaterialPurpose.roofSurface].IsEmpty)
                    throw new ArgumentException();
                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, color: ActiveUIManager.colorConfig.manufacturingBuildingColor);
                this.buildingComponentPropors = buildingComponentPropors;
            }

            public Result<ConcreteBuildingParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
                => ResAndIndustryAlgos.BuildingComponentsToAmountPUBA
                (
                    buildingComponentPropors: buildingComponentPropors,
                    buildingMatChoices: neededBuildingMatChoices,
                    buildingComponentsProporOfBuildingArea: CurWorldConfig.buildingComponentsProporOfBuildingArea
                ).Select
                (
                    buildingComponentsToAmountPUBA => new ConcreteBuildingParams
                    (
                        nodeState: nodeState,
                        generalParams: this,
                        buildingImage: buildingImageParams.CreateImage(nodeState),
                        buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA,
                        buildingMatChoices: neededBuildingMatChoices,
                        surfaceMaterial: neededBuildingMatChoices[IMaterialPurpose.roofSurface]
                    )
                );

            Result<IConcreteBuildingConstructionParams, EfficientReadOnlyHashSet<IMaterialPurpose>> IGeneralBuildingConstructionParams.CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
                => CreateConcrete(nodeState: nodeState, neededBuildingMatChoices: neededBuildingMatChoices).Select<IConcreteBuildingConstructionParams>
                (
                    concreteBuildingParams => concreteBuildingParams
                );
        }

        [Serializable]
        public readonly struct ConcreteBuildingParams : IConcreteBuildingConstructionParams
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public Material SurfaceMaterial { get; }
            public AllResAmounts BuildingCost { get; }
            public readonly DiskBuildingImage buildingImage;

            private readonly AreaDouble buildingArea;
            // generalParams and buildingMatChoices will be used if/when storage industry depends on material choices.
            // Probably the only possible dependance is how much weight it can hold.
            private readonly GeneralBuildingParams generalParams;
            private readonly MaterialChoices buildingMatChoices;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA,
                MaterialChoices buildingMatChoices, Material surfaceMaterial)
            {
                Name = generalParams.Name;
                NodeState = nodeState;
                this.buildingImage = buildingImage;
                SurfaceMaterial = surfaceMaterial;
                // Building area is used in BuildingCost calculation, thus needs to be computed first
                buildingArea = buildingImage.Area;
                this.generalParams = generalParams;
                this.buildingMatChoices = buildingMatChoices;
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
                        itemArea: storedRes.UsefulArea
                    )
                );

            public IIndustry CreateFullySpecifiedFilledStorage(ResPile buildingResPile, IResource storedRes, ResPile storedResSource)
            {
                var storageIndustry = new Storage
                (
                    storageParams: new(storedRes: storedRes),
                    buildingParams: this,
                    buildingResPile: buildingResPile
                );
                storageIndustry.storage.TransferFrom(source: storedResSource, amount: MaxStored(storedRes: storedRes));
                return storageIndustry;
            }

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new Storage(storageParams: new(), buildingParams: this, buildingResPile: buildingResPile);
        }

        [Serializable]
        public sealed class StorageParams
        {
            public EfficientReadOnlyCollection<IResource> StoredResources { get; private set; }

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
                        ok: material => new List<IResource>() { material }.ToEfficientReadOnlyCollection(),
                        error: errors => EfficientReadOnlyCollection<IResource>.empty
                    );
                }
            }

            /// <summary>
            /// NEVER use this directly. Always use CurMaterial instead
            /// </summary>
            private Result<IResource, TextErrors> curStoredRes;

            public StorageParams()
                => CurStoredRes = new(errors: new(UIAlgorithms.NoResourceIsChosen));

            public StorageParams(IResource storedRes)
                => CurStoredRes = new(ok: storedRes);
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

        public Material? SurfaceMaterial
            => buildingParams.SurfaceMaterial;

        public IHUDElement UIElement
#warning Complete this
            => new TextBox() { Text = "Storage UI Panel" };

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
        private readonly EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> resSources, resDestins;
        private AllResAmounts resTravellingHere;

        private Storage(StorageParams storageParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile)
        {
            this.storageParams = storageParams;
            this.buildingParams = buildingParams;
            this.buildingResPile = buildingResPile;
            deleted = new();
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

        public IIndustry? Update()
            => this;

        public void Delete()
        {
            // Need to wait for all resources travelling here to arrive
            throw new NotImplementedException();
            storage.TransferAllFrom(source: buildingResPile);
            IIndustry.DumpAllResIntoCosmicBody(nodeState: buildingParams.NodeState, resPile: storage);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        public string GetInfo()
            => throw new NotImplementedException();
    }
}
