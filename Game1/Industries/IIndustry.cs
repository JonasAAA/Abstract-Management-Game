﻿using Game1.Collections;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using static Game1.WorldManager;
using static Game1.GameConfig;

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
            if (newIndustry == this)
                return newIndustry;

            if (newIndustry is not null)
            {
                // Do this before deleting the old industry so that GetResWithNonEmptyNeighborhood, GetResNeighbors, etc. return thruthful results rather than empty things
                foreach (var neighborDir in Enum.GetValues<NeighborDir>())
                    foreach (var res in newIndustry.GetResWithPotentialNeighborhood(neighborDir: neighborDir))
                        if (IsNeighborhoodPossible(neighborDir: neighborDir.Opposite(), resource: res))
                            foreach (var neighbor in GetResNeighbors(neighborDir: neighborDir.Opposite(), resource: res))
                                ToggleResEdge(neighborDir: neighborDir, resource: res, industry: newIndustry, potentialNeighbor: neighbor);
            }

            Delete();
            return newIndustry;
        }

        public IIndustry? UpdateImpl();

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
            => new RoutePanelManager(industry: industry).routePanel;

        [Serializable]
        private readonly struct RoutePanelManager
        {
            public readonly UIRectVertPanel<IHUDElement> routePanel;

            public RoutePanelManager(IIndustry industry)
            {
                const HorizPosEnum childHorizPos = HorizPosEnum.Left;
                
                routePanel = new
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
                                            Button toggleResNeighborButton = new
                                            (
                                                shape: new MyRectangle(width: CurGameConfig.wideUIElementWidth, height: CurGameConfig.UILineHeight),
                                                tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ToggleResNeighborTooltip(neighborDir: neighborDir, res: res)),
                                                text: res.ToString()
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
        }

        [Serializable]
        private sealed class ChangeResNeighborsButtonListener(IIndustry industry, NeighborDir neighborDir, IResource resource) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                // Needed so that can pass toggleNeighborPanelManagers when creating ChooseSourceButton clicked response
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
                            Button toggleNeighborButton = new
                            (
                                shape: new MyRectangle(width: CurGameConfig.standardUIElementWidth, height: 2 * CurGameConfig.UILineHeight),
                                tooltip: new ImmutableTextTooltip(text: UIAlgorithms.ToggleResNeighborTooltip(neighborDir: neighborDir, res: resource, add: add)),
                                text: UIAlgorithms.ToggleResNeighborButtonName(neighborDir: neighborDir, add: add)
                            );

                            toggleNeighborButton.clicked.Add
                            (
                                listener: new ToggleRouteListener
                                (
                                    toggleNeighborPanelManagers: toggleNeighborPanelManagers.ToEfficientReadOnlyCollection(),
                                    neighborDir: neighborDir,
                                    resource: resource,
                                    industry: industry,
                                    potentialNeighbor: potentialNeighbor
                                )
                            );

                            return new ToggleNeighborPanelManager
                            (
                                ToggleNeighborPanel: toggleNeighborButton,
                                PotentialNeighbor: potentialNeighbor,
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
#warning Pause the game here. Also, when click anywhere else, cancel this action
                // PROBABLY want to pause the game here so that sources don't appear and disappear before the player's eyes

                Debug.Assert(CurWorldManager.IsCosmicBodyActive(nodeID: industry.NodeID));
                CurWorldManager.SetIsCosmicBodyActive(nodeID: industry.NodeID, active: false);
                CurWorldManager.DisableAllUIElements();

                foreach (var (toggleSourcePanel, _, panelHUDPosUpdater) in toggleNeighborPanelManagers)
                    CurWorldManager.AddWorldHUDElement(worldHUDElement: toggleSourcePanel, updateHUDPos: panelHUDPosUpdater);

#warning Add arrow visual for this
            }
        }

        [Serializable]
        private sealed class ToggleRouteListener(EfficientReadOnlyCollection<ToggleNeighborPanelManager> toggleNeighborPanelManagers, NeighborDir neighborDir, IResource resource, IIndustry industry, IIndustry potentialNeighbor) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
            {
                ToggleResEdge(neighborDir: neighborDir, resource: resource, industry: industry, potentialNeighbor: potentialNeighbor);

                foreach (var (toggleNeighborPanel, _, _) in toggleNeighborPanelManagers)
                    CurWorldManager.RemoveWorldHUDElement(worldHUDElement: toggleNeighborPanel);
                CurWorldManager.EnableAllUIElements();
                Debug.Assert(!CurWorldManager.IsCosmicBodyActive(nodeID: industry.NodeID));
                //CurWorldManager.SetIsCosmicBodyActive(nodeID: Industry.NodeID, active: true);
            }
        }

        [Serializable]
        private readonly record struct ToggleNeighborPanelManager(IHUDElement ToggleNeighborPanel, IIndustry PotentialNeighbor, IAction PanelHUDPosUpdater);
    }
}
