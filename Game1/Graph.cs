using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class Graph
    {
        private readonly List<Node> nodes;
        private readonly List<Link> links;
        private readonly HashSet<Node> nodeSet;
        private readonly HashSet<Link> linkSet;
        private readonly ulong ambientWattsPerSec;
        private IUIElement activeElement;
        private ulong reqWattsPerSec, prodWattsPerSec;

        public Graph(ulong ambientWattsPerSec)
        {
            nodes = new();
            links = new();
            nodeSet = new();
            linkSet = new();
            this.ambientWattsPerSec = ambientWattsPerSec;
            activeElement = null;
            reqWattsPerSec = 0;
            prodWattsPerSec = ambientWattsPerSec;
        }

        public void AddNode(Node node)
        {
            if (nodeSet.Contains(node))
                throw new ArgumentException();
            nodeSet.Add(node);
            nodes.Add(node);
        }

        public void AddEdge(Link link)
        {
            if (linkSet.Contains(link))
                throw new ArgumentException();
            linkSet.Add(link);
            links.Add(link);

            link.node1.AddLink(link: link);
            link.node2.AddLink(link: link);
        }

        public void Update(GameTime gameTime)
        {
            reqWattsPerSec = nodes.Sum(node => node.ReqWattsPerSec()) + links.Sum(link => link.ReqWattsPerSec());
            prodWattsPerSec = ambientWattsPerSec + nodes.Sum(node => node.ProdWattsPerSec());

            if (reqWattsPerSec > prodWattsPerSec)
                C.Update(elapsed: gameTime.ElapsedGameTime * prodWattsPerSec / reqWattsPerSec);
            else
                C.Update(elapsed: gameTime.ElapsedGameTime);



            links.ForEach(link => link.StartUpdate());

            nodes.ForEach(node => node.StartUpdate());

            links.ForEach(link => link.EndUpdate());

            nodes.ForEach(node => node.EndUpdate());

            if (MyMouse.RightClick)
            {
                activeElement = null;
                return;
            }
            if (MyMouse.LeftClick)
            {
                activeElement = null;
                foreach (var element in nodes.Cast<IUIElement>().Concat(links))
                    if (element.Contains(position: MyMouse.Position))
                    {
                        activeElement = element;
                        break;
                    }
            }

            if (activeElement is not null)
                activeElement.ActiveUpdate();
        }

        public void Draw()
        {
            foreach (var link in links)
                link.Draw(active: ReferenceEquals(link, activeElement));

            foreach (var node in nodes)
                node.Draw(active: node == activeElement);

            C.SpriteBatch.DrawString
            (
                spriteFont: C.Content.Load<SpriteFont>("font"),
                text: $"required: {reqWattsPerSec}\nproduced: {prodWattsPerSec}",
                position: new Vector2(-500, -500),
                color: Color.Black,
                rotation: 0,
                origin: Vector2.Zero,
                scale: .15f,
                effects: SpriteEffects.None,
                layerDepth: 0
            );
        }
    }
}
