using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class Node : UIElement
    {
        public Vector2 Position
            => state.position;
        public readonly float radius;
        public IEmployer Employer
            => industry;
        public IEnumerable<Person> UnemployedPeople
            => state.unemployedPeople;
        public override bool CanBeClicked
            => true;

        private readonly NodeState state;
        private readonly Shape shape;
        private readonly TextBox textBox;
        private readonly List<Link> links;
        private Industry industry;
        private readonly MyArray<ProporSplitter<Node>> resSplittersToDestins;
        private ConstULongArray targetStoredResAmounts;
        private readonly KeyButton incrDestinImp, decrDestinImp;
        private readonly MyArray<bool> store;
        private ULongArray undecidedResAmounts, resTravelHereAmounts;
        private bool active;

        private readonly UIHorizTabPanel<IUIElement<MyRectangle>> UITabPanel;
        private readonly UIRectPanel<IUIElement<NearRectangle>> buildButtonPannel;
        private readonly Dictionary<Overlay, UIRectPanel<IUIElement<NearRectangle>>> overlayTabPanels;
        private readonly string overlayTabLabel;
        private readonly Dictionary<Overlay, UITransparentPanel<IUIElement<Arrow>>> resDistribArrows;
        private readonly int resDistribArrowsUILayer;
        private readonly float resDestinArrowWidth;

        public Node(NodeState state, Shape shape, float letterHeight, float resDestinArrowWidth, int startPersonCount = 0)
            : base(shape: shape)
        {
            this.state = state;
            this.shape = shape;
            textBox = new(letterHeight: letterHeight);
            shape.Center = Position;
            textBox.Shape.Center = Position;
            SizeOrPosChanged += () =>
            {
                if (shape.Center != Position)
                    throw new Exception();
            };
            shape.Color = Color.White;
            links = new();
            industry = null;

            for (int i = 0; i < startPersonCount; i++)
                state.unemployedPeople.Add(Person.GenerateNew());
            resSplittersToDestins = new
            (
                values: from ind in Enumerable.Range(0, Resource.Count)
                        select new ProporSplitter<Node>()
            );
            targetStoredResAmounts = new();
            incrDestinImp = new(key: Keys.LeftShift);
            decrDestinImp = new(key: Keys.LeftControl);
            store = new(value: false);
            undecidedResAmounts = new();
            resTravelHereAmounts = new();
            active = false;

            textBox.Text = "";
            AddChild(child: textBox);

            UITabPanel = new
            (
                tabLabelWidth: 100,
                tabLabelHeight: 30,
                letterHeight: letterHeight,
                color: Color.White,
                inactiveTabLabelColor: Color.Gray
            );

            buildButtonPannel = new UIRectVertPanel<IUIElement<NearRectangle>>(color: Color.White);
            UITabPanel.AddTab
            (
                tabLabelText: "build",
                tab: buildButtonPannel
            );
            for (int i = 0; i < Industry.constrBuildingParams.Count; i++)
            {
                var parameters = Industry.constrBuildingParams[i];
                buildButtonPannel.AddChild
                (
                    child: new Button<Ellipse>
                    (
                        shape: new
                        (
                            width: 200,
                            height: 20
                        )
                        {
                            Color = Color.White
                        },
                        action: () =>
                        {
                            industry = parameters.MakeIndustry(state: state);
                            buildButtonPannel.PersonallyEnabled = false;
                        },
                        letterHeight: letterHeight,
                        text: "build " + parameters.industrParams.name
                    )
                );
            }

            overlayTabLabel = "overlay tab";
            overlayTabPanels = new();

            foreach (var overlay in Enum.GetValues<Overlay>())
                overlayTabPanels[overlay] = new UIRectVertPanel<IUIElement<NearRectangle>>(color: Color.Black);
            for (int resInd = 0; resInd <= (int)C.MaxRes; resInd++)
            {
                var storeToggle = new ToggleButton<MyRectangle>
                (
                    shape: new
                    (
                        width: 60,
                        height: 60
                    ),
                    letterHeight: letterHeight,
                    text: "store\nswitch",
                    on: store[resInd],
                    selectedColor: Color.White,
                    deselectedColor: Color.Gray
                );
                // hack to make lambda expression work correctly
                int curResInd = resInd;
                storeToggle.OnChanged += () => store[curResInd] = storeToggle.On;
                overlayTabPanels[(Overlay)resInd].AddChild(child: storeToggle);

                overlayTabPanels[(Overlay)resInd].AddChild
                (
                    child: new Button<MyRectangle>
                    (
                        shape: new(width: 100, height: 50)
                        {
                            Color = Color.White
                        },
                        action: () => ActiveUI.ArrowDrawingModeOn = true,
                        letterHeight: letterHeight,
                        text: "add resource\ndestination"
                    )
                );
            }
            UITabPanel.AddTab
            (
                tabLabelText: overlayTabLabel,
                tab: overlayTabPanels[Graph.Overlay]
            );

            resDistribArrowsUILayer = 1;
            resDistribArrows = new();
            foreach (var overlay in Enum.GetValues<Overlay>())
                resDistribArrows[overlay] = new UITransparentPanel<IUIElement<Arrow>>();
            this.resDestinArrowWidth = resDestinArrowWidth;

            //if (active)
            //Graph.World.AddUIElement(UIElement: resDistribArrows[Graph.Overlay], layer: resDistribArrowsUILayer);

            Graph.OverlayChanged += oldOverlay =>
            {
                UITabPanel.ReplaceTab
                (
                    tabLabelText: overlayTabLabel,
                    tab: overlayTabPanels[Graph.Overlay]
                );

                //if (active)
                //{
                Graph.World.RemoveUIElement
                (
                    UIElement: resDistribArrows[oldOverlay]
                );
                Graph.World.AddUIElement
                (
                    UIElement: resDistribArrows[Graph.Overlay],
                    layer: resDistribArrowsUILayer
                );
                //}
            };
        }

        public void Init()
            => Graph.World.AddUIElement(UIElement: resDistribArrows[Graph.Overlay], layer: resDistribArrowsUILayer);

        public void AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            links.Add(link);
        }

        public void AddText(string text)
            => textBox.Text += text;

        public void Arrive(ResAmountsPacketsByDestin resAmountsPackets)
        {
            state.waitingResAmountsPackets.Add(resAmountsPackets: resAmountsPackets);
            resTravelHereAmounts -= resAmountsPackets.ResToDestinAmounts(destination: Position);
        }

        public void Arrive(IEnumerable<Person> people)
            => state.waitingPeople.AddRange(people);

        public void Arrive(Person person)
            => state.waitingPeople.Add(person);

        public void AddResTravelHere(int resInd, ulong resAmount)
            => resTravelHereAmounts[resInd] += resAmount;

        public ulong TotalQueuedRes(int resInd)
            => state.storedRes[resInd] + resTravelHereAmounts[resInd];

        public bool Store(int resInd)
            => store[resInd];

        public IEnumerable<Node> ResDestins(int resInd)
            => resSplittersToDestins[resInd].Keys;

        public ulong TargetStoredResAmount(int resInd)
            => targetStoredResAmounts[resInd];

        public ulong StoredResAmount(int resInd)
            => state.storedRes[resInd];

        public override void OnClick()
        {
            base.OnClick();
            if (active)
                return;

            if (ActiveUI.ArrowDrawingModeOn)
                return;

            shape.Color = Color.Yellow;
            ActiveUI.AddHUDElement
            (
                UIElement: UITabPanel,
                horizPos: HorizPos.Right,
                vertPos: VertPos.Top
            );
            //Graph.World.AddUIElement
            //(
            //    UIElement: resDistribArrows[Graph.Overlay],
            //    layer: resDistribArrowsUILayer
            //);
            active = true;
        }

        private void ActiveUpdate()
        {
            incrDestinImp.Update();
            decrDestinImp.Update();
            if (Graph.Overlay <= C.MaxRes)
            {
                int resInd = (int)Graph.Overlay;

                if (MyMouse.RightClick)
                {
                    Node destinationNode = Graph.World.HoveringNode();
                    if (destinationNode is not null && destinationNode != this)
                    {
                        if (incrDestinImp.Hold)
                        {
                            if (!resSplittersToDestins[resInd].Proportions.ContainsKey(destinationNode))
                                resDistribArrows[(Overlay)resInd].AddChild
                                (
                                    child: new UIElement<Arrow>
                                    (
                                        shape: new Arrow(startPos: Position, endPos: destinationNode.Position, width: 10)
                                        {
                                            Color = Color.Red
                                        }
                                    )
                                );
                            resSplittersToDestins[resInd].AddToProp
                            (
                                key: destinationNode,
                                add: 1
                            );
                        }

                        if (decrDestinImp.Hold)
                            resSplittersToDestins[resInd].AddToProp
                            (
                                key: destinationNode,
                                add: -1
                            );
                    }
                }
            }

            foreach (var node in Graph.World.Nodes)
                node.AddText(text: $"personal distance {Graph.World.PersonDists[(Position, node.Position)]:0.##}\nresource distance {Graph.World.ResDists[(Position, node.Position)]:0.##}\n");
        }

        public void AddResDestin(Node destinationNode)
        {
            if (!active || !ActiveUI.ArrowDrawingModeOn)
                throw new InvalidOperationException();

            int resInd = (int)Graph.Overlay;
            if (!resSplittersToDestins[resInd].Proportions.ContainsKey(destinationNode))
            {
                resDistribArrows[(Overlay)resInd].AddChild
                (
                    child: new UIElement<Arrow>
                    (
                        shape: new Arrow(startPos: Position, endPos: destinationNode.Position, width: resDestinArrowWidth)
                        {
                            Color = Color.Red * .5f
                        }
                    )
                );

                resSplittersToDestins[resInd].AddToProp
                (
                    key: destinationNode,
                    add: 1
                );
            }
        }

        public override void OnMouseDownWorldNotMe()
        {
            base.OnMouseDownWorldNotMe();
            shape.Color = Color.White;
            ActiveUI.Remove(UIElement: UITabPanel);
            //Graph.World.RemoveUIElement
            //(
            //    UIElement: resDistribArrows[Graph.Overlay]
            //);
            active = false;
        }

        public void Update(TimeSpan elapsed)
        {
            if (active)
                ActiveUpdate();

            foreach (var person in state.unemployedPeople.Concat(state.waitingPeople))
                person.UpdateNotWorking(elapsed: elapsed);

            if (industry is not null)
                industry = industry.Update(elapsed: elapsed);

            // deal with people
            state.unemployedPeople.RemoveAll
            (
                match: person =>
                {
                    if (person.Destination is not null)
                    {
                        state.waitingPeople.Add(person);
                        return true;
                    }
                    return false;
                }
            );

            // take appropriate people and split the rest
            foreach (var person in state.waitingPeople)
            {
                if (person.Destination is null)
                {
                    state.unemployedPeople.Add(person);
                    continue;
                }

                if (person.Destination == Position)
                {
                    if (industry.IfEmploys(person: person))
                    {
                        person.StopTravelling();
                        industry.Take(person: person);
                        continue;
                    }
                    state.unemployedPeople.Add(person);
                    throw new Exception("Why were they travelling here is particular then?");
                    continue;
                }

                Graph.World.PersonFirstLinks[(Position, person.Destination.Value)].Add(start: this, person: person);
            }
            state.waitingPeople = new();
        }

        public void StartSplitRes()
        {
            targetStoredResAmounts = industry switch
            {
                null => new(),
                not null => industry.TargetStoredResAmounts()
            };

            // deal with resources
            undecidedResAmounts = state.storedRes + state.waitingResAmountsPackets.ReturnAndRemove(destination: Position);
            state.storedRes = new();

            state.storedRes = undecidedResAmounts.Min(ulongArray: targetStoredResAmounts);
            undecidedResAmounts -= state.storedRes;
        }

        /// <summary>
        /// MUST call StartSplitRes first
        /// </summary>
        public void SplitRes(int resInd, Func<Node, ulong> maxExtraResFunc)
        {
            if (undecidedResAmounts[resInd] is 0)
                return;

            var resSplitter = resSplittersToDestins[resInd];
            if (resSplitter.Empty)
                state.storedRes[resInd] += undecidedResAmounts[resInd];
            else
            {
                var (splitResAmounts, unsplitResAmount) = resSplitter.Split(amount: undecidedResAmounts[resInd], maxAmountsFunc: maxExtraResFunc);
                state.storedRes[resInd] += unsplitResAmount;
                foreach (var (destinationNode, resAmount) in splitResAmounts)
                {
                    state.waitingResAmountsPackets.Add
                    (
                        destination: destinationNode.Position,
                        resInd: resInd,
                        resAmount: resAmount
                    );
                    destinationNode.AddResTravelHere(resInd: resInd, resAmount: resAmount);
                }
            }

            undecidedResAmounts[resInd] = 0;
        }

        /// <summary>
        /// MUST call SplitRes first
        /// </summary>
        public void EndSplitRes()
        {
            undecidedResAmounts = new();

            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                Vector2 destination = resAmountsPacket.destination;
                Debug.Assert(destination != Position);

                Graph.World.ResFirstLinks[(Position, destination)].Add(start: this, resAmountsPacket: resAmountsPacket);
            }

            // update text
            textBox.Text = "";
            if (industry is not null)
                textBox.Text += industry.GetText();

            textBox.Text += Graph.Overlay switch
            {
                <= C.MaxRes => $"store {store[(int)Graph.Overlay]}\n" + (state.storedRes[(int)Graph.Overlay] >= targetStoredResAmounts[(int)Graph.Overlay] ?
                    $"have {state.storedRes[(int)Graph.Overlay] - targetStoredResAmounts[(int)Graph.Overlay]} extra resources" : $"have {(double)state.storedRes[(int)Graph.Overlay] / targetStoredResAmounts[(int)Graph.Overlay] * 100:0.}% of target stored resources\n"),
                Overlay.AllRes => $"stored total res weight {state.storedRes.TotalWeight()}",
                Overlay.People => $"unemployed {state.unemployedPeople.Count}\n",
                _ => throw new Exception(),
            };
        }

        public override void Draw()
        {
            //Draw amount of resources in storage
            //or write percentage of required res

            base.Draw();

            if (active && ActiveUI.ArrowDrawingModeOn)
                Arrow.DrawArrow(startPos: Position, endPos: MyMouse.WorldPos, width: resDestinArrowWidth, color: Color.Red * .25f);

            //if (Graph.Overlay <= C.MaxRes)
            //{
            //    var proportions = resSplittersToDestins[(int)Graph.Overlay].Proportions;
            //    decimal propSum = proportions.Values.Sum();
            //    foreach (var (destinationNode, proportion) in proportions)
            //    {
            //        Debug.Assert(destinationNode is not null && destinationNode != this
            //            && !C.IsTiny(proportion) && proportion > 0);

            //        ArrowDrawer.DrawArrow
            //        (
            //            start: Position,
            //            end: destinationNode.Position,
            //            color: Color.Red * (float)(proportion / propSum)
            //        );
            //    }
            //}
        }
    }
}
