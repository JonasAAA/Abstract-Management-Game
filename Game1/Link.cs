using Game1.Delegates;
using Game1.Inhabitants;
using Game1.Shapes;
using Game1.UI;
using System.Diagnostics.CodeAnalysis;
using static Game1.WorldManager;

namespace Game1
{
    // TODO: consider making this record class, but see comment below (where operator == is commented out)
    [Serializable]
    public sealed class Link : WorldUIElement
    {
        [Serializable]
        private sealed class DirLink : IEnergyConsumer
        {
            private static readonly Texture2D diskTexture;

            static DirLink()
                => diskTexture = C.LoadTexture(name: "big disk");

            /// <summary>
            /// CURRENTLY UNUSED
            /// </summary>
            public IEvent<IDeletedListener> Deleted
                => deleted;

            public readonly ILinkFacingPlanet startNode, endNode;
            public UDouble JoulesPerKg
                => (UDouble)timedPacketQueue.duration.TotalSeconds * reqWattsPerKg;
            public TimeSpan TravelTime
                => timedPacketQueue.duration;

            private readonly TimedPacketQueue timedPacketQueue;
            private readonly Propor minSafePropor;
            private readonly ResAmountsPacketsByDestin waitingResAmountsPackets;
            private readonly RealPeople waitingPeople;
            private readonly UDouble reqWattsPerKg;
            private Propor energyPropor;
            private readonly Event<IDeletedListener> deleted;

            public DirLink(ILinkFacingPlanet startNode, ILinkFacingPlanet endNode, TimeSpan travelTime, UDouble wattsPerKg, UDouble minSafeDist)
            {
                this.startNode = startNode;
                this.endNode = endNode;

                timedPacketQueue = new(duration: travelTime);
                minSafePropor = Propor.Create(part: minSafeDist, whole: MyVector2.Distance(startNode.Position, endNode.Position)) switch
                {
                    Propor propor => propor,
                    null => throw new ArgumentException()
                };
                waitingResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty();
                waitingPeople = RealPeople.CreateEmpty();
                if (wattsPerKg.IsCloseTo(other: 0))
                    throw new ArgumentOutOfRangeException();
                reqWattsPerKg = wattsPerKg / (UDouble)travelTime.TotalSeconds;
                energyPropor = Propor.empty;
                deleted = new();

                CurWorldManager.AddEnergyConsumer(energyConsumer: this);
            }

            public void TransferAllFrom(ResAmountsPacket resAmountsPacket)
                => waitingResAmountsPackets.TransferAllFrom(sourcePacket: resAmountsPacket);

            public void TransferAllFrom(RealPeople realPeople)
                => waitingPeople.TransferAllFrom(realPeopleSource: realPeople);

            public void TransferFrom(RealPeople realPersonSource, RealPerson realPerson)
                => waitingPeople.TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);

            public ulong GetTravellingAmount()
                => CurWorldManager.Overlay.SwitchExpression
                (
                    singleResCase: resInd => timedPacketQueue.TotalResAmounts[resInd],
                    allResCase: () => timedPacketQueue.TotalResAmounts.TotalMass(),
                    peopleCase: () => timedPacketQueue.PeopleCount,
                    powerCase: () => throw new InvalidOperationException()
                );

            public void Update()
            {
                timedPacketQueue.Update(workingPropor: energyPropor);
                var (resAmountsPackets, people) = timedPacketQueue.DonePacketsAndPeople();
                endNode.Arrive(resAmountsPackets: resAmountsPackets);
                endNode.Arrive(realPeople: people);

                if ((!waitingResAmountsPackets.Empty || waitingPeople.Count > 0)
                    && (timedPacketQueue.Count is 0 || timedPacketQueue.LastCompletionPropor() >= minSafePropor))
                    timedPacketQueue.Enqueue(resAmountsPackets: waitingResAmountsPackets, realPeople: waitingPeople);
            }

            public void UpdatePeople()
            {
                RealPerson.UpdateParams personUpdateParams = new(LastNodeID: startNode.NodeID, ClosestNodeID: endNode.NodeID);
                timedPacketQueue.UpdatePeople(updateParams: personUpdateParams, personalUpdate: null);
                waitingPeople.Update(updateParams: personUpdateParams, personalUpdate: null);
            }

            public void DrawTravelingRes()
            {
                // temporary
                CurWorldManager.Overlay.SwitchStatement
                (
                    singleResCase: resInd =>
                    {
                        foreach (var (complProp, resAmounts, _) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmounts[resInd]);
                    },
                    allResCase: () =>
                    {
                        foreach (var (complProp, resAmounts, _) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmounts.TotalMass());
                    },
                    powerCase: () => { },
                    peopleCase: () =>
                    {
                        foreach (var (complProp, _, peopleCount) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: peopleCount);
                    }
                );

                void DrawDisk(Propor complProp, UDouble size)
                    => C.Draw
                    (
                        texture: diskTexture,
                        position: startNode.Position + (double)complProp * (endNode.Position - startNode.Position),
                        color: CurWorldConfig.linkTravellerColor,
                        rotation: 0,
                        origin: new MyVector2(diskTexture.Width * .5, diskTexture.Height * .5),
                        scale: MyMathHelper.Sqrt(size) * 2 / (UDouble)diskTexture.Width
                    );
            }

            EnergyPriority IEnergyConsumer.EnergyPriority
                => CurWorldConfig.linkEnergyPriority;

            NodeID IEnergyConsumer.NodeID
                => startNode.NodeID;

            UDouble IEnergyConsumer.ReqWatts()
                => timedPacketQueue.Mass * reqWattsPerKg;

