using Game1.Industries;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using static Game1.WorldManager;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// person must be unhappy when don't get enough energy
    /// </summary>

    public class Person : IEnergyConsumer
    {
        public static int PeopleCount
            => people.Count;
        public static readonly double momentumCoeff, minReqWatts, maxReqWatts, randConrtribToChild, parentContribToChild;
        /// <summary>
        /// MUST always be the same for all people
        /// as the way industry deals with required energy requires that
        /// </summary>
        private static readonly ulong defaultEnergyPriority;
        private static readonly TimeSpan minSeekChangeTime, maxSeekChangeTime;
        private static readonly MyHashSet<Person> people;

        static Person()
        {
            momentumCoeff = .2;
            minReqWatts = .1;
            maxReqWatts = 1;
            randConrtribToChild = .1;
            parentContribToChild = 1 - randConrtribToChild;
            defaultEnergyPriority = 100;
            minSeekChangeTime = TimeSpan.FromSeconds(5);
            maxSeekChangeTime = TimeSpan.FromSeconds(30);

            people = new();
        }

        public static Person GeneratePerson(Vector2 nodePos)
            => new
            (
                nodePos: nodePos,
                enjoyments: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => C.Random(min: 0, max: 1)
                ),
                talents: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => C.Random(min: 0, max: 1)
                ),
                skills: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => C.Random(min: 0, max: 1)
                ),
                weight: 10,
                reqWatts: C.Random(min: minReqWatts, max: maxReqWatts),
                seekChangeTime: C.Random(min: minSeekChangeTime, max: maxSeekChangeTime)
            );

        public static Person GenerateChild(Person person1, Person person2, Vector2 nodePos)
            => new
            (
                nodePos: nodePos,
                enjoyments: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType
                        => parentContribToChild * (person1.enjoyments[indType] + person2.enjoyments[indType]) * .5
                        + randConrtribToChild * C.Random(min: 0, max: 1)
                ),
                talents: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType
                        => parentContribToChild * (person1.talents[indType] + person2.talents[indType]) * .5
                        + randConrtribToChild * C.Random(min: 0, max: 1)
                ),
                skills: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => 0.0
                ),
                weight: 10,
                reqWatts:
                    parentContribToChild * (person1.reqWatts + person2.reqWatts) * .5
                    + randConrtribToChild * C.Random(min: minReqWatts, max: maxReqWatts),
                seekChangeTime:
                    parentContribToChild * (person1.seekChangeTime + person2.seekChangeTime) * .5
                    + randConrtribToChild * C.Random(min: minSeekChangeTime, max: maxSeekChangeTime)
            );

        public static IEnumerable<Person> GetActivitySeekingPeople()
            => from person in people
               where person.IfSeeksNewActivity()
               select person;

        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> enjoyments;
        // between 0 and 1
        public readonly ReadOnlyDictionary<IndustryType, double> talents;
        // between 0 and 1
        public readonly Dictionary<IndustryType, double> skills;
        //public double MinAcceptableEnjoyment { get; private set; }

        
        public Vector2? ActivityCenterPosition
            => activityCenter?.Position;
        public Vector2 ClosestNodePos { get; private set; }
        public double EnergyPropor { get; private set; }
        public IReadOnlyDictionary<ActivityType, TimeSpan> LastActivityTimes
            => lastActivityTimes;
        public readonly ulong weight;
        public readonly double reqWatts;

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

        private Person(Vector2 nodePos, Dictionary<IndustryType, double> enjoyments, Dictionary<IndustryType, double> talents, Dictionary<IndustryType, double> skills, ulong weight, double reqWatts, TimeSpan seekChangeTime)
        {
            prevNodePos = nodePos;
            ClosestNodePos = nodePos;
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

            if (reqWatts < minReqWatts || reqWatts > maxReqWatts)
                throw new ArgumentOutOfRangeException();
            this.reqWatts = reqWatts;

            EnergyPropor = 0;

            AddEnergyConsumer(energyConsumer: this);

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
            //CurGraph.AddPerson(person: this);
        }

        public void Arrived()
            => activityCenter.TakePerson(person: this);

        public void Update(Vector2 prevNodePos, Vector2 closestNodePos)
        {
            this.prevNodePos = prevNodePos;
            ClosestNodePos = closestNodePos;
            if (activityCenter is not null && activityCenter.IsPersonHere(person: this))
            {
                activityCenter.UpdatePerson(person: this);
                lastActivityTimes[activityCenter.ActivityType] = CurTime;
                timeSinceActivitySearch += Elapsed;
            }
            else
                IActivityCenter.UpdatePersonDefault(person: this);
        }

        double IEnergyConsumer.ReqWatts()
            => reqWatts;

        void IEnergyConsumer.ConsumeEnergy(double energyPropor)
            => EnergyPropor = energyPropor;

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

        ulong IEnergyConsumer.EnergyPriority
            => activityCenter switch
            {
                null => defaultEnergyPriority,
                // if person has higher priority then activityCenter,
                // then activityCenter most likely will can't work at full capacity
                // so will not use all the available energyicity
                not null => Math.Min(defaultEnergyPriority, activityCenter.EnergyPriority)
            };

        Vector2 IEnergyConsumer.NodePos
            => prevNodePos;
        private Vector2 prevNodePos;
    }
}
