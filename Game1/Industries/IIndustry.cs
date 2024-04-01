using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.GameConfig;
using static Game1.UI.ActiveUIManager;
using Game1.ContentNames;

namespace Game1.Industries
{
    public interface IIndustry : IWithSpecialPositions, IDeletable
    {
        public static readonly IImage electricityIcon = new Image(TextureName.electricity, height: CurGameConfig.smallIconHeight);
        public static readonly IImage cosmicBodyIcon = new Image(TextureName.cosmicBody, height: CurGameConfig.smallIconHeight);
        public static readonly IImage starlightIcon = new Image(TextureName.starlight, height: CurGameConfig.smallIconHeight);

        /// <summary>
        /// This is not just TextBox (or a name string) to allow industries to include resource icons in their name
        /// E.g. Basic (gear icon) manufacturing
        /// </summary>
        public IFunction<IHUDElement> NameVisual { get; }
        public NodeID NodeID { get; }
        public IBuildingImage BuildingImage { get; }

        /// <summary>
        /// Null if no building
        /// </summary>
        public MaterialPalette? SurfaceMatPalette { get; }

        public IHUDElement UIElement { get; }

        public IHUDElement RoutePanel { get; }

        public IHUDElement? IndustryFunctionVisual { get; }

        public void FrameStart();

        public sealed IIndustry? Update()
        {
            var newIndustry = UpdateImpl();
            if (newIndustry == this)
                return newIndustry;

            if (newIndustry is not null)
            {
                // This makes sure that newIndustry inherits source and destination industries whenever possible.
                // The calculation needs to be done before Delete() so that GetResWithPotentialNeighborhood, GetResNeighbors, etc. return truthful results rather than empty collections.
                foreach (var neighborDir in Enum.GetValues<NeighborDir>())
                    foreach (var res in newIndustry.GetResWithPotentialNeighborhood(neighborDir: neighborDir))
                        if (IsNeighborhoodPossible(neighborDir: neighborDir, resource: res))
                            foreach (var neighbor in GetResNeighbors(neighborDir: neighborDir, resource: res))
                                ToggleResEdge(neighborDir: neighborDir, resource: res, industry: newIndustry, potentialNeighbor: neighbor);
            }

            Delete();
            return newIndustry;
        }

        public IIndustry? UpdateImpl();

        public void UpdateUI();

        /// <summary>
        /// Returns false if was deleted already
        /// </summary>
        protected bool Delete();

        MyVector2 IWithSpecialPositions.GetSpecPos(PosEnums origin)
            => BuildingImage.GetPosition(origin: origin);

        public bool IsNeighborhoodPossible(NeighborDir neighborDir, IResource resource);

        public IReadOnlyCollection<IResource> GetResWithPotentialNeighborhood(NeighborDir neighborDir);

        public EfficientReadOnlyHashSet<IIndustry> GetResNeighbors(NeighborDir neighborDir, IResource resource);

        /// <summary>
        /// <see cref="NeighborDir.In"/> means "get demand",
        /// <see cref="NeighborDir.Out"/> means "get supply",
        /// </summary>
        public AllResAmounts GetResAmountsRequestToNeighbors(NeighborDir neighborDir);

        //public AllResAmounts GetSupply();

        //public AllResAmounts GetDemand();

        public void TransportResTo(IIndustry destinIndustry, ResAmount<IResource> resAmount);

        public void WaitForResFrom(IIndustry sourceIndustry, ResAmount<IResource> resAmount);
        
        public void Arrive(ResPile arrivingResPile);

        /// <summary>
        /// If <paramref name="neighbor"/> is already a neighbor of this of direction <paramref name="neighborDir"/>, delete it. If not, add it.
        /// DON'T CALL this directly - use ToggleResEdge instead
        /// </summary>
        protected void ToggleResNeighbor(NeighborDir neighborDir, IResource resource, IIndustry neighbor);

