using Microsoft.Xna.Framework;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// may take into account that different people have different weight and required electricity
    /// so travel/employment costs would differ person to person
    /// </summary>
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
        private record EmployerNode(IEmployer Employer, Node Node);

        public static void Match()
        {
            HashSet<EmployerNode> vacantEmployers =
                (from node in Graph.Nodes
                 where node.Employer is not null && node.Employer.Desperation() is not double.NegativeInfinity
                 select new EmployerNode(Employer: node.Employer, Node: node)).ToHashSet();
            HashSet<PersonNode> unemployedPeople =
                (from node in Graph.Nodes
                 from person in node.UnemployedPeople
                 select new PersonNode(Person: person, Node: node)).ToHashSet();

            // prioritizes pairs with high score
            SimplePriorityQueue<(EmployerNode employerNode, PersonNode personNode), double> pairings = new((x, y) => y.CompareTo(x));
            foreach (var employerNode in vacantEmployers)
                foreach (var personNode in unemployedPeople)
                {
                    double score = NewEmploymentScore(employerNode: employerNode, personNode: personNode);
                    Debug.Assert(C.IsSuitable(value: score));
                    if (score >= minAcceptableScore)
                        pairings.Enqueue(item: (employerNode, personNode), priority: score);
                }

            while (pairings.Count > 0)
            {
                (EmployerNode employerNode, PersonNode personNode) = pairings.Dequeue();

                employerNode.Employer.Hire(person: personNode.Person);
                personNode.Person.TakeJob(job: employerNode.Employer.CreateJob(), employerNode: employerNode.Node);

                if (employerNode.Employer.Desperation() is double.NegativeInfinity)
                    vacantEmployers.Remove(employerNode);
                unemployedPeople.Remove(personNode);

                foreach (var otherPersonNode in unemployedPeople)
                    pairings.TryRemove(item: (employerNode, otherPersonNode));
                foreach (var otherEmployerNode in vacantEmployers)
                    pairings.TryRemove(item: (otherEmployerNode, personNode));

                if (vacantEmployers.Contains(employerNode))
                {
                    foreach (var otherPersonNode in unemployedPeople)
                    {
                        double score = NewEmploymentScore(employerNode: employerNode, personNode: otherPersonNode);
                        Debug.Assert(C.IsSuitable(value: score));
                        if (score >= minAcceptableScore)
                            pairings.Enqueue(item: (employerNode, otherPersonNode), priority: score);
                    }
                }
            }
        }

        // each parameter must be between 0 and 1 or double.NegativeInfinity
        // larger means this pair is more likely to work
        // must be between 0 and 1 or double.NegativeInfinity
        private static double NewEmploymentScore(EmployerNode employerNode, PersonNode personNode)
            => enjoymentCoeff * personNode.Person.EmployerScore(employer: employerNode.Employer)
            + talentCoeff * personNode.Person.talents[employerNode.Employer.IndustryType]
            + skillCoeff * personNode.Person.skills[employerNode.Employer.IndustryType]
            + desperationCoeff * employerNode.Employer.Desperation()
            + distCoeff * Distance(node1: employerNode.Node, node2: personNode.Node);

        public static double CurrentEmploymentScore(IEmployer employer, Person person)
            => enjoymentCoeff * person.EmployerScore(employer: employer)
            + talentCoeff * person.talents[employer.IndustryType]
            + skillCoeff * person.skills[employer.IndustryType];

        // must be between 0 and 1 or double.NegativeInfinity
        // should later be changed to graph distance (either time or electricity cost)
        private static double Distance(Node node1, Node node2)
            => 1 - Math.Tanh(Vector2.Distance(node1.Position, node2.Position) / 100);
    }
}