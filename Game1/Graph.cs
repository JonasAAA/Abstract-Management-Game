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
        //// TODO:
        //// if place stays vacant, it should decrease it standards
        ////
        //// when could fire someone due to having more skill then needed, need to do so
        //private static class JobMatching
        //{
        //    private static readonly double enjoymentCoeff, talentCoeff, skillCoeff, /*vacancyDurationCoeff, */jobOpenSpaceCoeff, distCoeff, minAcceptableScore;

        //    static JobMatching()
        //    {
        //        enjoymentCoeff = .2;
        //        talentCoeff = .2;
        //        skillCoeff = .2;
        //        distCoeff = .2;
        //        //vacancyDurationCoeff = .1;
        //        jobOpenSpaceCoeff = .2;
        //        minAcceptableScore = .5;
        //    }

        //    private record PersonNode(Person Person, Node Node);
        //    private record JobNode(IJob Job, Node Node);

        //    // later should take as parameters list of employed people looking for job and list of filled jobs looking to change employee
        //    public static void Match()
        //    {
        //        HashSet<JobNode> vacantJobs =
        //            (from node in nodes
        //            where node.Job is not null && node.Job.OpenSpace() is not double.NegativeInfinity
        //            select new JobNode(Job: node.Job, Node: node)).ToHashSet();
        //        HashSet<PersonNode> unemployedPeople =
        //            (from node in nodes
        //            from person in node.UnemployedPeople
        //            select new PersonNode(Person: person, Node: node)).ToHashSet();

        //        // prioritizes pairs with high score
        //        SimplePriorityQueue<(JobNode jobNode, PersonNode personNode), double> pairings = new((x, y) => y.CompareTo(x));
        //        foreach (var jobNode in vacantJobs)
        //            foreach (var personNode in unemployedPeople)
        //            {
        //                double score = Score(jobNode: jobNode, personNode: personNode);
        //                Debug.Assert(C.IsSuitable(value: score));
        //                if (score >= minAcceptableScore)
        //                    pairings.Enqueue(item: (jobNode, personNode), priority: score);
        //            }

        //        while (pairings.Count > 0)
        //        {
        //            (JobNode jobNode, PersonNode personNode) = pairings.Dequeue();

        //            jobNode.Job.Hire(person: personNode.Person);
        //            personNode.Person.TakeJob(job: jobNode.Job, jobNode: jobNode.Node);

        //            if (jobNode.Job.OpenSpace() is double.NegativeInfinity)
        //                vacantJobs.Remove(jobNode);
        //            unemployedPeople.Remove(personNode);

        //            foreach (var otherPersonNode in unemployedPeople)
        //                pairings.TryRemove(item: (jobNode, otherPersonNode));
        //            foreach (var otherJobNode in vacantJobs)
        //                pairings.TryRemove(item: (otherJobNode, personNode));

        //            if (vacantJobs.Contains(jobNode))
        //            {
        //                foreach (var otherPersonNode in unemployedPeople)
        //                {
        //                    double score = Score(jobNode: jobNode, personNode: otherPersonNode);
        //                    Debug.Assert(C.IsSuitable(value: score));
        //                    if (score >= minAcceptableScore)
        //                        pairings.Enqueue(item: (jobNode, otherPersonNode), priority: score);
        //                }
        //            }
        //        }
        //    }

        //    // each parameter must be between 0 and 1 or double.NegativeInfinity
        //    // larger means this pair is more likely to work
        //    // must be between 0 and 1 or double.NegativeInfinity
        //    private static double Score(JobNode jobNode, PersonNode personNode)
        //        => enjoymentCoeff * personNode.Person.EvaluateJob(job: jobNode.Job)
        //        + talentCoeff * personNode.Person.talents[jobNode.Job.IndustryType]
        //        + skillCoeff * personNode.Person.skills[jobNode.Job.IndustryType]
        //        //+ vacancyDurationCoeff * VacancyDuration(startTime: jobNode.Job.SearchStart)
        //        + jobOpenSpaceCoeff * jobNode.Job.OpenSpace()
        //        + distCoeff * Distance(node1: jobNode.Node, node2: personNode.Node);

        //    //// must be between 0 and 1
        //    //private static double VacancyDuration(TimeSpan startTime)
        //    //{
        //    //    if (startTime > C.TotalGameTime)
        //    //        throw new ArgumentOutOfRangeException();
        //    //    return 1 - Math.Tanh((C.TotalGameTime - startTime).TotalSeconds);
        //    //}

        //    // must be between 0 and 1 or double.NegativeInfinity
        //    // should later be changed to graph distance (either time or electricity cost)
        //    private static double Distance(Node node1, Node node2)
        //        => 1 - Math.Tanh(Vector2.Distance(node1.Position, node2.Position) / 100);
        //}

        public static IEnumerable<Node> Nodes
            => nodes;
        public static IEnumerable<Link> Links
            => links;

        private static readonly List<Node> nodes;
        private static readonly List<Link> links;
        private static readonly HashSet<Node> nodeSet;
        private static readonly HashSet<Link> linkSet;
        private static readonly ulong ambientWattsPerSec = 100;
        private static IUIElement activeElement;
        private static double reqWattsPerSec, prodWattsPerSec;

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

        public static void AddLink(Link link)
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

            //double electrPropor = Math.Min(1, prodWattsPerSec / reqWattsPerSec);
            //Debug.Assert(electrPropor is >= 0 and <= 1);

            //throw new NotImplementedException();
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

            JobMatching.Match();
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
