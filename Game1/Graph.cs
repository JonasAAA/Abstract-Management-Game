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
        private IUIElement activeElement;

        public Graph()
        {
            nodes = new();
            links = new();
            nodeSet = new();
            linkSet = new();
            activeElement = null;
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
        }

        public void Update()
        {
            foreach (var link in links)
                link.Update();

            foreach (var node in nodes)
                node.Update();

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
        }
    }
}
