using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class Link : IUIElement
    {
        private class DirLink : IElectrConsumer
        {
            public readonly Node begin, end;
            public double WattsPerKg
                => timedPacketQueue.duration.TotalSeconds * reqWattsPerKgPerSec;
            public TimeSpan TravelTime
                => timedPacketQueue.duration;

            private readonly TimedPacketQueue timedPacketQueue;
            private readonly double minSafePropor;
            private ResAmountsPacketsByDestin waitingResAmountsPackets;
            private List<Person> waitingPeople;
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

            public double ReqWattsPerSec()
                => timedPacketQueue.TotalWeight * reqWattsPerKgPerSec;

            public void Add(ResAmountsPacket resAmountsPacket)
                => waitingResAmountsPackets.Add(resAmountsPacket: resAmountsPacket);

            public void Add(IEnumerable<Person> people)
                => waitingPeople.AddRange(people);

            public void Add(Person person)
                => waitingPeople.Add(person);

            public void Update(TimeSpan elapsed)
            {
                timedPacketQueue.Update(elapsed: elapsed, electrPropor: electrPropor);
                if ((!waitingResAmountsPackets.Empty || waitingPeople.Count > 0)
                    && (timedPacketQueue.Empty || timedPacketQueue.LastCompletionProp() >= minSafePropor))
                {
                    timedPacketQueue.Enqueue(resAmountsPackets: waitingResAmountsPackets, people: waitingPeople);
                    waitingResAmountsPackets = new();
                    waitingPeople = new();
                }
                var (resAmountsPackets, people) = timedPacketQueue.DonePackets();
                end.Arrive(resAmountsPackets: resAmountsPackets);
                end.Arrive(people: people);
            }

            public void DrawTravelingRes()
            {
                // temporary
                void DrawDisk(double complProp, double size)
                    => C.SpriteBatch.Draw
                    (
                        texture: diskTexture,
                        position: begin.Position + (float)complProp * (end.Position - begin.Position),
                        sourceRectangle: null,
                        color: Color.Black,
                        rotation: 0,
                        origin: new Vector2(diskTexture.Width * .5f, diskTexture.Height * .5f),
                        scale: (float)Math.Sqrt(size) * 2 / diskTexture.Width,
                        effects: SpriteEffects.None,
                        layerDepth: 0
                    );

                switch (Graph.Overlay)
                {
                    case <= C.MaxRes:
                        foreach (var (complProp, resAmounts, _) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmounts[(int)Graph.Overlay]);
                        break;
                    case Overlay.AllRes:
                        foreach (var (complProp, resAmounts, _) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: resAmounts.TotalWeight());
                        break;
                    case Overlay.People:
                        foreach (var (complProp, _, numPeople) in timedPacketQueue.GetData())
                            DrawDisk(complProp: complProp, size: numPeople);
                        break;
                }
            }

            public ulong ElectrPriority
                => electrPriority;

            public void ConsumeElectr(double electrPropor)
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

        public bool Contains(Vector2 position)
            => false;

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

        public double ReqWattsPerSec()
            => link1To2.ReqWattsPerSec() + link2To1.ReqWattsPerSec();

        public void ActiveUpdate()
        { }

        public void Update(TimeSpan elapsed)
        {
            link1To2.Update(elapsed: elapsed);
            link2To1.Update(elapsed: elapsed);
        }

        public void Draw(bool active)
        {
            // temporary
            Texture2D pixel = C.Content.Load<Texture2D>(assetName: "pixel");
            Color color = Color.Lerp
            (
                value1: Color.White,
                value2: Color.Green,
                amount: Graph.Overlay switch
                {
                    Overlay.People => (float)(TravelTime / Graph.MaxLinkTravelTime),
                    _ => (float)(WattsPerKg / Graph.MaxLinkWattsPerKg)
                }
            );
            C.SpriteBatch.Draw
            (
                texture: pixel,
                position: (node1.Position + node2.Position) / 2,
                sourceRectangle: null,
                color: color,
                rotation: C.Rotation(vector: node1.Position - node2.Position),
                origin: new Vector2(.5f, .5f),
                scale: new Vector2(Vector2.Distance(node1.Position, node2.Position), 10),
                effects: SpriteEffects.None,
                layerDepth: 0
            );

            link1To2.DrawTravelingRes();
            link2To1.DrawTravelingRes();
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
