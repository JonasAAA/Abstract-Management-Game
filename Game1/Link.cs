using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class Link : IUIElement
    {
        private class DirLink
        {
            public readonly Node begin, end;

            private readonly TimedResQueue travel;
            private readonly TimeSpan minSafeTime;
            private TimeSpan minNextStartTime;
            private ConstUIntArray waitingResAmounts;
            private readonly Texture2D diskTexture;

            public DirLink(Node begin, Node end, TimeSpan travelTime, double minSafeDist)
            {
                this.begin = begin;
                this.end = end;

                travel = new(duration: travelTime);
                if (minSafeDist < 0 || minSafeDist > Vector2.Distance(begin.Position, end.Position))
                    throw new ArgumentOutOfRangeException();
                minSafeTime = travelTime * (minSafeDist / Vector2.Distance(begin.Position, end.Position));
                minNextStartTime = TimeSpan.MinValue;
                waitingResAmounts = new();

                diskTexture = C.Content.Load<Texture2D>("big disk");
            }

            public void AddRes(ConstUIntArray resAmounts)
            {
                waitingResAmounts += resAmounts;

                if (!waitingResAmounts.IsEmpty && minNextStartTime <= C.TotalGameTime)
                {
                    travel.Enqueue(newResAmounts: waitingResAmounts);
                    minNextStartTime = C.TotalGameTime + minSafeTime;
                    waitingResAmounts = new();
                }
            }

            public void Update()
                => end.AddRes(resAmounts: travel.DoneResAmounts());

            public void DrawTravelingRes()
            {
                // temporary
                //for (int i = 0; i < ConstArray.length; i++)
                int i = 0;
                {
                    travel.GetData
                    (
                        resInd: i,
                        completionProps: out List<double> completionProps,
                        resAmounts: out List<uint> resAmounts
                    );

                    foreach (var (completionProp, resAmount) in completionProps.Zip(resAmounts))
                    {
                        // temporary
                        Vector2 travelDir = end.Position - begin.Position;
                        travelDir.Normalize();
                        Vector2 orthToTravelDir = new(travelDir.Y, -travelDir.X);
                        C.SpriteBatch.Draw
                        (
                            texture: diskTexture,
                            position: begin.Position + (float)completionProp * (end.Position - begin.Position) + orthToTravelDir * i * 10,
                            sourceRectangle: null,
                            color: C.ResColors[i],
                            rotation: 0,
                            origin: new(diskTexture.Width * .5f, diskTexture.Height * .5f),
                            scale: (float)Math.Sqrt(resAmount) * 2 / diskTexture.Width,
                            effects: SpriteEffects.None,
                            layerDepth: 0
                        );
                    }
                }
            }
        }

        public readonly Node node1, node2;
        private readonly DirLink link1To2, link2To1;

        public Link(Node node1, Node node2, TimeSpan travelTime, double minSafeDist)
        {
            if (node1 == node2)
                throw new ArgumentException();

            this.node1 = node1;
            this.node2 = node2;
            
            link1To2 = new(begin: node1, end: node2, travelTime: travelTime, minSafeDist: minSafeDist);
            link2To1 = new(begin: node2, end: node1, travelTime: travelTime, minSafeDist: minSafeDist);
        }

        public Node Other(Node node)
        {
            if (!Contains(node))
                throw new ArgumentException();
            return node == node1 ? node2 : node1;
        }

        public bool Contains(Node node)
            => node == node1 || node == node2;

        public bool Contains(Vector2 position)
            => false;

        public void AddRes(Node start, ConstUIntArray resAmounts)
        {
            if (start == node1)
            {
                link1To2.AddRes(resAmounts: resAmounts);
                return;
            }
            if (start == node2)
            {
                link2To1.AddRes(resAmounts: resAmounts);
                return;
            }
            throw new ArgumentException();
        }

        public void ActiveUpdate()
        { }

        public void Update()
        {
            link1To2.Update();
            link2To1.Update();
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
