using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// may have separate fire method when can put more there
    /// </summary>

    public class Person
    {
        public static double reqWattsPerSec { get; }
        private static readonly double timeSkillCoeff;

        static Person()
        {
            reqWattsPerSec = .2;
            timeSkillCoeff = .1;
        }

        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> enjoyments;
        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> talents;
        // between 0 and 1
        public readonly Dictionary<IndustryType, double> skills;
        //public double MinAcceptableEnjoyment { get; private set; }
        public Node Destination { get; private set; }
        public readonly ulong weight;

        private Person(Dictionary<IndustryType, double> enjoyments, Dictionary<IndustryType, double> talents, Dictionary<IndustryType, double> skills, ulong weight)
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
            this.weight = weight;
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
                    .ToDictionary(a => a.indType, a => a.value),
                weight: 10
            );

        // must be between 0 and 1 or double.NegativeInfinity
        public double JobScore(IJob job)
            => enjoyments[job.IndustryType];

        /// <summary>
        /// TODO:
        /// if already had a job, need to inform it about quitting
        /// </summary>
        public void TakeJob(IJob job, Node jobNode)
        {
            if (JobScore(job: job) is double.NegativeInfinity)
                throw new ArgumentException();

            Destination = jobNode;

            // TODO:
            // if already had a job, need to inform it about quitting
        }

        public void StopTravelling()
            =>Destination = null;

        public void Work(IndustryType industryType)
        {
            Debug.Assert(C.IsInSuitableRange(value: skills[industryType]));
            skills[industryType] = 1 - (1 - skills[industryType]) * Math.Pow(1 - talents[industryType], C.ElapsedGameTime.TotalSeconds * timeSkillCoeff);
            Debug.Assert(C.IsInSuitableRange(value: skills[industryType]));
        }
    }
}
