using Game1.Collections;
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
                this.NodeState = nodeState;
                this.buildingImage = buildingImage;
                this.SurfaceMaterial = surfaceMaterial;
                BuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: buildingArea);

                buildingArea = buildingImage.Area;
                this.generalParams = generalParams;
                this.buildingMatChoices = buildingMatChoices;
            }

            public ulong MaxStoredAmount(IResource storedRes)
                => ResAndIndustryAlgos.MaxAmountInStorage
                (
                    areaInStorage: buildingArea * CurWorldConfig.storageProporOfBuildingArea,
                    // Could use Area here instead, if decide to have such a thing
                    itemArea: storedRes.UsefulArea
                );

            public IIndustry CreateFullySpecifiedIndustry(ResPile buildingResPile, IResource storedRes)
                => new Storage
                (
                    storageParams: new(storedRes: storedRes),
                    buildingParams: this,
                    buildingResPile: buildingResPile
                );

            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

            IIndustry IConcreteBuildingConstructionParams.CreateIndustry(ResPile buildingResPile)
                => new Storage(storageParams: new(), buildingParams: this, buildingResPile: buildingResPile);
        }

        [Serializable]
        public sealed class StorageParams
        {
            public EfficientReadOnlyCollection<IResource> ProducedResources { get; private set; }

            /// <summary>
            /// Eiher material, or error saying no material was chosen
            /// </summary>
            public Result<IResource, TextErrors> CurStoredRes
            {
                get => curStoredRes;
                private set
                {
                    curStoredRes = value;
                    ProducedResources = value.SwitchExpression
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
        private readonly ResPile buildingResPile;
        private readonly Event<IDeletedListener> deleted;
        private readonly EfficientReadOnlyDictionary<IResource, List<ResRoute>> resSources, resDestins;

        private Storage(StorageParams storageParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile)
        {
            this.storageParams = storageParams;
            this.buildingParams = buildingParams;
            this.buildingResPile = buildingResPile;
            deleted = new();
            resSources = IIndustry.CreateRoutesLists(resources: TargetStoredResAmounts().resList);
            resDestins = IIndustry.CreateRoutesLists(resources: storageParams.ProducedResources);
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

        public AllResAmounts TargetStoredResAmounts()
            => storageParams.CurStoredRes.SwitchExpression
            (
                ok: storedRes => new AllResAmounts(res: storedRes, amount: buildingParams.MaxStoredAmount(storedRes: storedRes)),
                error: _ => AllResAmounts.empty
            );

        public void FrameStartNoProduction(string error)
        { }

        public void FrameStart()
        { }

        public IIndustry? Update()
            => this;

        public void Delete()
        {
            buildingParams.NodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        public string GetInfo()
        {
            throw new NotImplementedException();
        }
    }
}
