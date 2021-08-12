using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
        // resToLinksSplitters[i][j] - resource i to link j
        private readonly MyArray<ProporSplitter> resToLinksSplitters;
        private Industry industry;
        private ULongArray curWaitingRes;

        public Node(NodeState state, Image image)
        {
            this.state = state;
            radius = image.Width * .5f;
            this.image = image;
            links = new();
            resToLinksSplitters = new();
            for (int i = 0; i < ConstArray.length; i++)
                resToLinksSplitters[i] = new ProporSplitter();
            industry = Industry.emptyParams.MakeIndustry(state);
            curWaitingRes = new();
        }

        public void AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            foreach (var resToLinksSplitter in resToLinksSplitters)
            {
                resToLinksSplitter.InsertVar(index: links.Count);
                resToLinksSplitter.Proportions = new(Enumerable.Repeat(element: 1.0, count: (int)resToLinksSplitter.Count).ToList());
            }
            links.Add(link);
        }

        public bool Contains(Vector2 position)
            => Vector2.Distance(this.Position, position) <= radius;

        public void AddRes(ConstULongArray resAmounts)
            => state.waitingRes += resAmounts;

        public ulong ReqWattsPerSec()
            => industry.ReqWattsPerSec();

        public ulong ProdWattsPerSec()
            => industry.ProdWattsPerSec();

        public void ActiveUpdate()
            => industry.ActiveUpdate();

        public void StartUpdate()
        {
            industry = industry.Update();
            //industry.StartProduction();
            //industry = industry.FinishProduction();

            curWaitingRes += state.storedRes;
            state.storedRes = new();

            state.storedRes = curWaitingRes.Min(industry.TargetStoredResAmounts());
            curWaitingRes -= state.storedRes;

            // maybe I should just send one resource at a time rather then pack them to ULongArray
            ULongArray[] resSplitAmounts = new ULongArray[links.Count];
            for (int i = 0; i < resSplitAmounts.Length; i++)
                resSplitAmounts[i] = new();

            for (int j = 0; j < ConstArray.length; j++)
            {
                if (!resToLinksSplitters[j].CanSplit(amount: curWaitingRes[j]))
                    throw new NotImplementedException();
                ulong[] split = resToLinksSplitters[j].Split(amount: curWaitingRes[j]);
                Debug.Assert(split.Length == resSplitAmounts.Length);
                for (int i = 0; i < resSplitAmounts.Length; i++)
                    resSplitAmounts[i][j] = split[i];
            }

            for (int i = 0; i < links.Count; i++)
                links[i].AddRes(start: this, resAmounts: resSplitAmounts[i]);

            curWaitingRes = new();
        }

        public void EndUpdate()
        {
            curWaitingRes += state.waitingRes;
            state.waitingRes = new();
        }

        public void Draw(bool active)
        {
            //Draw amount of resources in storage
            //or write percentage of required res
            if (active)
                image.Color = Color.Yellow;
            else
                image.Color = Color.White;
            image.Draw(Position);
            industry.Draw();
        }
    }
}
