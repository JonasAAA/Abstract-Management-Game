using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class Graph : UIElement
    {
        public delegate void OverlayChangedEventHandler(Overlay oldOverlay);

        public static Overlay Overlay { get; private set; }

        public static event OverlayChangedEventHandler OverlayChanged;

        public static Graph World { get; private set; }

        public static void InitializeWorld(IEnumerable<Node> nodes, IEnumerable<Link> links, Overlay overlay)
        {
            if (World is not null)
                throw new InvalidOperationException();

            World = new Graph(nodes: nodes, links: links, overlay: overlay);
            foreach (var node in nodes)
                node.Init(startPersonCount: 5);
        }

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
        public ReadOnlyDictionary<Vector2, Node> PosToNode { get; private set; }

        private readonly List<Node> nodes;
        private readonly List<Link> links;
        private readonly HashSet<Node> nodeSet;
        private readonly HashSet<Link> linkSet;
        private readonly MyHashSet<Person> people;
        private readonly double persDistTimeCoeff, persDistElectrCoeff, resDistTimeCoeff, resDistElectrCoeff;
        private readonly TextBox globalTextBox;
        private bool paused;

        private Graph(IEnumerable<Node> nodes, IEnumerable<Link> links, Overlay overlay)
            : base(shape: new InfinitePlane())
        {
            this.nodes = new();
            this.links = new();
            nodeSet = new();
            linkSet = new();
            people = new();
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
            PosToNode = new
            (
                dictionary: nodes.ToDictionary
                (
                    keySelector: nodes => nodes.Position
                )
            );

            Overlay = overlay;

            paused = false;
            
            if (ActiveUI.Count is not 0)
                throw new Exception();

            ActiveUI.AddWorldElement(UIElement: this);

            globalTextBox = new();
            globalTextBox.Shape.MinWidth = 250;
            globalTextBox.Shape.Color = Color.White;
            ActiveUI.AddHUDElement
            (
                UIElement: globalTextBox,
                horizPos: HorizPos.Left,
                vertPos: VertPos.Top
            );

            ToggleButton<MyRectangle> pauseButton = new
            (
                shape: new
                (
                    width: 60,
                    height: 60
                ),
                on: false,
                text: "Toggle\nPause",
                selectedColor: Color.White,
                deselectedColor: Color.Gray
            );

            pauseButton.OnChanged += () => paused = pauseButton.On;

            ActiveUI.AddHUDElement
            (
                UIElement: pauseButton,
                horizPos: HorizPos.Right,
                vertPos: VertPos.Bottom
            );

            MultipleChoicePanel overlayChoicePanel = new
            (
                horizontal: true,
                choiceWidth: 100,
                choiceHeight: 30,
                selectedColor: Color.White,
                deselectedColor: Color.Gray,
                backgroundColor: Color.White
            );

            foreach (var posOverlay in Enum.GetValues<Overlay>())
                overlayChoicePanel.AddChoice
                (
                    choiceText: posOverlay.ToString(),
                    select: () =>
                    {
                        if (Overlay == posOverlay)
                            return;
                        Overlay oldOverlay = Overlay;
                        Overlay = posOverlay;
                        OverlayChanged?.Invoke(oldOverlay: oldOverlay);
                    }
                );

            ActiveUI.AddHUDElement
            (
                UIElement: overlayChoicePanel,
                horizPos: HorizPos.Middle,
                vertPos: VertPos.Top
            );
        }

        private void AddNode(Node node)
        {
            if (nodeSet.Contains(node))
                throw new ArgumentException();
            nodeSet.Add(node);
            nodes.Add(node);
            AddChild(child: node, layer: 10);
        }

        private void AddLink(Link link)
        {
            if (linkSet.Contains(link))
                throw new ArgumentException();
            linkSet.Add(link);
            links.Add(link);

            link.node1.AddLink(link: link);
            link.node2.AddLink(link: link);
            AddChild(child: link, layer: 0);
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

        public void AddPerson(Person person)
            => people.Add(person);

        public void AddUIElement(IUIElement UIElement, int layer)
            => AddChild(child: UIElement, layer: layer);

        public void RemoveUIElement(IUIElement UIElement)
            => RemoveChild(child: UIElement);

        public void Update(TimeSpan elapsed)
        {
            if (paused)
                elapsed = TimeSpan.Zero;

            ElectricityDistributor.DistributeElectr();

            links.ForEach(link => link.Update(elapsed: elapsed));
            nodes.ForEach(node => node.Update(elapsed: elapsed));

            links.ForEach(link => link.UpdatePeople(elapsed: elapsed));
            nodes.ForEach(node => node.UpdatePeople(elapsed: elapsed));

            nodes.ForEach(node => node.StartSplitRes());

            for (int resInd = 0; resInd < Resource.Count; resInd++)
                SplitRes(resInd: resInd);

            nodes.ForEach(node => node.EndSplitRes());

            ActivityManager.ManageActivities();

            //foreach (var person in people)
            //    person.Update();

            //what to do when person finds a job and a partner in the same frame?
            //JobMatching.Match();

            //PeopleReproduction.Match();

            globalTextBox.Text = ElectricityDistributor.Summary().Trim();
        }

        private class NodeInfo
        {
            private static int resInd;

            public static void Init(int resInd)
                => NodeInfo.resInd = resInd;

            public readonly Node node;
            public readonly List<NodeInfo> nodesIn, nodesOut;
            public uint unvisitedDestinsCount;
            public bool isSplitAleady;

            public NodeInfo(Node node)
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

                foreach (var nodeInfo in nodesOut)
                {
                    var (curMaxExtraRes, curSubgraphUserTargetStoredRes) = nodeInfo.DFS();
                    maxExtraResFromNodesOut += curMaxExtraRes;
                    userTargetStoredResFromNodesOut += curSubgraphUserTargetStoredRes;
                }

                ulong subgraphUserTargetStoredRes = node.TargetStoredResAmount(resInd: resInd) + userTargetStoredResFromNodesOut,
                    targetStoredRes = node.IfStore(resInd: resInd) switch
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
            NodeInfo.Init(resInd: resInd);
            Dictionary<Vector2, NodeInfo> nodeInfos = nodes.ToDictionary
            (
                keySelector: node => node.Position,
                elementSelector: node => new NodeInfo(node: node)
            );

            foreach (var nodeInfo in nodeInfos.Values)
                foreach (var resDestin in nodeInfo.node.ResDestins(resInd: resInd))
                {
                    var nodeInfoDestin = nodeInfos[resDestin];

                    nodeInfo.unvisitedDestinsCount++;
                    nodeInfo.nodesOut.Add(nodeInfoDestin);
                    nodeInfoDestin.nodesIn.Add(nodeInfo);
                }

            Queue<NodeInfo> leafs = new
            (
                collection: from nodeInfo in nodeInfos.Values
                            where nodeInfo.unvisitedDestinsCount is 0
                            select nodeInfo
            );

            ulong MaxExtraRes(Vector2 position)
                => nodeInfos[position].MaxExtraRes();

            while (leafs.Count > 0)
            {
                // want to choose random leaf instead of this
                NodeInfo leaf = leafs.Dequeue();
                leaf.node.SplitRes(resInd: resInd, maxExtraResFunc: MaxExtraRes);

                foreach (var nodeInfo in leaf.nodesIn)
                {
                    nodeInfo.unvisitedDestinsCount--;
                    if (nodeInfo.unvisitedDestinsCount is 0)
                        leafs.Enqueue(nodeInfo);
                }
                leaf.isSplitAleady = true;
            }

            foreach (var nodeInfo in nodeInfos.Values)
                if (!nodeInfo.isSplitAleady)
                {
                    nodeInfo.node.SplitRes(resInd: resInd, maxExtraResFunc: MaxExtraRes);
                    nodeInfo.isSplitAleady = true;
                }
        }

        public IEnumerable<Person> GetActivitySeekingPeople()
            => from person in people
               where person.IfSeeksNewActivity()
               select person;
    }
}
