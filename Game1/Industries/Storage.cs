using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;

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

            public Result<IConcreteBuildingConstructionParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
                => ResAndIndustryAlgos.BuildingComponentsToAmountPUBA
                (
                    buildingComponentPropors: buildingComponentPropors,
                    buildingMatChoices: neededBuildingMatChoices
                ).Select<IConcreteBuildingConstructionParams>
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
        }

        [Serializable]
        private readonly struct ConcreteBuildingParams : IConcreteBuildingConstructionParams
        {
            public string Name { get; }
            public IIndustryFacingNodeState NodeState { get; }
            public Material SurfaceMaterial { get; }
            public readonly DiskBuildingImage buildingImage;
            public readonly AllResAmounts startingBuildingCost;

            /// <summary>
            /// Things depend on this rather than on building components target area as can say that if planet underneath building shrinks,
            /// building gets not enough space to operate at maximum efficiency
            /// </summary>
            public AreaDouble CurBuildingArea
                => buildingImage.Area;
            // generalParams and buildingMatChoices will be used if/when storage industry depends on material choices.
            // Probably the only possible dependance is how much weight it can hold.
            private readonly GeneralBuildingParams generalParams;
            private readonly EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA;
            private readonly MaterialChoices buildingMatChoices;

            public ConcreteBuildingParams(IIndustryFacingNodeState nodeState, GeneralBuildingParams generalParams, DiskBuildingImage buildingImage,
                EfficientReadOnlyCollection<(Product prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA,
                MaterialChoices buildingMatChoices, Material surfaceMaterial)
            {
                Name = generalParams.Name;
                this.NodeState = nodeState;
                this.buildingImage = buildingImage;
                this.SurfaceMaterial = surfaceMaterial;

                this.generalParams = generalParams;
                this.buildingComponentsToAmountPUBA = buildingComponentsToAmountPUBA;
                this.buildingMatChoices = buildingMatChoices;
                startingBuildingCost = ResAndIndustryHelpers.CurNeededBuildingComponents(buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);
            }

            public void RemoveUnneededBuildingComponents(ResPile buildingResPile)
                => ResAndIndustryHelpers.RemoveUnneededBuildingComponents(nodeState: NodeState, buildingResPile: buildingResPile, buildingComponentsToAmountPUBA: buildingComponentsToAmountPUBA, curBuildingArea: CurBuildingArea);

            AllResAmounts IConcreteBuildingConstructionParams.BuildingCost
                => startingBuildingCost;

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

        public EfficientReadOnlyCollection<IResource> PotentiallyNotNeededBuildingComponents
            => buildingParams.startingBuildingCost.resList;

        private readonly StorageParams storageParams;
        private readonly ConcreteBuildingParams buildingParams;
        private readonly ResPile buildingResPile;
        private readonly Event<IDeletedListener> deleted;

        private Storage(StorageParams storageParams, ConcreteBuildingParams buildingParams, ResPile buildingResPile)
        {
            this.storageParams = storageParams;
            this.buildingParams = buildingParams;
            this.buildingResPile = buildingResPile;
            deleted = new();
        }

        public EfficientReadOnlyCollection<IResource> GetConsumedResources()
            => TargetStoredResAmounts().resList;

        public EfficientReadOnlyCollection<IResource> GetProducedResources()
            => storageParams.ProducedResources;

        private ulong MaxStoredAmount(IResource storedRes)
            => ResAndIndustryAlgos.MaxAmountInStorage
            (
                areaInStorage: ResAndIndustryAlgos.StorageArea(buildingArea: buildingParams.CurBuildingArea),
                // Could use Area here instead, if decide to have such a thing
                itemArea: storedRes.UsefulArea
            );

        public AllResAmounts TargetStoredResAmounts()
            => storageParams.CurStoredRes.SwitchExpression
            (
                ok: storedRes => new AllResAmounts(res: storedRes, amount: MaxStoredAmount(storedRes: storedRes)),
                error: _ => AllResAmounts.empty
            );

        public void FrameStartNoProduction(string error)
        { }

        public void FrameStart()
        { }

        public IIndustry? Update()
        {
            buildingParams.RemoveUnneededBuildingComponents(buildingResPile: buildingResPile);
            return this;
        }

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
