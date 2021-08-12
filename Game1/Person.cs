using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Game1
{
    // TODO:
    // MinAcceptableEnjoyment needs to decrease if person stays unemployed
    // or could make it more similar to when job is vacant for a while
    public class Person
    {
        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> enjoyments;
        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> talents;
        // between 0 and 1
        public readonly Dictionary<IndustryType, double> skills;
        public double MinAcceptableEnjoyment { get; private set; }
        public Node Node { get; set; }
        public Job Job { get; set; }

        private Person(Dictionary<IndustryType, double> enjoyments, Dictionary<IndustryType, double> talents, Dictionary<IndustryType, double> skills)
        {
            if (!enjoyments.Values.All(C.IsInSuitableRange))
                throw new ArgumentException();
            this.enjoyments = new(enjoyments);

            if (!talents.Values.Any(C.IsInSuitableRange))
                throw new ArgumentException();
            this.talents = new(talents);

            if (!skills.Values.All(C.IsInSuitableRange))
                throw new ArgumentException();
            this.skills = new(skills);

            Node = null;
            Job = null;
        }

        public static Person GenerateNew()
            => new
            (
                talents: Enum.GetValues<IndustryType>()
                    .Select(indType => (indType, value: C.Random(min: 0.0, max: 1.0)))
                    .ToDictionary(a => a.indType, a => a.value),
                enjoyments: Enum.GetValues<IndustryType>()
                    .Select(indType => (indType, value: C.Random(min: 0.0, max: 1.0)))
                    .ToDictionary(a => a.indType, a => a.value),
                skills: Enum.GetValues<IndustryType>()
                    .Select(indType => (indType, value: C.Random(min: 0.0, max: 1.0)))
                    .ToDictionary(a => a.indType, a => a.value)
            );

        // must be between 0 and 1 or double.NegativeInfinity
        public double EvaluateJob(Job job)
            => enjoyments[job.industryType];

        public void TakeJob(Job job)
        {
            if (EvaluateJob(job: job) is double.NegativeInfinity)
                throw new ArgumentException();

            //if (Job is not null)
            //{
            //    job.node.
            //}
            Job = job;

            // TODO:
            // travel to new job destination
            // if already had a job, need to inform it about quitting

            throw new NotImplementedException();
        }
    }
}
