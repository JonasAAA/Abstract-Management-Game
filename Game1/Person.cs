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
        public static int PeopleCount
            => people.Count;
        public static readonly double momentumCoeff, minReqWattsPerSec, maxReqWattsPerSec;
        /// <summary>
        /// MUST always be the same for all people
        /// as the way industry deals with required electricity requires that
        /// </summary>
        private static readonly ulong defaultElectrPriority;
        private static readonly TimeSpan minSeekChangeTime, maxSeekChangeTime;
        private static readonly MyHashSet<Person> people;

        static Person()
        {
            momentumCoeff = .2;
            minReqWattsPerSec = .1;
            maxReqWattsPerSec = 1;
            defaultElectrPriority = 100;
            minSeekChangeTime = TimeSpan.FromSeconds(5);
            maxSeekChangeTime = TimeSpan.FromSeconds(30);

            people = new();
        }

        public static Person GeneratePerson()
            => new
            (
                enjoyments: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => C.Random(min: 0.0, max: 1.0)
                ),
                talents: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => C.Random(min: 0.0, max: 1.0)
                ),
                skills: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => C.Random(min: 0.0, max: 1.0)
                ),
                weight: 10,
                reqWattsPerSec: C.Random(min: minReqWattsPerSec, max: maxReqWattsPerSec),
                seekChangeTime: minSeekChangeTime +  C.Random(min: 0, max: 1) * (maxSeekChangeTime - minSeekChangeTime)
            );

        public static Person GenerateChild(Person person1, Person person2)
            => MakePersonByClamping
            (
                enjoyments: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => (person1.enjoyments[indType] + person2.enjoyments[indType]) * .5 + C.Random(min: -.1, max: .1)
                ),
                talents: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => (person1.talents[indType] + person2.talents[indType]) * .5 + C.Random(min: -.1, max: .1)
                ),
                skills: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => (person1.skills[indType] + person2.skills[indType]) * .5 + C.Random(min: -.1, max: .1)
                ),
                weight: (int)(person1.weight + person2.weight) / 2 + C.RandInt(min: -1, max: 1),
                reqWattsPerSec: (person1.reqWattsPerSec + person2.reqWattsPerSec) * .5 + C.Random(min: -.1, max: .1),
                seekChangeTime: (person1.seekChangeTime + person2.seekChangeTime) * .5 + C.Random(-.1, .1) * maxSeekChangeTime
            );

        public static IEnumerable<Person> GetActivitySeekingPeople()
            => from person in people
               where person.IfSeeksNewActivity()
               select person;

        private static Person MakePersonByClamping(Dictionary<IndustryType, double> enjoyments, Dictionary<IndustryType, double> talents, Dictionary<IndustryType, double> skills, int weight, double reqWattsPerSec, TimeSpan seekChangeTime)
            => new
            (
                enjoyments: enjoyments.ClampValues(min: 0, max: 1),
                talents: talents.ClampValues(min: 0, max: 1),
                skills: skills.ClampValues(min: 0, max: 1),
                weight: (ulong)Math.Max(1, weight),
                reqWattsPerSec: Math.Clamp(reqWattsPerSec, maxReqWattsPerSec, maxReqWattsPerSec),
                seekChangeTime: TimeSpan.FromSeconds(Math.Clamp(seekChangeTime.TotalSeconds, minSeekChangeTime.TotalSeconds, maxSeekChangeTime.TotalSeconds))
            );

        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> enjoyments;
        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> talents;
        // between 0 and 1
        public readonly Dictionary<IndustryType, double> skills;
        //public double MinAcceptableEnjoyment { get; private set; }

        public ulong ElectrPriority
            => activityCenter switch
            {
                null => defaultElectrPriority,
                // if person has higher priority then activityCenter,
                // then activityCenter most likely will can't work at full capacity
                // so will not use all the available electricity
                not null => Math.Min(defaultElectrPriority, activityCenter.ElectrPriority)
            };
        public Vector2? ActivityCenterPosition
            => activityCenter?.Position;
        public Vector2 ClosestNodePos { get; private set; }
        public double ElectrPropor { get; private set; }
        public IReadOnlyDictionary<ActivityType, TimeSpan> LastActivityTimes
            => lastActivityTimes;
        public readonly ulong weight;
        public readonly double reqWattsPerSec;

        /// <summary>
        /// CURRENTLY UNUSED
        /// </summary>
        public event Action Deleted;

        /// <summary>
        /// is null just been let go from activity center
        /// </summary>
        private IPersonFacingActivityCenter activityCenter;
        private readonly TimeSpan seekChangeTime;
        private TimeSpan timeSinceActivitySearch;
        private readonly Dictionary<ActivityType, TimeSpan> lastActivityTimes;

        private Person(Dictionary<IndustryType, double> enjoyments, Dictionary<IndustryType, double> talents, Dictionary<IndustryType, double> skills, ulong weight, double reqWattsPerSec, TimeSpan seekChangeTime)
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

            activityCenter = null;
            this.weight = weight;

            if (reqWattsPerSec < minReqWattsPerSec || reqWattsPerSec > maxReqWattsPerSec)
                throw new ArgumentOutOfRangeException();
            this.reqWattsPerSec = reqWattsPerSec;

            ElectrPropor = 0;

            ElectricityDistributor.AddElectrConsumer(electrConsumer: this);

            if (seekChangeTime < minSeekChangeTime || seekChangeTime > maxSeekChangeTime)
                throw new ArgumentOutOfRangeException();
            this.seekChangeTime = seekChangeTime;
            timeSinceActivitySearch = seekChangeTime;
            lastActivityTimes = Enum.GetValues<ActivityType>().ToDictionary
            (
                keySelector: activityType => activityType,
                elementSelector: activityType => TimeSpan.MinValue / 3
            );

            people.Add(this);
            //Graph.World.AddPerson(person: this);
        }

        public void Arrived()
            => activityCenter.TakePerson(person: this);

        public void Update(Vector2 closestNodePos)
        {
            ClosestNodePos = closestNodePos;
            if (activityCenter is not null && activityCenter.IsPersonHere(person: this))
            {
                activityCenter.UpdatePerson(person: this);
                lastActivityTimes[activityCenter.ActivityType] = Graph.CurrentTime;
                timeSinceActivitySearch += Graph.Elapsed;
            }
            else
                IActivityCenter.UpdatePersonDefault(person: this);
        }

        double IElectrConsumer.ReqWattsPerSec()
            => reqWattsPerSec;

        void IElectrConsumer.ConsumeElectr(double electrPropor)
            => ElectrPropor = electrPropor;

        public bool IfSeeksNewActivity()
            => activityCenter is null || (timeSinceActivitySearch >= seekChangeTime && activityCenter.CanPersonLeave(person: this));

        public IPersonFacingActivityCenter ChooseActivityCenter(IEnumerable<IPersonFacingActivityCenter> activityCenters)
        {
            if (!IfSeeksNewActivity())
                throw new InvalidOperationException();

            var bestActivityCenter = activityCenters.ArgMaxOrDefault(activityCenter => activityCenter.PersonScoreOfThis(person: this));
            if (bestActivityCenter is null)
                throw new ArgumentException();
            SetActivityCenter(newActivityCenter: bestActivityCenter);
            return activityCenter;
        }

        private void SetActivityCenter(IPersonFacingActivityCenter newActivityCenter)
        {
            timeSinceActivitySearch = TimeSpan.Zero;
            if (activityCenter == newActivityCenter)
                return;

            if (activityCenter is not null)
                activityCenter.RemovePerson(person: this);
            activityCenter = newActivityCenter;
        }

        public void LetGoFromActivityCenter()
            => SetActivityCenter(newActivityCenter: null);
    }
}
