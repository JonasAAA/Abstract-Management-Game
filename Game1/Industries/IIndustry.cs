using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.UI.ActiveUIManager;

namespace Game1.Industries
{
    public interface IIndustry : IWithSpecialPositions, IDeletable
    {
        public string Name { get; }
        public NodeID NodeID { get; }
        public IBuildingImage BuildingImage { get; }

        /// <summary>
        /// Null if no building
        /// </summary>
        public MaterialPalette? SurfaceMatPalette { get; }

        public IHUDElement UIElement { get; }

        public IHUDElement RoutePanel { get; }

        public void FrameStart();

        public sealed IIndustry? Update()
        {
            var newIndustry = UpdateImpl();
            if (newIndustry == this || newIndustry is null)
                return newIndustry;

            // Do this before deleting the old industry so that GetConsumedRes, GetSource, etc. return thruthful results rather than empty things
            foreach (var res in newIndustry.GetConsumedRes())
                if (IsDestinOf(resource: res))
                    foreach (var source in GetSources(resource: res))
                        ToggleSourceAndDestin(resource: res, source: source, destin: newIndustry);

            foreach (var res in newIndustry.GetProducedRes())
                if (IsSourceOf(resource: res))
                    foreach (var destin in GetDestins(resource: res))
                        ToggleSourceAndDestin(resource: res, source: newIndustry, destin: destin);

            Delete();
            return newIndustry;
        }

        public IIndustry? UpdateImpl();

        /// <summary>
        /// Returns false if was deleted already
        /// </summary>
        /// <returns></returns>
        protected bool Delete();

        MyVector2 IWithSpecialPositions.GetSpecPos(PosEnums origin)
            => BuildingImage.GetPosition(origin: origin);

        public bool IsSourceOf(IResource resource);

        public bool IsDestinOf(IResource resource);

        public IEnumerable<IResource> GetConsumedRes();

        public IEnumerable<IResource> GetProducedRes();

        public EfficientReadOnlyHashSet<IIndustry> GetSources(IResource resource);

        public EfficientReadOnlyHashSet<IIndustry> GetDestins(IResource resource);

        public AllResAmounts GetSupply();

        public AllResAmounts GetDemand();

        public void TransportResTo(IIndustry destinIndustry, ResAmount<IResource> resAmount);

        public void WaitForResFrom(IIndustry sourceIndustry, ResAmount<IResource> resAmount);
        
        public void Arrive(ResPile arrivingResPile);

        /// <summary>
        /// If <paramref name="sourceIndustry"/> is already a source of <paramref name="resource"/>, delete it. If not, add it.
        /// DON'T CALL this directly - use ToggleSourceAndDestin instead
        /// </summary>
        protected void ToggleSource(IResource resource, IIndustry sourceIndustry);

        /// <summary>
        /// If <paramref name="destinIndustry"/> is already a destination of <paramref name="resource"/>, delete it. If not, add it.
        /// DON'T CALL this directly - use ToggleSourceAndDestin instead
        /// </summary>
        protected void ToggleDestin(IResource resource, IIndustry destinIndustry);

        private static void ToggleSourceAndDestin(IResource resource, IIndustry source, IIndustry destin)
        {
            source.ToggleDestin(resource: resource, destinIndustry: destin);
            destin.ToggleSource(resource: resource, sourceIndustry: source);
            Debug.Assert(source.GetDestins(resource: resource).Contains(destin) == destin.GetSources(resource: resource).Contains(source));
        }

