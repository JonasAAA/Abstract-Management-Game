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

        private record PersonAndPos(Person Person, Vector2 Pos);
        private record EmployerAndPos(IEmployer Employer, Vector2 Pos);

        public static void Match()
        {
            HashSet<EmployerAndPos> vacantEmployers =
                (from node in Graph.World.Nodes
                 where node.Employer is not null && node.Employer.Desperation() is not double.NegativeInfinity
                 select new EmployerAndPos(Employer: node.Employer, Pos: node.Position)).ToHashSet();
            HashSet<PersonAndPos> unemployedPeople =
                (from node in Graph.World.Nodes
                 from person in node.UnemployedPeople
                 select new PersonAndPos(Person: person, Pos: node.Position)).ToHashSet();

            // prioritizes pairs with high score
            SimplePriorityQueue<(EmployerAndPos employerPos, PersonAndPos personPos), double> pairings = new((x, y) => y.CompareTo(x));
            foreach (var employerPos in vacantEmployers)
                foreach (var personPos in unemployedPeople)
                {
                    double score = NewEmploymentScore(employerPos: employerPos, personPos: personPos);
                    Debug.Assert(C.IsSuitable(value: score));
                    if (score >= minAcceptableScore)
                        pairings.Enqueue(item: (employerPos, personPos), priority: score);
                }

            while (pairings.Count > 0)
            {
                (EmployerAndPos employerPos, PersonAndPos personPos) = pairings.Dequeue();

                employerPos.Employer.Hire(person: personPos.Person);
                personPos.Person.TakeJob(job: employerPos.Employer.CreateJob(), employerPos: employerPos.Pos);

                if (employerPos.Employer.Desperation() is double.NegativeInfinity)
                    vacantEmployers.Remove(employerPos);
                unemployedPeople.Remove(personPos);

                foreach (var otherPersonPos in unemployedPeople)
                    pairings.TryRemove(item: (employerPos, otherPersonPos));
                foreach (var otherEmployerPos in vacantEmployers)
                    pairings.TryRemove(item: (otherEmployerPos, personPos));

                if (vacantEmployers.Contains(employerPos))
                {
                    foreach (var otherPersonPos in unemployedPeople)
                    {
                        double score = NewEmploymentScore(employerPos: employerPos, personPos: otherPersonPos);
                        Debug.Assert(C.IsSuitable(value: score));
                        if (score >= minAcceptableScore)
                            pairings.Enqueue(item: (employerPos, otherPersonPos), priority: score);
                    }
                }
            }
        }

        // each parameter must be between 0 and 1 or double.NegativeInfinity
        // larger means this pair is more likely to work
        // must be between 0 and 1 or double.NegativeInfinity
        private static double NewEmploymentScore(EmployerAndPos employerPos, PersonAndPos personPos)
            => enjoymentCoeff * personPos.Person.EmployerScore(employer: employerPos.Employer)
            + talentCoeff * personPos.Person.talents[employerPos.Employer.IndustryType]
            + skillCoeff * personPos.Person.skills[employerPos.Employer.IndustryType]
            + desperationCoeff * employerPos.Employer.Desperation()
            + distCoeff * Distance(pos1: employerPos.Pos, pos2: personPos.Pos);

        public static double CurrentEmploymentScore(IEmployer employer, Person person)
            => enjoymentCoeff * person.EmployerScore(employer: employer)
            + talentCoeff * person.talents[employer.IndustryType]
            + skillCoeff * person.skills[employer.IndustryType];

        // must be between 0 and 1 or double.NegativeInfinity
        // should later be changed to graph distance (either time or electricity cost)
        private static double Distance(Vector2 pos1, Vector2 pos2)
            => 1 - Math.Tanh(Vector2.Distance(pos1, pos2) / 100);
    }
}