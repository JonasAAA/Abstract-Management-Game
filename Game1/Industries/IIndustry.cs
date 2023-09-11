using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;

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
        public Material? SurfaceMaterial { get; }

        public IHUDElement UIElement { get; }

        public IHUDElement RoutePanel { get; }

        public void FrameStart();

        public IIndustry? Update();

        public string GetInfo();

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
        /// </summary>
        public void ToggleSource(IResource resource, IIndustry sourceIndustry);

        /// <summary>
        /// If <paramref name="destinIndustry"/> is already a destination of <paramref name="resource"/>, delete it. If not, add it.
        /// </summary>
        public void ToggleDestin(IResource resource, IIndustry destinIndustry);

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

        protected static EfficientReadOnlyDictionary<IResource, HashSet<IIndustry>> CreateRoutesLists(EfficientReadOnlyCollection<IResource> resources)
            => resources.ToEfficientReadOnlyDict(elementSelector: _ => new HashSet<IIndustry>());

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
                routePanel = new(childHorizPos: childHorizPos);

                UIRectVertPanel<IHUDElement> resSourcePanel = new(childHorizPos: childHorizPos);
                routePanel.AddChild(child: resSourcePanel);
                resSourcePanel.AddChild(new TextBox() { Text = UIAlgorithms.ChangeResSources });
                if (resSources.Count is 0)
                {
                    resSourcePanel.AddChild(child: new TextBox() { Text = UIAlgorithms.NoResourcesProduced });
                }
                else
                {
                    foreach (var (res, sources) in resSources)
                    {
                        Button addOrRemoveResSourceButton = new
                        (
                            shape: new MyRectangle(width: 100, height: 30),
                            tooltip: new ImmutableTextTooltip(text: UIAlgorithms.AddOrRemoveResSourceTooltip(res: res)),
                            text: res.ToString()
                        );
                        resSourcePanel.AddChild(child: addOrRemoveResSourceButton);
                        addOrRemoveResSourceButton.clicked.Add
                        (
                            listener: new ChangeResSourcesButtonListener
                            (
                                Industry: industry,
                                Resource: res,
                                Sources: sources
                            )
                        );
                    }
                }

                UIRectVertPanel<IHUDElement> resDestinPanel = new(childHorizPos: childHorizPos);
                routePanel.AddChild(resDestinPanel);
                resDestinPanel.AddChild(new TextBox() { Text = UIAlgorithms.ProducedResourcesDestinations });
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
                                shape: new MyRectangle(width: 100, height: 60),
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
                                    DestinIndustry: Industry
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
                                    HUDElementOrigin: new(HorizPosEnum.Middle, VertPosEnum.Middle),
                                    anchorInBaseWorldObject: new(HorizPosEnum.Middle, VertPosEnum.Middle)
                                )
                            );
                        }
                    )
                );
#warning Pause the game here. Also, when click anywhere else, cancel this action
                // PROBABLY want to pause the game here so that sources don't appear and disappear before the player's eyes
                CurWorldManager.DisableAllUIElements();

                foreach (var (toggleSourcePanel, _, panelHUDPosUpdater) in toggleSourcePanelManagers)
                    CurWorldManager.AddWorldHUDElement(worldHUDElement: toggleSourcePanel, updateHUDPos: panelHUDPosUpdater);

#warning Add arrow visual for this
            }
        }

        [Serializable]
        private sealed record ToggleRouteListener(EfficientReadOnlyCollection<ToggleSourcePanelManager> ToggleSourcePanelManagers, IResource Resource, IIndustry SourceIndustry, IIndustry DestinIndustry) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                SourceIndustry.ToggleDestin(resource: Resource, destinIndustry: DestinIndustry);
                DestinIndustry.ToggleSource(resource: Resource, sourceIndustry: SourceIndustry);

                foreach (var (toggleSourcePanel, _, _) in ToggleSourcePanelManagers)
                    CurWorldManager.RemoveWorldHUDElement(worldHUDElement: toggleSourcePanel);
                CurWorldManager.EnableAllUIElements();
            }
        }

        [Serializable]
        private readonly record struct ToggleSourcePanelManager(IHUDElement ToggleSourcePanel, IIndustry SourceIndustry, IAction PanelHUDPosUpdater);
    }
}
