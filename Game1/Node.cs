using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class Node : IUIElement
    {
        public readonly Vector2 position;
        public readonly float radius;

        private readonly NodeState state;
        private readonly Image image;
        private readonly List<Link> links;
        private readonly MyArray<double> stayProps;
        private readonly MyArray<List<double>> linkProps;
        private Industry industry;

        public Node(Vector2 position, NodeState state, Image image)
        {
            this.position = position;
            this.state = state;
            radius = image.Width * .5f;
            this.image = image;
            links = new();
            stayProps = new(value: 1);
            linkProps = new();
            industry = Industry.emptyParams.MakeIndustry(state); 
        }

        public void AddEdge(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            links.Add(link);
            foreach (var linkProp in linkProps)
                linkProp.Add(0);
        }

        public bool Contains(Vector2 position)
            => Vector2.Distance(this.position, position) <= radius;

        public void AddRes(IntArray resAmounts)
            => state.arrived += resAmounts;

        public void ActiveUpdate()
        { }

        public void Update()
        {
            industry.FinishProduction();

            foreach (var linkProp in linkProps)
            {

            }

            industry.StartProduction();
        }

        public void Draw(bool active)
        {
            if (active)
                image.Color = Color.Yellow;
            else
                image.Color = Color.White;
            image.Draw(position);
        }
    }
}
