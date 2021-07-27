using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class Node : IUIElement
    {
        public readonly Vector2 position;
        public readonly float radius;

        private readonly NodeState state;
        private readonly Image image;
        private readonly List<Link> links;
        // resToLinksSplitters[i][j] - resource i to link j (remain in the node if j is links.Count)
        private readonly MyArray<ProporSplitter> resToLinksSplitters;
        private Industry industry;

        public Node(Vector2 position, NodeState state, Image image)
        {
            state.arrived[0] += 1;
            this.position = position;
            this.state = state;
            radius = image.Width * .5f;
            this.image = image;
            links = new();
            resToLinksSplitters = new();
            for (int i = 0; i < ConstArray.length; i++)
            {
                resToLinksSplitters[i] = new ProporSplitter();
                resToLinksSplitters[i].InsertVar(index: 0);
            }
            industry = Industry.emptyParams.MakeIndustry(state); 
        }

        public void AddEdge(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            foreach (var resToLinksSplitter in resToLinksSplitters)
                resToLinksSplitter.InsertVar(index: links.Count);
            links.Add(link);
        }

        public bool Contains(Vector2 position)
            => Vector2.Distance(this.position, position) <= radius;

        public void AddRes(ConstIntArray resAmounts)
        {
            if (resAmounts.Any(a => a < 0))
                throw new ArgumentException();
            state.arrived += resAmounts;
        }

        public void ActiveUpdate()
        { }

        public void Update()
        {
            if (state.arrived.Any(a => a < 0))
                throw new Exception();

            industry.FinishProduction();
            
            //maybe I should just send one resource at a time rather then pack them to IntArray
            IntArray[] resSplitAmounts = new IntArray[links.Count + 1];
            for (int i = 0; i < resSplitAmounts.Length; i++)
                resSplitAmounts[i] = new();

            for (int j = 0; j < ConstArray.length; j++)
            {
                if (!resToLinksSplitters[j].CanSplit(amount: state.arrived[j]))
                    throw new NotImplementedException();
                int[] split = resToLinksSplitters[j].Split(amount: state.arrived[j]);
                Debug.Assert(split.Length == resSplitAmounts.Length);
                for (int i = 0; i < resSplitAmounts.Length; i++)
                    resSplitAmounts[i][j] = split[i];
            }

            for (int i = 0; i < links.Count; i++)
                links[i].AddRes(start: this, resAmounts: resSplitAmounts[i]);

            state.stored += resSplitAmounts[^1];

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
