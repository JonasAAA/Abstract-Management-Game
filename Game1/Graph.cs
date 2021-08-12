using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public static class Graph
    {
        private static class JobMatching
        {
            private static readonly double enjoymentCoeff, talentCoeff, skillCoeff, vacancyDurationCoeff, distCoeff, minAcceptableScore;

            static JobMatching()
            {
                enjoymentCoeff = .2;
                talentCoeff = .2;
                skillCoeff = .2;
                distCoeff = .2;
                vacancyDurationCoeff = .2;
                minAcceptableScore = .5;
            }

            // later should take as parameters list of employed people looking for job and list of filled jobs looking to change employee
            public static void Match()
            {
                Job[] vacantJobs =
                    (from node in nodes
                    where node.Job is not null && !node.Job.IsFull
                    select node.Job).ToArray();
                Person[] unemployedPeople =
                    (from node in nodes
                    from person in node.UnemployedPeople
                    select person).ToArray();

                // prioritizes pairs with high score
                SimplePriorityQueue<(Job job, Person person), double> pairs = new((x, y) => y.CompareTo(x));
                foreach (var job in vacantJobs)
                    foreach (var person in unemployedPeople)
                    {
                        double score = Score(job: job, person: person);
                        Debug.Assert(C.IsSuitable(value: score));
                        if (score >= minAcceptableScore)
                            pairs.Enqueue(item: (job, person), priority: score);
                    }

                while (pairs.Count > 0)
                {
                    (Job job, Person person) = pairs.Dequeue();
                    person.TakeJob(job: job);
                    foreach (var otherJob in vacantJobs)
                        pairs.TryRemove(item: (otherJob, person));

                    job.Hire(person: person);
                    foreach (var otherPerson in unemployedPeople)
                    {
                        if (otherPerson == person)
                            continue;
                        pairs.TryRemove(item: (job, otherPerson));
                        double score = Score(job: job, person: otherPerson);
                        Debug.Assert(C.IsSuitable(value: score));
                        if (!job.IsFull && score >= minAcceptableScore)
                            pairs.Enqueue(item: (job, otherPerson), priority: score);
                    }
                }
            }

            // each parameter must be between 0 and 1 or double.NegativeInfinity
            // larger means this pair is more likely to work
            // must be between 0 and 1 or double.NegativeInfinity
            private static double Score(Job job, Person person)
                => enjoymentCoeff * person.EvaluateJob(job: job)
                + talentCoeff * person.talents[job.industryType]
                + skillCoeff * person.skills[job.industryType]
                + vacancyDurationCoeff * VacancyDuration(startTime: job.offerStartTime)
                + distCoeff * Distance(node1: job.node, node2: person.Node);

            // must be between 0 and 1
            private static double VacancyDuration(TimeSpan startTime)
            {
                if (startTime > C.TotalGameTime)
                    throw new ArgumentOutOfRangeException();
                return 1 - Math.Tanh((C.TotalGameTime - startTime).TotalSeconds);
            }

            // must be between 0 and 1 or double.NegativeInfinity
            // should later be changed to graph distance (either time or electricity cost)
            private static double Distance(Node node1, Node node2)
                => 1 - Math.Tanh(Vector2.Distance(node1.Position, node2.Position) / 100);
        }

        private static readonly List<Node> nodes;
        private static readonly List<Link> links;
        private static readonly HashSet<Node> nodeSet;
        private static readonly HashSet<Link> linkSet;
        private static readonly ulong ambientWattsPerSec = 100;
        private static IUIElement activeElement;
        private static ulong reqWattsPerSec, prodWattsPerSec;

        static Graph()
        {
            nodes = new();
            links = new();
            nodeSet = new();
            linkSet = new();
            activeElement = null;
            reqWattsPerSec = 0;
            prodWattsPerSec = ambientWattsPerSec;
        }

        public static void AddNode(Node node)
        {
            if (nodeSet.Contains(node))
                throw new ArgumentException();
            nodeSet.Add(node);
            nodes.Add(node);
        }

        public static void AddEdge(Link link)
        {
            if (linkSet.Contains(link))
                throw new ArgumentException();
            linkSet.Add(link);
            links.Add(link);

            link.node1.AddLink(link: link);
            link.node2.AddLink(link: link);
        }

        public static void Update(GameTime gameTime)
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

        public static void Draw()
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
