using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class Link : UIElement
    {
        private class DirLink : IElectrConsumer
        {
            /// <summary>
            /// CURRENTLY UNUSED
            /// </summary>
            public event Action Deleted;

            public readonly Node begin, end;
            public double WattsPerKg
                => timedPacketQueue.duration.TotalSeconds * reqWattsPerKgPerSec;
            public TimeSpan TravelTime
                => timedPacketQueue.duration;

            private readonly TimedPacketQueue timedPacketQueue;
            private readonly double minSafePropor;
            private ResAmountsPacketsByDestin waitingResAmountsPackets;
            private readonly MyHashSet<Person> waitingPeople;
            private readonly double reqWattsPerKgPerSec;
            private readonly Texture2D diskTexture;
            private double electrPropor;

            public DirLink(Node begin, Node end, TimeSpan travelTime, double wattsPerKg, double minSafeDist)
            {
                this.begin = begin;
                this.end = end;

                timedPacketQueue = new(duration: travelTime);
                minSafePropor = minSafeDist / Vector2.Distance(begin.Position, end.Position);
                if (!C.IsInSuitableRange(value: minSafePropor))
                    throw new ArgumentOutOfRangeException();
                waitingResAmountsPackets = new();
                waitingPeople = new();
                if (wattsPerKg <= 0)
                    throw new ArgumentOutOfRangeException();
                reqWattsPerKgPerSec = wattsPerKg / travelTime.TotalSeconds;
                diskTexture = C.Content.Load<Texture2D>("big disk");
                electrPropor = 0;

                ElectricityDistributor.AddElectrConsumer(electrConsumer: this);
            }

            public void Add(ResAmountsPacket resAmountsPacket)
                => waitingResAmountsPackets.Add(resAmountsPacket: resAmountsPacket);

            public void Add(IEnumerable<Person> people)
                => waitingPeople.UnionWith(people);

            public void Add(Person person)
                => waitingPeople.Add(person);

            public void Update()
            {
                timedPacketQueue.Update(workingPropor: electrPropor);
                if ((!waitingResAmountsPackets.Empty || waitingPeople.Count > 0)
                    && (timedPacketQueue.Count is 0 || timedPacketQueue.LastCompletionProp() >= minSafePropor))
                {
                    timedPacketQueue.Enqueue(resAmountsPackets: waitingResAmountsPackets, people: waitingPeople);
                    waitingResAmountsPackets = new();
                    waitingPeople.Clear();
                }
                var (resAmountsPackets, people) = timedPacketQueue.DonePacketsAndPeople();
                end.Arrive(resAmountsPackets: resAmountsPackets);
                end.Arrive(people: people);
            }

            public void UpdatePeople()
            {
                foreach (var person in waitingPeople.Concat(timedPacketQueue.People))
                    person.Update(closestNodePos: end.Position);
            }

            public void DrawTravelingRes()
            {
                // temporary
                void DrawDisk(double complProp, double size)
                    => C.Draw
                    (
                        texture: diskTexture,
                        position: begin.Position + (float)complProp * (end.Position - begin.Position),
                        color: Color.Black,
                        rotation: 0,
                        origin: new Vector2(diskTexture.Width * .5f, diskTexture.Height * .5f),
                        scale: (float)Math.Sqrt(size) * 2 / diskTexture.Width
                    );

                switch (Graph.Overlay)
                {
                    case <= C.MaxRes:
                        foreach (var (complProp, (resAmountsPackets, _)) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmountsPackets.ResAmounts[(int)Graph.Overlay]);
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
            }

            double IElectrConsumer.ReqWattsPerSec()
                => timedPacketQueue.TotalWeight * reqWattsPerKgPerSec;

            ulong IElectrConsumer.ElectrPriority
                => electrPriority;

            void IElectrConsumer.ConsumeElectr(double electrPropor)
                => this.electrPropor = electrPropor;
        }

        private static readonly ulong electrPriority;

        static Link()
            => electrPriority = 10;

        public readonly Node node1, node2;
        public double WattsPerKg
            => link1To2.WattsPerKg;
        public TimeSpan TravelTime
            => link1To2.TravelTime;

        private readonly DirLink link1To2, link2To1;

        public Link(Node node1, Node node2, TimeSpan travelTime, double wattsPerKg, double minSafeDist)
            : base(shape: new EmptyShape())
        {
            if (node1 == node2)
                throw new ArgumentException();

            this.node1 = node1;
            this.node2 = node2;
            
            link1To2 = new(begin: node1, end: node2, travelTime: travelTime, wattsPerKg: wattsPerKg, minSafeDist: minSafeDist);
            link2To1 = new(begin: node2, end: node1, travelTime: travelTime, wattsPerKg: wattsPerKg, minSafeDist: minSafeDist);
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
        }

        public void UpdatePeople()
        {
            link1To2.UpdatePeople();
            link2To1.UpdatePeople();
        }

        public override void Draw()
        {
            // temporary
            Texture2D pixel = C.Content.Load<Texture2D>(assetName: "pixel");
            Color color = Color.Lerp
            (
                value1: Color.White,
                value2: Color.Green,
                amount: Graph.Overlay switch
                {
                    Overlay.People => (float)(TravelTime / Graph.World.MaxLinkTravelTime),
                    _ => (float)(WattsPerKg / Graph.World.MaxLinkWattsPerKg)
                }
            );
            C.Draw
            (
                texture: pixel,
                position: (node1.Position + node2.Position) / 2,
                color: color,
                rotation: C.Rotation(vector: node1.Position - node2.Position),
                origin: new Vector2(.5f, .5f),
                scale: new Vector2(Vector2.Distance(node1.Position, node2.Position), 10)
            );

            link1To2.DrawTravelingRes();
            link2To1.DrawTravelingRes();

            base.Draw();
        }

        public override int GetHashCode()
            => node1.GetHashCode() ^ node2.GetHashCode();

        public static bool operator ==(Link link1, Link link2)
            => (link1.node1 == link2.node1 && link1.node2 == link2.node2) ||
            (link1.node1 == link2.node2 && link1.node2 == link2.node1);

        public static bool operator !=(Link link1, Link link2)
            => !(link1 == link2);

        public override bool Equals(object obj)
        {
            if (obj is Link other)
                return this == other;

            return false;
        }
    }
}