        private static void ToggleResEdge(NeighborDir neighborDir, IResource resource, IIndustry industry, IIndustry potentialNeighbor)
        {
            industry.ToggleResNeighbor(neighborDir: neighborDir, resource: resource, neighbor: potentialNeighbor);
            potentialNeighbor.ToggleResNeighbor(neighborDir: neighborDir.Opposite(), resource: resource, neighbor: industry);
            Debug.Assert(industry.GetResNeighbors(neighborDir: neighborDir, resource: resource).Contains(potentialNeighbor)
                == potentialNeighbor.GetResNeighbors(neighborDir: neighborDir.Opposite(), resource: resource).Contains(industry));
        }

        protected static void ToggleElement<T>(HashSet<T> set, T element)
        {
            if (!set.Remove(element))
                set.Add(element);
        }

        protected static void DumpAllResIntoCosmicBody(IIndustryFacingNodeState nodeState, ResPile resPile)
        {
            resPile.TransformFrom
            (
                source: resPile,
                recipe: resPile.Amount.TurningIntoRawMatsRecipe()
            );
            nodeState.EnlargeFrom(source: resPile, amount: resPile.Amount.RawMatComposition());
        }

        protected static EnumDict<NeighborDir, EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>>> CreateResNeighboursCollection(Func<NeighborDir, SortedResSet<IResource>> resources)
            => new(neighbourDir => resources(neighbourDir).ToEfficientReadOnlyDict(elementSelector: _ => new HashSet<IIndustry>()));

        // It's static so that it's actually protected. Otherwise couldn't call it
        protected static void DeleteResNeighbors(IIndustry industry)
        {
            foreach (var neighborDir in Enum.GetValues<NeighborDir>())
                foreach (var res in industry.GetResWithPotentialNeighborhood(neighborDir: neighborDir))
                    foreach (var neighbor in industry.GetResNeighbors(neighborDir: neighborDir, resource: res))
                        ToggleResEdge(neighborDir: neighborDir, resource: res, industry: industry, potentialNeighbor: neighbor);
        }

        protected static IHUDElement CreateRoutePanel(IIndustry industry)
        {
            const HorizPosEnum childHorizPos = HorizPosEnum.Left;
            return new UIRectVertPanel<IHUDElement>
            (
                childHorizPos: childHorizPos,
                children: Enum.GetValues<NeighborDir>().Select
                (
                    neighborDir => new UIRectVertPanel<IHUDElement>
                    (
                        childHorizPos: childHorizPos,
                        children: new List<IHUDElement>() { new TextBox(text: UIAlgorithms.ChangeResNeighbors(neighborDir: neighborDir)) }.Concat
                        (
                            industry.GetResWithPotentialNeighborhood(neighborDir: neighborDir) switch
                            {
                                { Count: 0 } => new List<IHUDElement>() { new TextBox(text: UIAlgorithms.NoPossibleNeighbors(neighborDir: neighborDir)) },
                                var resources => resources.Select
                                (
                                    res =>
                                    {
                                        Button<ImageHUDElement> toggleResNeighborButton = new
                                        (
                                            shape: new MyRectangle(width: CurGameConfig.iconWidth, height: CurGameConfig.iconHeight),
                                            visual: new ImageHUDElement(image: res.Icon),
                                            tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ToggleResNeighborTooltip(neighborDir: neighborDir, res: res))
                                        );
                                        toggleResNeighborButton.clicked.Add
                                        (
                                            listener: new ChangeResNeighborsButtonListener
                                            (
                                                industry: industry,
                                                neighborDir: neighborDir,
                                                resource: res
                                            )
                                        );
                                        return toggleResNeighborButton;
                                    }
                                )
                            }
                        )
                    )
                )
            );
        }

