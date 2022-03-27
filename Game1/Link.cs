using Game1.Delegates;
using Game1.PrimitiveTypeWrappers;
using Game1.Shapes;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class Link : WorldUIElement
    {
        [Serializable]
        private class DirLink : IEnergyConsumer
        {
            private static readonly Texture2D diskTexture;

            static DirLink()
                => diskTexture = C.LoadTexture(name: "big disk");

            /// <summary>
            /// CURRENTLY UNUSED
            /// </summary>
            public IEvent<IDeletedListener> Deleted
                => deleted;

            public readonly Node startNode, endNode;
            public double JoulesPerKg
                => timedPacketQueue.duration.TotalSeconds * reqWattsPerKg;
            public TimeSpan TravelTime
                => timedPacketQueue.duration;

            private readonly TimedPacketQueue timedPacketQueue;
            private readonly double minSafePropor;
            private ResAmountsPacketsByDestin waitingResAmountsPackets;
            private readonly MySet<Person> waitingPeople;
            private readonly double reqWattsPerKg;
            private double energyPropor;
            private readonly Event<IDeletedListener> deleted;

            public DirLink(Node startNode, Node endNode, TimeSpan travelTime, double wattsPerKg, double minSafeDist)
            {
                this.startNode = startNode;
                this.endNode = endNode;

                timedPacketQueue = new(duration: travelTime);
                minSafePropor = minSafeDist / Vector2.Distance(startNode.Position, endNode.Position);
                if (!C.IsInSuitableRange(value: minSafePropor))
                    throw new ArgumentOutOfRangeException();
                waitingResAmountsPackets = new();
                waitingPeople = new();
                if (wattsPerKg <= 0)
                    throw new ArgumentOutOfRangeException();
                reqWattsPerKg = wattsPerKg / travelTime.TotalSeconds;
                energyPropor = 0;
                deleted = new();

                CurWorldManager.AddEnergyConsumer(energyConsumer: this);
            }

            public void Add(ResAmountsPacket resAmountsPacket)
                => waitingResAmountsPackets.Add(resAmountsPacket: resAmountsPacket);

            public void Add(IEnumerable<Person> people)
                => waitingPeople.UnionWith(people);

            public void Add(Person person)
                => waitingPeople.Add(person);

            public ulong GetTravellingAmount()
                => CurWorldManager.Overlay switch
                {
                    <= MaxRes => timedPacketQueue.TotalResAmounts[(int)CurWorldManager.Overlay],
                    Overlay.AllRes => timedPacketQueue.TotalResAmounts.TotalWeight(),
                    Overlay.People => (ulong)timedPacketQueue.PeopleCount,
                    _ => throw new InvalidOperationException()
                };

            public void Update()
            {
                timedPacketQueue.Update(workingPropor: energyPropor);
                if ((!waitingResAmountsPackets.Empty || waitingPeople.Count > 0)
                    && (timedPacketQueue.Count is 0 || timedPacketQueue.LastCompletionProp() >= minSafePropor))
                {
                    timedPacketQueue.Enqueue(resAmountsPackets: waitingResAmountsPackets, people: waitingPeople);
                    waitingResAmountsPackets = new();
                    waitingPeople.Clear();
                }
                var (resAmountsPackets, people) = timedPacketQueue.DonePacketsAndPeople();
                endNode.Arrive(resAmountsPackets: resAmountsPackets);
                endNode.Arrive(people: people);
            }

            public void UpdatePeople()
            {
                foreach (var person in waitingPeople.Concat(timedPacketQueue.People))
                    person.Update(prevNodePos: startNode.Position, closestNodePos: endNode.Position);
            }

            public void DrawTravelingRes()
            {
                // temporary
                switch (CurWorldManager.Overlay)
                {
                    case <= MaxRes:
                        foreach (var (complProp, (resAmountsPackets, _)) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmountsPackets.ResAmounts[(int)CurWorldManager.Overlay]);
                        break;
                    case Overlay.AllRes:
                        foreach (var (complProp, (resAmountsPackets, _)) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmountsPackets.ResAmounts.TotalWeight());
                        break;
                    case Overlay.People:
                        foreach (var (complProp, (_, people)) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: people.Count());
                        break;
                }

                void DrawDisk(double complProp, double size)
                    => C.Draw
                    (
                        texture: diskTexture,
                        position: startNode.Position + (float)complProp * (endNode.Position - startNode.Position),
                        color: Color.Black,
                        rotation: 0,
                        origin: new Vector2(diskTexture.Width * .5f, diskTexture.Height * .5f),
                        scale: (float)Math.Sqrt(size) * 2 / diskTexture.Width
                    );
            }

            ulong IEnergyConsumer.EnergyPriority
                => CurWorldConfig.linkEnergyPriority;

            Vector2 IEnergyConsumer.NodePos
                => startNode.Position;

            double IEnergyConsumer.ReqWatts()
                => timedPacketQueue.TotalWeight * reqWattsPerKg;

            void IEnergyConsumer.ConsumeEnergy(double energyPropor)
                => this.energyPropor = energyPropor;
        }

        public readonly Node node1, node2;
        public double JoulesPerKg
            => link1To2.JoulesPerKg;
        public TimeSpan TravelTime
            => link1To2.TravelTime;

        private readonly DirLink link1To2, link2To1;
        private readonly MyArray<TextBox> resTextBoxes;
        private readonly TextBox allResTextBox, peopleTextBox;

        public Link(Node node1, Node node2, TimeSpan travelTime, double wattsPerKg, double minSafeDist)
            : base
            (
                shape: new LineSegment
                (
                    startPos: node1.Position,
                    endPos: node2.Position,
                    width: new ChangingUFloat(CurWorldConfig.linkWidth)
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
            for (int resInd = 0; resInd <= (int)MaxRes; resInd++)
            {
                resTextBoxes[resInd] = new();
                resTextBoxes[resInd].Shape.Color = Color.White;
                SetPopup(HUDElement: resTextBoxes[resInd], overlay: (Overlay)resInd);
            }

            allResTextBox = new();
            allResTextBox.Shape.Color = Color.White;
            SetPopup(HUDElement: allResTextBox, overlay: Overlay.AllRes);

            peopleTextBox = new();
            peopleTextBox.Shape.Color = Color.White;
            SetPopup(HUDElement: peopleTextBox, overlay: Overlay.People);
        }

        public Node OtherNode(Node node)
        {
            if (!Contains(node))
                throw new ArgumentException();
            return node == node1 ? node2 : node1;
        }

        public bool Contains(Node node)
            => node == node1 || node == node2;

        private DirLink GetDirLink(Node start)
        {
            if (start == node1)
                return link1To2;
            if (start == node2)
                return link2To1;
            throw new ArgumentException();
        }

        public void Add(Node start, ResAmountsPacket resAmountsPacket)
            => GetDirLink(start: start).Add(resAmountsPacket: resAmountsPacket);

        public void Add(Node start, IEnumerable<Person> people)
            => GetDirLink(start: start).Add(people: people);

        public void Add(Node start, Person person)
            => GetDirLink(start: start).Add(person: person);

        public void Update()
        {
            link1To2.Update();
            link2To1.Update();

            InactiveColor = Color.Lerp
            (
                value1: Color.White,
                value2: Color.Green,
                amount: CurWorldManager.Overlay switch
                {
                    Overlay.People => (float)(TravelTime / CurWorldManager.MaxLinkTravelTime),
                    _ => (float)(JoulesPerKg / CurWorldManager.MaxLinkJoulesPerKg)
                }
            );

            if (CurWorldManager.Overlay is Overlay.Power)
                return;

            ulong travellingAmount = link1To2.GetTravellingAmount() + link2To1.GetTravellingAmount();

            switch (CurWorldManager.Overlay)
            {
                case <= MaxRes:
                    resTextBoxes[(int)CurWorldManager.Overlay].Text = $"{travellingAmount} of {CurWorldManager.Overlay} is travelling";
                    break;
                case Overlay.AllRes:
                    allResTextBox.Text = $"{travellingAmount} kg of resources are travelling";
                    break;
                case Overlay.People:
                    peopleTextBox.Text = $"{travellingAmount} of people are travelling";
                    break;
            }
        }

        public void UpdatePeople()
        {
            link1To2.UpdatePeople();
            link2To1.UpdatePeople();
        }

        public override void Draw()
        {
            base.Draw();

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
