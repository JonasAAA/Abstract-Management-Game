using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Game1
{
    public class Node : IUIElement
    {
        public Vector2 Position
            => state.position;
        public readonly float radius;

        private readonly NodeState state;
        private readonly Image image;
        private readonly List<Link> links;
        // resToLinksSplitters[i][j] - resource i to link j (remain in the node if j is links.Count)
        private readonly MyArray<ProporSplitter> resToLinksSplitters;
        private Industry industry;

        public Node(NodeState state, Image image)
        {
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

        public void AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            foreach (var resToLinksSplitter in resToLinksSplitters)
            {
                resToLinksSplitter.InsertVar(index: links.Count);
                // temporary
                double[] proportions = new double[resToLinksSplitter.Count];
                Array.Fill(array: proportions, value: 1);
                proportions[^1] = 0;
                resToLinksSplitter.Proportions = new(proportions);
            }
            links.Add(link);
        }

        public bool Contains(Vector2 position)
            => Vector2.Distance(this.Position, position) <= radius;

        public void AddRes(ConstUIntArray resAmounts)
            => state.arrived += resAmounts;

        public void ActiveUpdate()
        {
            //// temporary
            //if (Keyboard.GetState().IsKeyDown(Keys.F))
            //{
            //    Factory.Params parameters = new
            //    (
            //        name: "factory",
            //        upgrades: new(),
            //        supply: new()
            //        {
            //            [0] = 10,
            //        },
            //        demand: new(),
            //        prodTime: TimeSpan.FromSeconds(value: 2)
            //    );

            //    industry = parameters.MakeIndustry(state: state);
            //}

            industry.ActiveUpdate();
        }

        public void Update()
        {
            industry = industry.FinishProduction();

            //// temporary
            //state.arrived += new UIntArray(value: 1);

            //maybe I should just send one resource at a time rather then pack them to IntArray
            UIntArray[] resSplitAmounts = new UIntArray[links.Count + 1];
            for (int i = 0; i < resSplitAmounts.Length; i++)
                resSplitAmounts[i] = new();

            for (int j = 0; j < ConstArray.length; j++)
            {
                if (!resToLinksSplitters[j].CanSplit(amount: state.arrived[j]))
                    throw new NotImplementedException();
                uint[] split = resToLinksSplitters[j].Split(amount: state.arrived[j]);
                Debug.Assert(split.Length == resSplitAmounts.Length);
                for (int i = 0; i < resSplitAmounts.Length; i++)
                    resSplitAmounts[i][j] = split[i];
            }

            for (int i = 0; i < links.Count; i++)
                links[i].AddRes(start: this, resAmounts: resSplitAmounts[i]);

            state.stored += resSplitAmounts[^1];
            state.arrived = new();

            industry.StartProductionIfCan();
        }

        public void Draw(bool active)
        {
            if (active)
                image.Color = Color.Yellow;
            else
                image.Color = Color.White;
            image.Draw(Position);
            industry.Draw();
        }
    }
}