        protected static void ToggleElement<T>(HashSet<T> set, T element)
        {
            if (set.Contains(element))
                set.Remove(element);
            else
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

        protected static EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> CreateRoutesLists(SortedResSet<IResource> resources)
            => resources.ToEfficientReadOnlyDict(elementSelector: _ => new HashSet<IIndustry>());

        // It's static so that it's actually protected. Otherwise couldn't call it
        protected static void DeleteSourcesAndDestins(IIndustry industry)
        {
            foreach (var res in industry.GetConsumedRes())
                foreach (var source in industry.GetSources(resource: res))
                    ToggleSourceAndDestin(resource: res, source: source, destin: industry);

            foreach (var res in industry.GetProducedRes())
                foreach (var destin in industry.GetDestins(resource: res))
                    ToggleSourceAndDestin(resource: res, source: industry, destin: destin);
        }

        protected static IHUDElement CreateRoutePanel(IIndustry industry, EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> resSources,
            EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> resDestins)
            => new RoutePanelManager(industry: industry, resSources: resSources, resDestins: resDestins).routePanel;

        [Serializable]
        private readonly struct RoutePanelManager
        {
            public readonly UIRectVertPanel<IHUDElement> routePanel;
            private readonly EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> resSources, resDestins;

            public RoutePanelManager(IIndustry industry, EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> resSources,
                EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> resDestins)
            {
                this.resSources = resSources;
                this.resDestins = resDestins;

                const HorizPosEnum childHorizPos = HorizPosEnum.Left;
                
                UIRectVertPanel<IHUDElement> resSourcePanel = new
                (
                    childHorizPos: childHorizPos,
                    children: new List<IHUDElement>() { new TextBox(text: UIAlgorithms.ChangeResSources) }.Concat
                    (
                        resSources.Count switch
                        {
                            0 => new List<IHUDElement>() { new TextBox(text: UIAlgorithms.NoResourcesProduced) },
                            not 0 => resSources.Select
                            (
                                resSource =>
                                {
                                    var (res, sources) = resSource;
                                    Button addOrRemoveResSourceButton = new
                                    (
                                        shape: new MyRectangle(width: curUIConfig.wideUIElementWidth, height: curUIConfig.UILineHeight),
                                        tooltip: new ImmutableTextTooltip(text: UIAlgorithms.AddOrRemoveResSourceTooltip(res: res)),
                                        text: res.ToString()
                                    );
                                    addOrRemoveResSourceButton.clicked.Add
                                    (
                                        listener: new ChangeResSourcesButtonListener
                                        (
                                            Industry: industry,
                                            Resource: res,
                                            Sources: sources
                                        )
                                    );
                                    return addOrRemoveResSourceButton;
                                }
                            )
                        }
                    )
                );

                UIRectVertPanel<IHUDElement> resDestinPanel = new
                (
                    childHorizPos: childHorizPos,
                    children: new List<IHUDElement>() { new TextBox(text: UIAlgorithms.ProducedResourcesDestinations) }
                );

                routePanel = new(childHorizPos: childHorizPos, children: new List<IHUDElement>() { resSourcePanel, resDestinPanel });
            }
        }

        [Serializable]
        private sealed record ChangeResSourcesButtonListener(IIndustry Industry, IResource Resource, HashSet<IIndustry> Sources) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                // Needed so that can pass toggleSourcePanelManagers when creating ChooseSourceButton clicked response
                List<ToggleSourcePanelManager> toggleSourcePanelManagers = new();
                toggleSourcePanelManagers.AddRange
                (
                    CurWorldManager.SourcesOf(resource: Resource)
                        .Where(sourceIndustry => sourceIndustry != Industry)
                        .Select
                    (
                        sourceIndustry =>
                        {
                            bool add = !Industry.GetSources(resource: Resource).Contains(sourceIndustry);
                            Button toggleSourceButton = new
                            (
                                shape: new MyRectangle(width: curUIConfig.standardUIElementWidth, height: 2 * curUIConfig.UILineHeight),
                                tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ToggleSourceTooltip(res: Resource, add: add)),
                                text: UIAlgorithms.ToggleSourceButtonName(add: add)
                            );

                            toggleSourceButton.clicked.Add
                            (
                                listener: new ToggleRouteListener
                                (
                                    ToggleSourcePanelManagers: toggleSourcePanelManagers.ToEfficientReadOnlyCollection(),
                                    Resource: Resource,
                                    SourceIndustry: sourceIndustry,
                                    Industry: Industry
                                )
                            );

                            return new ToggleSourcePanelManager
                            (
                                ToggleSourcePanel: toggleSourceButton,
                                SourceIndustry: sourceIndustry,
                                PanelHUDPosUpdater: new HUDElementPosUpdater
                                (
                                    HUDElement: toggleSourceButton,
                                    baseWorldObject: sourceIndustry,
                                    HUDElementOrigin: new(HorizPosEnum.Middle, VertPosEnum.Top),
                                    anchorInBaseWorldObject: new(HorizPosEnum.Middle, VertPosEnum.Middle)
                                )
                            );
                        }
                    )
                );
#warning Pause the game here. Also, when click anywhere else, cancel this action
                // PROBABLY want to pause the game here so that sources don't appear and disappear before the player's eyes

                Debug.Assert(CurWorldManager.IsCosmicBodyActive(nodeID: Industry.NodeID));
                CurWorldManager.SetIsCosmicBodyActive(nodeID: Industry.NodeID, active: false);
                CurWorldManager.DisableAllUIElements();

                foreach (var (toggleSourcePanel, _, panelHUDPosUpdater) in toggleSourcePanelManagers)
                    CurWorldManager.AddWorldHUDElement(worldHUDElement: toggleSourcePanel, updateHUDPos: panelHUDPosUpdater);

#warning Add arrow visual for this
            }
        }

        [Serializable]
        private sealed record ToggleRouteListener(EfficientReadOnlyCollection<ToggleSourcePanelManager> ToggleSourcePanelManagers, IResource Resource, IIndustry SourceIndustry, IIndustry Industry) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                ToggleSourceAndDestin(resource: Resource, source: SourceIndustry, destin: Industry);

                foreach (var (toggleSourcePanel, _, _) in ToggleSourcePanelManagers)
                    CurWorldManager.RemoveWorldHUDElement(worldHUDElement: toggleSourcePanel);
                CurWorldManager.EnableAllUIElements();
                Debug.Assert(!CurWorldManager.IsCosmicBodyActive(nodeID: Industry.NodeID));
                //CurWorldManager.SetIsCosmicBodyActive(nodeID: Industry.NodeID, active: true);
            }
        }

        [Serializable]
        private readonly record struct ToggleSourcePanelManager(IHUDElement ToggleSourcePanel, IIndustry SourceIndustry, IAction PanelHUDPosUpdater);
    }
}