            void IEnergyConsumer.ConsumeEnergy(Propor energyPropor)
                => this.energyPropor = energyPropor;
        }

        [Serializable]
        private readonly record struct ShapeParams(ILinkFacingPlanet Node1, ILinkFacingPlanet Node2) : VectorShape.IParams
        {
            public MyVector2 StartPos
                => Node1.Position;

            public MyVector2 EndPos
                => Node2.Position;

            public UDouble Width
                => CurWorldConfig.linkWidth;
        }

        public readonly ILinkFacingPlanet node1, node2;
        public UDouble JoulesPerKg
            => link1To2.JoulesPerKg;
        public TimeSpan TravelTime
            => link1To2.TravelTime;

        private readonly DirLink link1To2, link2To1;
        private readonly MyArray<TextBox> resTextBoxes;
        private readonly TextBox allResTextBox, peopleTextBox;

        public Link(ILinkFacingPlanet node1, ILinkFacingPlanet node2, TimeSpan travelTime, UDouble wattsPerKg, UDouble minSafeDist)
            : base
            (
                shape: new LineSegment
                (
                    parameters: new ShapeParams(Node1: node1, Node2: node2)
                ),
                activeColor: Color.White,
                inactiveColor: Color.Green,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            if (node1 == node2)
                throw new ArgumentException();

            this.node1 = node1;
            this.node2 = node2;

            link1To2 = new(startNode: node1, endNode: node2, travelTime: travelTime, wattsPerKg: wattsPerKg, minSafeDist: minSafeDist);
            link2To1 = new(startNode: node2, endNode: node1, travelTime: travelTime, wattsPerKg: wattsPerKg, minSafeDist: minSafeDist);

            resTextBoxes = new();
            foreach (var resInd in ResInd.All)
            {
                resTextBoxes[resInd] = new(backgroundColor: Color.White);
                SetPopup(HUDElement: resTextBoxes[resInd], overlay: resInd);
            }

            allResTextBox = new(backgroundColor: Color.White);
            SetPopup(HUDElement: allResTextBox, overlay: IOverlay.allRes);

            peopleTextBox = new(backgroundColor: Color.White);
            SetPopup(HUDElement: peopleTextBox, overlay: IOverlay.people);
        }

        public ILinkFacingPlanet OtherNode(ILinkFacingPlanet node)
        {
            if (!Contains(node))
                throw new ArgumentException();
            return node == node1 ? node2 : node1;
        }

        public bool Contains(ILinkFacingPlanet node)
            => node == node1 || node == node2;

        private DirLink GetDirLink(ILinkFacingPlanet start)
        {
            if (start == node1)
                return link1To2;
            if (start == node2)
                return link2To1;
            throw new ArgumentException();
        }

        public void TransferAllFrom(ILinkFacingPlanet start, ResAmountsPacket resAmountsPacket)
            => GetDirLink(start: start).TransferAllFrom(resAmountsPacket: resAmountsPacket);

        public void TransferAllFrom(ILinkFacingPlanet start, RealPeople realPeople)
            => GetDirLink(start: start).TransferAllFrom(realPeople: realPeople);

        public void TransferFrom(ILinkFacingPlanet start, RealPeople realPersonSource, RealPerson realPerson)
            => GetDirLink(start: start).TransferFrom(realPersonSource: realPersonSource, realPerson: realPerson);

        public void Update()
        {
            link1To2.Update();
            link2To1.Update();

            // TODO(color): turn activeColor and inactiveColor into abstract properties
            inactiveColor = Color.Lerp
            (
                value1: Color.White,
                value2: Color.Green,
                amount: CurWorldManager.Overlay switch
                {
                    IPeopleOverlay => (float)(TravelTime / CurWorldManager.MaxLinkTravelTime),
                    _ => (float)(JoulesPerKg / CurWorldManager.MaxLinkJoulesPerKg)
                }
            );

            if (CurWorldManager.Overlay is IPowerOverlay)
                return;

            // TODO: It may be more appropriate for link1To2.GetTravellingAmount() to return a dictionary from Overlay cases to amounts
            // in order to not have two switch statements mirroring each other
            ulong travellingAmount = link1To2.GetTravellingAmount() + link2To1.GetTravellingAmount();

            CurWorldManager.Overlay.SwitchStatement
            (
                singleResCase: resInd => resTextBoxes[resInd].Text = $"{travellingAmount} of {CurWorldManager.Overlay} is travelling",
                allResCase: () => allResTextBox.Text = $"{travellingAmount} kg of resources are travelling",
                peopleCase: () => peopleTextBox.Text = $"{travellingAmount} of people are travelling",
                powerCase: () => throw new ArgumentOutOfRangeException()
            );
        }

        public void UpdatePeople()
        {
            link1To2.UpdatePeople();
            link2To1.UpdatePeople();
        }

        protected override void DrawChildren()
        {
            base.DrawChildren();

            link1To2.DrawTravelingRes();
            link2To1.DrawTravelingRes();
        }

        // this is commented out, otherwise the object construction fails as
        // tries to put object into HashSet before assigning node1 and node2

        //public override int GetHashCode()
        //    => node1.GetHashCode() ^ node2.GetHashCode();

        //public static bool operator ==(Link link1, Link link2)
        //    => (link1.node1 == link2.node1 && link1.node2 == link2.node2) ||
        //    (link1.node1 == link2.node2 && link1.node2 == link2.node1);

        //public static bool operator !=(Link link1, Link link2)
        //    => !(link1 == link2);

        //public override bool Equals(object obj)
        //{
        //    if (obj is Link other)
        //        return this == other;

        //    return false;
        //}
    }
}
