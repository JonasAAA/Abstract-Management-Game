﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1
{
    public class Link : IUIElement
    {
        private class DirLink
        {
            public readonly Node begin, end;
            public double WattsPerKg
                => travel.duration.TotalSeconds * reqWattsPerKgPerSec;
            public TimeSpan TravelTime
                => travel.duration;

            private readonly TimedTravelPacketQueue travel;
            private readonly TimeSpan minSafeTime;
            private TimeSpan minNextStartTime;
            private TravelPacket waitingTravelPacket, curWaitingTravelPacket;
            private readonly double reqWattsPerKgPerSec;
            private readonly Texture2D diskTexture;

            public DirLink(Node begin, Node end, TimeSpan travelTime, double minSafeDist, double reqWattsPerKgPerSec)
            {
                this.begin = begin;
                this.end = end;

                travel = new(duration: travelTime);
                if (minSafeDist < 0 || minSafeDist > Vector2.Distance(begin.Position, end.Position))
                    throw new ArgumentOutOfRangeException();
                minSafeTime = travelTime * (minSafeDist / Vector2.Distance(begin.Position, end.Position));
                minNextStartTime = TimeSpan.MinValue;
                waitingTravelPacket = new();
                curWaitingTravelPacket = new();
                if (reqWattsPerKgPerSec <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerKgPerSec = reqWattsPerKgPerSec;
                diskTexture = C.Content.Load<Texture2D>("big disk");
            }

            public double ReqWattsPerSec()
                => travel.TotalWeight * reqWattsPerKgPerSec;

            public void AddTravelPacket(TravelPacket travelPacket)
                => waitingTravelPacket.Add(travelPacket: travelPacket);

            public void StartUpdate()
            {
                if (!curWaitingTravelPacket.Empty && minNextStartTime <= C.TotalGameTime)
                {
                    travel.Enqueue(travelPacket: curWaitingTravelPacket);
                    minNextStartTime = C.TotalGameTime + minSafeTime;
                    curWaitingTravelPacket = new();
                }
                end.AddTravelPacket(travelPacket: travel.DoneTravelPacket());
            }

            public void EndUpdate()
            {
                curWaitingTravelPacket.Add(travelPacket: waitingTravelPacket);
                waitingTravelPacket = new();
            }

            public void DrawTravelingRes()
            {
                foreach (var (complProp, resAmounts, numPeople) in travel.GetData())
                {
                    // temporary
                    Vector2 travelDir = end.Position - begin.Position;
                    travelDir.Normalize();
                    Vector2 orthToTravelDir = new(travelDir.Y, -travelDir.X);
                    C.SpriteBatch.Draw
                    (
                        texture: diskTexture,
                        position: begin.Position + (float)complProp * (end.Position - begin.Position) + orthToTravelDir * -10,
                        sourceRectangle: null,
                        color: Color.Black,
                        rotation: 0,
                        origin: new Vector2(diskTexture.Width * .5f, diskTexture.Height * .5f),
                        scale: (float)Math.Sqrt(numPeople) * 2 / diskTexture.Width,
                        effects: SpriteEffects.None,
                        layerDepth: 0
                    );
                    for (int i = 0; i < Resource.Count; i++)
                        C.SpriteBatch.Draw
                        (
                            texture: diskTexture,
                            position: begin.Position + (float)complProp * (end.Position - begin.Position) + orthToTravelDir * i * 10,
                            sourceRectangle: null,
                            color: C.ResColors[i],
                            rotation: 0,
                            origin: new Vector2(diskTexture.Width * .5f, diskTexture.Height * .5f),
                            scale: (float)Math.Sqrt(resAmounts[i]) * 2 / diskTexture.Width,
                            effects: SpriteEffects.None,
                            layerDepth: 0
                        );
                }
            }
        }

        public readonly Node node1, node2;
        public double WattsPerKg
            => link1To2.WattsPerKg;

        public TimeSpan TravelTime
            => link1To2.TravelTime;

        private readonly DirLink link1To2, link2To1;

        public Link(Node node1, Node node2, TimeSpan travelTime, double minSafeDist, double reqWattsPerKgPerSec)
        {
            if (node1 == node2)
                throw new ArgumentException();

            this.node1 = node1;
            this.node2 = node2;
            
            link1To2 = new(begin: node1, end: node2, travelTime: travelTime, minSafeDist: minSafeDist, reqWattsPerKgPerSec: reqWattsPerKgPerSec);
            link2To1 = new(begin: node2, end: node1, travelTime: travelTime, minSafeDist: minSafeDist, reqWattsPerKgPerSec: reqWattsPerKgPerSec);
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

        public void AddTravelPacket(Node start, TravelPacket travelPacket)
        {
            if (start == node1)
            {
                link1To2.AddTravelPacket(travelPacket: travelPacket);
                return;
            }
            if (start == node2)
            {
                link2To1.AddTravelPacket(travelPacket: travelPacket);
                return;
            }
            throw new ArgumentException();
        }

        public double ReqWattsPerSec()
            => link1To2.ReqWattsPerSec() + link2To1.ReqWattsPerSec();

        public void ActiveUpdate()
        { }

        public void StartUpdate()
        {
            link1To2.StartUpdate();
            link2To1.StartUpdate();
        }

        public void EndUpdate()
        {
            link1To2.EndUpdate();
            link2To1.EndUpdate();
        }

        public void Draw(bool active)
        {
            Texture2D pixel = C.Content.Load<Texture2D>(assetName: "pixel");
            // temporary
            C.SpriteBatch.Draw
            (
                texture: pixel,
                position: (node1.Position + node2.Position) / 2,
                sourceRectangle: null,
                color: active ? Color.Yellow : Color.Green,
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
