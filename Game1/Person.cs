using Game1.Industries;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// person must be unhappy when don't get enough electricity
    /// </summary>

    public class Person : IElectrConsumer
    {
        public static readonly double momentumCoeff;
        /// <summary>
        /// MUST always be the same for all people
        /// as the way industry deals with required electricity requires that
        /// </summary>
        private static readonly ulong defaultElectrPriority;
        //private static readonly double timeSkillCoeff;
        private static readonly ulong seekChangeFrameNum;

        static Person()
        {
            momentumCoeff = .2;
            defaultElectrPriority = 100;
            //timeSkillCoeff = .1;
            seekChangeFrameNum = 100;
        }

        public static Person GenerateNew(/*IActivityCenter defaultActivityCenter*/)
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
                reqWattsPerSec: .2,
                //defaultActivityCenter: defaultActivityCenter,
                startFrameNum: (ulong)C.RandInt(min: 0, max: (int)seekChangeFrameNum)
            );

        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> enjoyments;
        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> talents;
        // between 0 and 1
        public readonly Dictionary<IndustryType, double> skills;
        //public double MinAcceptableEnjoyment { get; private set; }
        
        //public Vector2? Destination { get; private set; }
        //private IJob1 job;

        public Vector2? ActivityCenterPosition
            => activityCenter?.Position;
        public Vector2 ClosestNodePos { get; private set; }
        public double ElectrPropor { get; private set; }
        public readonly ulong weight;
        public readonly double reqWattsPerSec;

        /// <summary>
        /// CURRENTLY UNUSED
        /// </summary>
        public event Action Deleted;


        /// <summary>
        /// is null just been let go from activity center
        /// </summary>
        private IPersonFacingActivityCenter /*defaultActivityCenter, */activityCenter;
        private ulong frameNum;

        private Person(Dictionary<IndustryType, double> enjoyments, Dictionary<IndustryType, double> talents, Dictionary<IndustryType, double> skills, ulong weight, double reqWattsPerSec, ulong startFrameNum)
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

            //Destination = null;
            //this.defaultActivityCenter = defaultActivityCenter;
            activityCenter = null;
            //job = null;
            this.weight = weight;

            if (reqWattsPerSec <= 0)
                throw new ArgumentOutOfRangeException();
            this.reqWattsPerSec = reqWattsPerSec;

            ElectrPropor = 0;

            ElectricityDistributor.AddElectrConsumer(electrConsumer: this);

            frameNum = startFrameNum;
        }

        //// must be between 0 and 1 or double.NegativeInfinity
        //public double EmployerScore(IEmployer1 employer)
        //    => enjoyments[employer.IndustryType];

        ///// <summary>
        ///// TODO:
        ///// if already had a employer, need to inform it about quitting
        ///// person may have event it calls when changes jobs, interested parties subscribe to it
        ///// </summary>
        //public void TakeJob(IJob1 job, Vector2 employerPos)
        //{
        //    this.job = job;
        //    Destination = employerPos;

        //    // TODO:
        //    // if already had a employer, need to inform it about quitting
        //}

        //public void Fire()
        //{
        //    job = null;
        //    StopTravelling();
        //}

        //public void GoAndKeepJob(Vector2 destination)
        //{
        //    if (Destination is not null)
        //        throw new InvalidOperationException();

        //    Destination = destination;
        //    if (job is not null)
        //        job.TempLeave(person: this);
        //    else
        //        throw new NotImplementedException();
        //}

        //public void StopTravelling()
        //    => Destination = null;

        public void Arrived()
            => activityCenter.TakePerson(person: this);

        public void Update(TimeSpan elapsed, Vector2 closestNodePos)
        {
            ClosestNodePos = closestNodePos;
            if (activityCenter is not null && activityCenter.IsPersonHere(person: this))
            {
                activityCenter.UpdatePerson(person: this, elapsed: elapsed);
                frameNum++;
                frameNum %= seekChangeFrameNum;
            }
            else
                IActivityCenter.UpdatePersonDefault(person: this, elapsed: elapsed);
        }

        //public void UpdateNotWorking(TimeSpan elapsed)
        //{
        //    if (job is not null && Destination is null)
        //        throw new InvalidOperationException();

        //    // skills may decrease here
        //    // person may go on vacation, etc.
        //}

        //public void UpdateWorking(TimeSpan elapsed, double workingPropor)
        //{
        //    if (!C.IsInSuitableRange(value: workingPropor))
        //        throw new ArgumentOutOfRangeException();

        //    Debug.Assert(C.IsInSuitableRange(value: skills[job.IndustryType]));
        //    skills[job.IndustryType] = 1 - (1 - skills[job.IndustryType]) * Math.Pow(1 - talents[job.IndustryType], elapsed.TotalSeconds * workingPropor * timeSkillCoeff);
        //    Debug.Assert(C.IsInSuitableRange(value: skills[job.IndustryType]));
        //}

        public ulong ElectrPriority
            => activityCenter switch
            {
                null => defaultElectrPriority,
                // if person has higher priority then activityCenter,
                // then activityCenter most likely will can't work at full capacity
                // so will not use all the available electricity
                not null => Math.Min(defaultElectrPriority, activityCenter.ElectrPriority)
            };

        double IElectrConsumer.ReqWattsPerSec()
            => reqWattsPerSec;

        void IElectrConsumer.ConsumeElectr(double electrPropor)
            => ElectrPropor = electrPropor;

        public bool IfSeeksNewActivity()
            => activityCenter is null || (frameNum % seekChangeFrameNum) is 0;

        public IPersonFacingActivityCenter ChooseActivityCenter(IEnumerable<IPersonFacingActivityCenter> activityCenters)
        {
            var bestActivityCenter = activityCenters.ArgMaxOrDefault(activityCenter => activityCenter.PersonScoreOfThis(person: this));
            if (bestActivityCenter is null)
                throw new ArgumentException();
            SetActivityCenter(newActivityCenter: bestActivityCenter);
            return activityCenter;
        }

        private void SetActivityCenter(IPersonFacingActivityCenter newActivityCenter)
        {
            if (activityCenter == newActivityCenter)
                return;

            if (activityCenter is not null)
                activityCenter.RemovePerson(person: this);
            activityCenter = newActivityCenter;
            frameNum = 1;
        }

        public void LetGoFromActivityCenter()
            => SetActivityCenter(newActivityCenter: null);

        //private void SetActivityCenterToDefault()
        //{
        //    Debug.Assert(defaultActivityCenter is not null);
        //    SetActivityCenter(newActivityCenter: defaultActivityCenter);
        //}
    }
}
