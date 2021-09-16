using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class Graph : UIElement
    {
        private class EndlessRect : Shape
        {
            public EndlessRect()
                => Color = Color.Transparent;

            public override bool Contains(Vector2 position)
                => true;

            public override void Draw()
            { }
        }

        public static Graph World { get; private set; }

        public static void InitializeWorld(IEnumerable<Node> nodes, IEnumerable<Link> links, Overlay overlay, float letterHeight)
        {
            if (World is not null)
                throw new InvalidOperationException();

            World = new Graph(nodes: nodes, links: links, overlay: overlay, letterHeight);
        }

        public Overlay Overlay { get; private set; }
        public IEnumerable<Node> Nodes
            => nodes;
        public IEnumerable<Link> Links
            => links;
        public ReadOnlyDictionary<(Vector2, Vector2), double> PersonDists { get; private set; }
        public ReadOnlyDictionary<(Vector2, Vector2), double> ResDists { get; private set; }
        public TimeSpan MaxLinkTravelTime { get; private set; }
        public double MaxLinkWattsPerKg { get; private set; }
        /// <summary>
        /// if both key nodes are the same, value is null
        /// </summary>
        public ReadOnlyDictionary<(Vector2, Vector2), Link> PersonFirstLinks { get; private set; }
        public ReadOnlyDictionary<(Vector2, Vector2), Link> ResFirstLinks { get; private set; }
        private readonly EndlessRect background;

        private readonly List<Node> nodes;
        private readonly List<Link> links;
        private readonly HashSet<Node> nodeSet;
        private readonly HashSet<Link> linkSet;
        private readonly double persDistTimeCoeff, persDistElectrCoeff, resDistTimeCoeff, resDistElectrCoeff;
        private readonly KeyButton[] overlayKeyButtons;
        private readonly KeyButton pauseKey;
        private readonly TextBox globalTextBox;
        private bool paused;

        private Graph(IEnumerable<Node> nodes, IEnumerable<Link> links, Overlay overlay, float letterHeight)
        {
            this.nodes = new();
            this.links = new();
            nodeSet = new();
            linkSet = new();
            persDistTimeCoeff = 1;
            persDistElectrCoeff = 0;
            resDistTimeCoeff = 0;
            resDistElectrCoeff = 1;

            foreach (var node in nodes)
                AddNode(node);
            foreach (var link in links)
                AddLink(link);

            MaxLinkTravelTime = this.links.Max(link => link.TravelTime);
            MaxLinkWattsPerKg = this.links.Max(link => link.WattsPerKg);

            (PersonDists, PersonFirstLinks) = FindShortestPaths(distTimeCoeff: persDistTimeCoeff, distElectrCoeff: persDistElectrCoeff);
            (ResDists, ResFirstLinks) = FindShortestPaths(distTimeCoeff: resDistTimeCoeff, distElectrCoeff: resDistElectrCoeff);

            Overlay = overlay;

            overlayKeyButtons = new KeyButton[Enum.GetValues<Overlay>().Length];
            Debug.Assert(overlayKeyButtons.Length <= C.numericKeys.Count);
            for (int i = 0; i < overlayKeyButtons.Length; i++)
            {
                // hack for lambda expression to work correctly
                int overlayInd = i;
                overlayKeyButtons[i] = new
                (
                    key: C.numericKeys[overlayInd],
                    action: () => Overlay = (Overlay)overlayInd
                );
            }

            pauseKey = new
            (
                key: Keys.Space,
                action: () => paused = !paused
            );
            paused = false;

            background = new();
            
            if (ActiveUI.Count is not 0)
                throw new Exception();

            ActiveUI.AddWorldElement(UIElement: this);

            globalTextBox = new(letterHeight: letterHeight);
            globalTextBox.Shape.MinWidth = 250;
            globalTextBox.Shape.Color = Color.White;
            ActiveUI.AddHUDElement
            (
                UIElement: globalTextBox,
                horizPos: HorizPos.Left,
                vertPos: VertPos.Top
            );
        }

        protected override Shape GetShape()
            => background;

        protected override IEnumerable<UIElement> GetChildren()
            => links.Cast<UIElement>().Concat(nodes);

        private void AddNode(Node node)
        {
            if (nodeSet.Contains(node))
                throw new ArgumentException();
            nodeSet.Add(node);
            nodes.Add(node);
        }

        private void AddLink(Link link)
        {
            if (linkSet.Contains(link))
                throw new ArgumentException();
            linkSet.Add(link);
            links.Add(link);

            link.node1.AddLink(link: link);
            link.node2.AddLink(link: link);
        }

        // currently uses Floyd-Warshall;
        // Dijkstra would be more efficient
        private (ReadOnlyDictionary<(Vector2, Vector2), double> dists, ReadOnlyDictionary<(Vector2, Vector2), Link> firstLinks) FindShortestPaths(double distTimeCoeff, double distElectrCoeff)
        {
            if (distTimeCoeff < 0)
                throw new ArgumentOutOfRangeException();
            if (distElectrCoeff < 0)
                throw new ArgumentOutOfRangeException();

            double[,] distsArray = new double[nodes.Count, nodes.Count];
            Link[,] firstLinksArray = new Link[nodes.Count, nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
                for (int j = 0; j < nodes.Count; j++)
                {
                    distsArray[i, j] = double.PositiveInfinity;
                    firstLinksArray[i, j] = null;
                }

            for (int i = 0; i < nodes.Count; i++)
                distsArray[i, i] = 0;

            foreach (var link in links)
            {
                int i = nodes.IndexOf(link.node1), j = nodes.IndexOf(link.node2);
                Debug.Assert(i >= 0 && j >= 0);
                distsArray[i, j] = distTimeCoeff * link.TravelTime.TotalSeconds + distElectrCoeff * link.WattsPerKg;
                distsArray[j, i] = distsArray[i, j];
                firstLinksArray[i, j] = link;
                firstLinksArray[j, i] = link;
            }

            for (int k = 0; k < nodes.Count; k++)
                for (int i = 0; i < nodes.Count; i++)
                    for (int j = 0; j < nodes.Count; j++)
                        if (i != k && distsArray[i, j] > distsArray[i, k] + distsArray[k, j])
                        {
                            distsArray[i, j] = distsArray[i, k] + distsArray[k, j];
                            firstLinksArray[i, j] = firstLinksArray[i, k];
                            Debug.Assert(firstLinksArray[i, j] is not null);
                        }

            Dictionary<(Vector2, Vector2), double> distsDict = new();
            Dictionary<(Vector2, Vector2), Link> firstLinksDict = new();
            for (int i = 0; i < nodes.Count; i++)
                for (int j = 0; j < nodes.Count; j++)
                {
                    distsDict.Add
                    (
                        key: (nodes[i].Position, nodes[j].Position),
                        value: distsArray[i, j]
                    );
                    firstLinksDict.Add
                    (
                        key: (nodes[i].Position, nodes[j].Position),
                        value: firstLinksArray[i, j]
                    );
                }

            return (dists: new(distsDict), firstLinks: new(firstLinksDict));
        }

        /// <returns> returns null if not hovering above a node </returns>
        public Node HoveringNode()
        {
            Node result = null;
            foreach (var node in nodes)
                if (node.Contains(position: MyMouse.WorldPos))
                {
                    result = node;
                    break;
                }

            return result;
        }

        public void Update(TimeSpan elapsed)
        {
            pauseKey.Update();

            if (paused)
                elapsed = TimeSpan.Zero;

            ElectricityDistributor.DistributeElectr();

            links.ForEach(link => link.Update(elapsed: elapsed));

            nodes.ForEach(node => node.Update(elapsed: elapsed));

            nodes.ForEach(node => node.StartSplitRes());

            for (int resInd = 0; resInd < Resource.Count; resInd++)
                SplitRes(resInd: resInd);

            nodes.ForEach(node => node.EndSplitRes());

            JobMatching.Match();

            globalTextBox.Text = $"overlay {Overlay}\n" + ElectricityDistributor.Summary();
        }

        private class BetterNode
        {
            private static int resInd;

            public static void Init(int resInd)
                => BetterNode.resInd = resInd;

            public readonly Node node;
            public readonly List<BetterNode> nodesIn, nodesOut;
            public uint unvisitedDestinsCount;
            public bool isSplitAleady;

            public BetterNode(Node node)
            {
                this.node = node;
                nodesIn = new();
                nodesOut = new();
                unvisitedDestinsCount = 0;
                isSplitAleady = false;
            }

            public ulong MaxExtraRes()
                => unvisitedDestinsCount switch
                {
                    0 => DFS().maxExtraRes,
                    > 0 => ulong.MaxValue
                };

            private (ulong maxExtraRes, ulong subgraphUserTargetStoredRes) DFS()
            {
                if (unvisitedDestinsCount is not 0)
                    throw new InvalidOperationException();

                ulong maxExtraResFromNodesOut = 0,
                    userTargetStoredResFromNodesOut = 0;

                foreach (var betterNode in nodesOut)
                {
                    var (curMaxExtraRes, curSubgraphUserTargetStoredRes) = betterNode.DFS();
                    maxExtraResFromNodesOut += curMaxExtraRes;
                    userTargetStoredResFromNodesOut += curSubgraphUserTargetStoredRes;
                }

                ulong subgraphUserTargetStoredRes = node.TargetStoredResAmount(resInd: resInd) + userTargetStoredResFromNodesOut,
                    targetStoredRes = node.Store(resInd: resInd) switch
                    {
                        true => subgraphUserTargetStoredRes,
                        false => node.TargetStoredResAmount(resInd: resInd)
                    };

                return
                (
                    maxExtraRes: (maxExtraResFromNodesOut + targetStoredRes >= node.TotalQueuedRes(resInd: resInd)) switch
                    {
                        true => maxExtraResFromNodesOut + targetStoredRes - node.TotalQueuedRes(resInd: resInd),
                        false => 0
                    },
                    subgraphUserTargetStoredRes: subgraphUserTargetStoredRes
                );
            }
        }

        /// <summary>
        /// TODO:
        /// choose random leafs
        /// </summary>
        public void SplitRes(int resInd)
        {
            BetterNode.Init(resInd: resInd);
            Dictionary<Node, BetterNode> betterNodes = nodes.ToDictionary
            (
                keySelector: node => node,
                elementSelector: node => new BetterNode(node: node)
            );

            foreach (var betterNode in betterNodes.Values)
                foreach (var resDestin in betterNode.node.ResDestins(resInd: resInd))
                {
                    var betterNodeDestin = betterNodes[resDestin];

                    betterNode.unvisitedDestinsCount++;
                    betterNode.nodesOut.Add(betterNodeDestin);
                    betterNodeDestin.nodesIn.Add(betterNode);
                }

            Queue<BetterNode> leafs = new
            (
                collection: from betterNode in betterNodes.Values
                            where betterNode.unvisitedDestinsCount is 0
                            select betterNode
            );

            ulong MaxExtraRes(Node node)
                => betterNodes[node].MaxExtraRes();

            while (leafs.Count > 0)
            {
                // want to choose random leaf instead of this
                BetterNode leaf = leafs.Dequeue();
                leaf.node.SplitRes(resInd: resInd, maxExtraResFunc: MaxExtraRes);

                foreach (var betterNode in leaf.nodesIn)
                {
                    betterNode.unvisitedDestinsCount--;
                    if (betterNode.unvisitedDestinsCount is 0)
                        leafs.Enqueue(betterNode);
                }
                leaf.isSplitAleady = true;
            }

            foreach (var betterNode in betterNodes.Values)
                if (!betterNode.isSplitAleady)
                {
                    betterNode.node.SplitRes(resInd: resInd, maxExtraResFunc: MaxExtraRes);
                    betterNode.isSplitAleady = true;
                }
        }
    }
}
