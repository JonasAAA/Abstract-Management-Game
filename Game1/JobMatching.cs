//using Microsoft.Xna.Framework;
//using Priority_Queue;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;

//namespace Game1
//{
//    public static class JobMatching
//    {
//        public static readonly HashSet<Job> vacantJobs;
//        public static readonly HashSet<Person> unemployedPeople;

//        private static readonly double enjoymentCoeff, talentCoeff, skillCoeff, vacancyDurationCoeff, distCoeff, minAcceptableScore;

//        static JobMatching()
//        {
//            vacantJobs = new();
//            unemployedPeople = new();
//            enjoymentCoeff = .2;
//            talentCoeff = .2;
//            skillCoeff = .2;
//            distCoeff = .2;
//            vacancyDurationCoeff = .2;
//            minAcceptableScore = .5;
//        }

//        // later should take as parameters list of employed people looking for job and list of filled jobs looking to change employee
//        public static void Match()
//        {
//            // prioritizes pairs with high score
//            SimplePriorityQueue<(Job job, Person person), double> pairs = new((x, y) => y.CompareTo(x));
//            foreach (var job in vacantJobs)
//                foreach (var person in unemployedPeople)
//                {
//                    double score = Score(job: job, person: person);
//                    Debug.Assert(C.IsSuitable(value: score));
//                    if (score >= minAcceptableScore)
//                        pairs.Enqueue(item: (job, person), priority: score);
//                }

//            while (pairs.Count > 0)
//            {
//                (Job job, Person person) = pairs.Dequeue();
//                person.TakeJob(job: job);
//                foreach (var otherJob in vacantJobs)
//                    pairs.TryRemove(item: (otherJob, person));

//                job.Hire(person: person);
//                foreach (var otherPerson in unemployedPeople)
//                {
//                    if (otherPerson == person)
//                        continue;
//                    pairs.TryRemove(item: (job, otherPerson));
//                    double score = Score(job: job, person: otherPerson);
//                    Debug.Assert(C.IsSuitable(value: score));
//                    if (!job.IsFull && score >= minAcceptableScore)
//                        pairs.Enqueue(item: (job, otherPerson), priority: score);
//                }
//            }
//        }

//        // each parameter must be between 0 and 1 or double.NegativeInfinity
//        // larger means this pair is more likely to work
//        // must be between 0 and 1 or double.NegativeInfinity
//        private static double Score(Job job, Person person)
//            => enjoymentCoeff * person.EvaluateJob(job: job)
//            + talentCoeff * person.talents[job.industryType]
//            + skillCoeff * person.skills[job.industryType]
//            + vacancyDurationCoeff * VacancyDuration(startTime: job.offerStartTime)
//            + distCoeff * Distance(node1: job.node, node2: person.Node);

//        // must be between 0 and 1
//        private static double VacancyDuration(TimeSpan startTime)
//        {
//            if (startTime > C.TotalGameTime)
//                throw new ArgumentOutOfRangeException();
//            return 1 - Math.Tanh((C.TotalGameTime - startTime).TotalSeconds);
//        }

//        // must be between 0 and 1 or double.NegativeInfinity
//        // should later be changed to graph distance (either time or electricity cost)
//        private static double Distance(Node node1, Node node2)
//            => 1 - Math.Tanh(Vector2.Distance(node1.Position, node2.Position) / 100);
//    }
//}
