using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1
{
    public class Link : IUIElement
    {
        private class DirLink
        {
            public readonly Node begin, end;

            private readonly TimedResQueue travel;

            public DirLink(Node begin, Node end, TimeSpan travelTime)
            {
                this.begin = begin;
                this.end = end;

                travel = new(duration: travelTime);
            }

            public void AddRes(IntArray resAmounts)
                => travel.Enqueue(newResAmounts: resAmounts);

            public void Update()
                => end.AddRes(resAmounts: travel.DoneResAmounts());
        }

        private readonly Node node1, node2;
        private readonly DirLink link1To2, link2To1;

        public Link(Node node1, Node node2, TimeSpan travelTime)
        {
            this.node1 = node1;
            this.node2 = node2;
            
            link1To2 = new(begin: node1, end: node2, travelTime: travelTime);
            link2To1 = new(begin: node2, end: node1, travelTime: travelTime);
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

        public void AddRes(Node start, IntArray resAmounts)
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
            C.SpriteBatch.Draw
            (
                texture: pixel,
                position: (node1.position + node2.position) / 2,
                sourceRectangle: null,
                color: active ? Color.Yellow : Color.Green,
                rotation: C.Rotation(vector: node1.position - node2.position),
                origin: new Vector2(.5f, .5f),
                scale: new Vector2(Vector2.Distance(node1.position, node2.position), 10),
                effects: SpriteEffects.None,
                layerDepth: 0
            );
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
