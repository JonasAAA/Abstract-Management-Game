using Microsoft.Xna.Framework;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public static class JobMatching
    {
        private static readonly double enjoymentCoeff, talentCoeff, skillCoeff, desperationCoeff, distCoeff, minAcceptableScore;

        static JobMatching()
        {
            enjoymentCoeff = .2;
            talentCoeff = .1;
            skillCoeff = .2;
            distCoeff = .1;
            desperationCoeff = .4;

            minAcceptableScore = .4;
        }

        private record PersonNode(Person Person, Node Node);
        private record JobNode(IJob Job, Node Node);

        public static void Match()
        {
            HashSet<JobNode> vacantJobs =
                (from node in Graph.Nodes
                 where node.Job is not null && node.Job.Desperation() is not double.NegativeInfinity
                 select new JobNode(Job: node.Job, Node: node)).ToHashSet();
            HashSet<PersonNode> unemployedPeople =
                (from node in Graph.Nodes
                 from person in node.UnemployedPeople
                 select new PersonNode(Person: person, Node: node)).ToHashSet();

            // prioritizes pairs with high score
            SimplePriorityQueue<(JobNode jobNode, PersonNode personNode), double> pairings = new((x, y) => y.CompareTo(x));
            foreach (var jobNode in vacantJobs)
                foreach (var personNode in unemployedPeople)
                {
                    double score = NewEmploymentScore(jobNode: jobNode, personNode: personNode);
                    Debug.Assert(C.IsSuitable(value: score));
                    if (score >= minAcceptableScore)
                        pairings.Enqueue(item: (jobNode, personNode), priority: score);
                }

            while (pairings.Count > 0)
            {
                (JobNode jobNode, PersonNode personNode) = pairings.Dequeue();

                jobNode.Job.Hire(person: personNode.Person);
                personNode.Person.TakeJob(job: jobNode.Job, jobNode: jobNode.Node);

                if (jobNode.Job.Desperation() is double.NegativeInfinity)
                    vacantJobs.Remove(jobNode);
                unemployedPeople.Remove(personNode);

                foreach (var otherPersonNode in unemployedPeople)
                    pairings.TryRemove(item: (jobNode, otherPersonNode));
                foreach (var otherJobNode in vacantJobs)
                    pairings.TryRemove(item: (otherJobNode, personNode));

                if (vacantJobs.Contains(jobNode))
                {
                    foreach (var otherPersonNode in unemployedPeople)
                    {
                        double score = NewEmploymentScore(jobNode: jobNode, personNode: otherPersonNode);
                        Debug.Assert(C.IsSuitable(value: score));
                        if (score >= minAcceptableScore)
                            pairings.Enqueue(item: (jobNode, otherPersonNode), priority: score);
                    }
                }
            }
        }

        // each parameter must be between 0 and 1 or double.NegativeInfinity
        // larger means this pair is more likely to work
        // must be between 0 and 1 or double.NegativeInfinity
        private static double NewEmploymentScore(JobNode jobNode, PersonNode personNode)
            => enjoymentCoeff * personNode.Person.JobScore(job: jobNode.Job)
            + talentCoeff * personNode.Person.talents[jobNode.Job.IndustryType]
            + skillCoeff * personNode.Person.skills[jobNode.Job.IndustryType]
            + desperationCoeff * jobNode.Job.Desperation()
            + distCoeff * Distance(node1: jobNode.Node, node2: personNode.Node);

        public static double CurrentEmploymentScore(IJob job, Person person)
            => enjoymentCoeff * person.JobScore(job: job)
            + talentCoeff * person.talents[job.IndustryType]
            + skillCoeff * person.skills[job.IndustryType];

        // must be between 0 and 1 or double.NegativeInfinity
        // should later be changed to graph distance (either time or electricity cost)
        private static double Distance(Node node1, Node node2)
            => 1 - Math.Tanh(Vector2.Distance(node1.Position, node2.Position) / 100);
    }
}