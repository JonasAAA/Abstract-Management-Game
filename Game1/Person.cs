using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// person must be unhappy when don't get enough electricity
    /// </summary>

    public class Person : IElectrConsumer
    {
        /// <summary>
        /// MUST always be the same for all people
        /// as the way industry deals with required electricity requires that
        /// </summary>
        public static readonly ulong defaultElectrPriority;
        private static readonly double timeSkillCoeff;

        static Person()
        {
            defaultElectrPriority = 100;
            timeSkillCoeff = .1;
        }

        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> enjoyments;
        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> talents;
        // between 0 and 1
        public readonly Dictionary<IndustryType, double> skills;
        //public double MinAcceptableEnjoyment { get; private set; }
        public Position Destination { get; private set; }
        private IJob job;
        public double ElectrPropor { get; private set; }
        public readonly ulong weight;
        public readonly double reqWattsPerSec;

        private Person(Dictionary<IndustryType, double> enjoyments, Dictionary<IndustryType, double> talents, Dictionary<IndustryType, double> skills, ulong weight, double reqWattsPerSec)
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
            job = null;
            this.weight = weight;

            if (reqWattsPerSec <= 0)
                throw new ArgumentOutOfRangeException();
            this.reqWattsPerSec = reqWattsPerSec;

            ElectrPropor = 0;

            ElectricityDistributor.AddElectrConsumer(electrConsumer: this);
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
                weight: 10,
                reqWattsPerSec: .2
            );

        // must be between 0 and 1 or double.NegativeInfinity
        public double EmployerScore(IEmployer employer)
            => enjoyments[employer.IndustryType];

        /// <summary>
        /// TODO:
        /// if already had a employer, need to inform it about quitting
        /// person may have event it calls when changes jobs, interested parties subscribe to it
        /// </summary>
        public void TakeJob(IJob job, Position employerPos)
        {
            this.job = job;
            Destination = employerPos;

            // TODO:
            // if already had a employer, need to inform it about quitting
        }

        public void Fire()
        {
            job = null;
            StopTravelling();
        }

        public void StopTravelling()
            => Destination = null;

        public void UpdateNotWorking(TimeSpan elapsed)
        {
            if (job is not null && Destination is null)
                throw new InvalidOperationException();

            // skills may decrease here
            // person may go on vacation, etc.
        }

        public void UpdateWorking(TimeSpan elapsed, double workingPropor)
        {
            if (!C.IsInSuitableRange(value: workingPropor))
                throw new ArgumentOutOfRangeException();

            Debug.Assert(C.IsInSuitableRange(value: skills[job.IndustryType]));
            skills[job.IndustryType] = 1 - (1 - skills[job.IndustryType]) * Math.Pow(1 - talents[job.IndustryType], elapsed.TotalSeconds * workingPropor * timeSkillCoeff);
            Debug.Assert(C.IsInSuitableRange(value: skills[job.IndustryType]));
        }

        public ulong ElectrPriority
            => job switch
            {
                null => defaultElectrPriority,
                // if person has higher priority then job,
                // then job most likely will can't work at full capacity
                // so will not use all the available electricity
                not null => Math.Min(defaultElectrPriority, job.ElectrPriority)
            };

        public double ReqWattsPerSec()
            => reqWattsPerSec;

        public void ConsumeElectr(double electrPropor)
            => ElectrPropor = electrPropor;
    }
}
