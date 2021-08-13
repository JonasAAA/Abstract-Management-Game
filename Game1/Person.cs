using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Game1
{
    // TODO:
    // MinAcceptableEnjoyment needs to decrease if person stays unemployed
    // or could make it more similar to when job is vacant for a while
    //
    // also need to increse relevant skill when employed
    public class Person
    {
        public static readonly double reqWattsPerSec;

        static Person()
            => reqWattsPerSec = .2;

        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> enjoyments;
        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> talents;
        // between 0 and 1
        public readonly Dictionary<IndustryType, double> skills;
        //public double MinAcceptableEnjoyment { get; private set; }
        public Node Destination { get; private set; }

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

            Destination = null;
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
        public double EvaluateJob(IJob job)
            => enjoyments[job.IndustryType];

        public void TakeJob(IJob job, Node jobNode)
        {
            if (EvaluateJob(job: job) is double.NegativeInfinity)
                throw new ArgumentException();

            Destination = jobNode;

            // TODO:
            // travel to new job destination
            // if already had a job, need to inform it about quitting
        }
    }
}