        [Serializable]
        private sealed class ChangeResNeighborsButtonListener(IIndustry industry, NeighborDir neighborDir, IResource resource) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                // Needed so that can pass toggleNeighborPanelManagers when creating toggleNeighborButton clicked response
                List<ToggleNeighborPanelManager> toggleNeighborPanelManagers = [];
                toggleNeighborPanelManagers.AddRange
                (
                    CurWorldManager.IndustriesWithPossibleNeighbourhood(neighborDir: neighborDir.Opposite(), resource: resource)
                        .Where(potentialNeighbor => potentialNeighbor != industry)
                        .Select
                    (
                        potentialNeighbor =>
                        {
                            bool add = !industry.GetResNeighbors(neighborDir: neighborDir, resource: resource).Contains(potentialNeighbor);
                            ToggleButton<TextBox> toggleNeighborButton = new
                            (
                                shape: new MyRectangle(width: CurGameConfig.standardUIElementWidth, height: 2 * CurGameConfig.UILineHeight),
                                visual: new
                                (
                                    text: UIAlgorithms.ToggleResNeighborButtonName(neighborDir: neighborDir, add: add),
                                    textColor: colorConfig.buttonTextColor
                                ),
                                tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ToggleResNeighborTooltip(neighborDir: neighborDir, res: resource, add: add)),
                                on: !add
                            );

                            toggleNeighborButton.onChanged.Add
                            (
                                listener: new ToggleRouteListener
                                (
                                    toggleNeighborButton: toggleNeighborButton,
                                    neighborDir: neighborDir,
                                    resource: resource,
                                    industry: industry,
                                    potentialNeighbor: potentialNeighbor
                                )
                            );

                            return new ToggleNeighborPanelManager
                            (
                                ToggleNeighborPanel: toggleNeighborButton,
                                PanelHUDPosUpdater: new HUDElementPosUpdater
                                (
                                    HUDElement: toggleNeighborButton,
                                    baseWorldObject: potentialNeighbor,
                                    HUDElementOrigin: new(HorizPosEnum.Middle, VertPosEnum.Top),
                                    anchorInBaseWorldObject: new(HorizPosEnum.Middle, VertPosEnum.Middle)
                                )
                            );
                        }
                    )
                );
                CurWorldManager.SetOneUseClickedNowhereResponse(new QuitChangeResNeighborsState(toggleNeighborPanelManagers: toggleNeighborPanelManagers));
#warning Pause the game here.
                // PROBABLY want to pause the game here so that sources don't appear and disappear before the player's eyes

                Debug.Assert(CurWorldManager.IsCosmicBodyActive(nodeID: industry.NodeID));
                CurWorldManager.SetIsCosmicBodyActive(nodeID: industry.NodeID, active: false);
                CurWorldManager.DisableAllUIElements();

                foreach (var (toggleSourcePanel, panelHUDPosUpdater) in toggleNeighborPanelManagers)
                    CurWorldManager.AddWorldHUDElement(worldHUDElement: toggleSourcePanel, updateHUDPos: panelHUDPosUpdater);

#warning Add arrow visual for this
            }
        }

        [Serializable]
        private sealed class ToggleRouteListener(ToggleButton<TextBox> toggleNeighborButton, NeighborDir neighborDir, IResource resource, IIndustry industry, IIndustry potentialNeighbor) : IOnChangedListener
        {
            void IOnChangedListener.OnChangedResponse()
            {
                ToggleResEdge(neighborDir: neighborDir, resource: resource, industry: industry, potentialNeighbor: potentialNeighbor);
                bool add = !industry.GetResNeighbors(neighborDir: neighborDir, resource: resource).Contains(potentialNeighbor);
                toggleNeighborButton.Visual.Text = UIAlgorithms.ToggleResNeighborButtonName(neighborDir: neighborDir, add: add);
            }
        }

        [Serializable]
        private readonly record struct ToggleNeighborPanelManager(IHUDElement ToggleNeighborPanel, IAction PanelHUDPosUpdater);

        [Serializable]
        private sealed class QuitChangeResNeighborsState(List<ToggleNeighborPanelManager> toggleNeighborPanelManagers) : IAction
        {
            void IAction.Invoke()
            {
                foreach (var (toggleNeighborPanel, _) in toggleNeighborPanelManagers)
                    CurWorldManager.RemoveWorldHUDElement(worldHUDElement: toggleNeighborPanel);
                CurWorldManager.EnableAllUIElements();
            }
        }
    }
}
