using Game1.Industries;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// person must be unhappy when don't get enough energy
    /// </summary>
    [DataContract]
    public class Person : IEnergyConsumer
    {
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
                reqWatts: C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                seekChangeTime: C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime)
            );

        public static Person GenerateChild(Person person1, Person person2, Vector2 nodePos)
            => new
            (
                nodePos: nodePos,
                enjoyments: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType
                        => CurWorldConfig.parentContribToChild * (person1.enjoyments[indType] + person2.enjoyments[indType]) * .5
                        + CurWorldConfig.randConrtribToChild * C.Random(min: 0, max: 1)
                ),
                talents: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType
                        => CurWorldConfig.parentContribToChild * (person1.talents[indType] + person2.talents[indType]) * .5
                        + CurWorldConfig.randConrtribToChild * C.Random(min: 0, max: 1)
                ),
                skills: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => 0.0
                ),
                weight: 10,
                reqWatts:
                    CurWorldConfig.parentContribToChild * (person1.reqWatts + person2.reqWatts) * .5
                    + CurWorldConfig.randConrtribToChild * C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                seekChangeTime:
                    CurWorldConfig.parentContribToChild * (person1.seekChangeTime + person2.seekChangeTime) * .5
                    + CurWorldConfig.randConrtribToChild * C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime)
            );

        // between 0 and 1
        [DataMember]
        public readonly ReadOnlyDictionary<IndustryType, double> enjoyments;
        // between 0 and 1
        [DataMember]
        public readonly ReadOnlyDictionary<IndustryType, double> talents;
        // between 0 and 1
        [DataMember]
        public readonly Dictionary<IndustryType, double> skills;
        //public double MinAcceptableEnjoyment { get; private set; }
        
        public Vector2? ActivityCenterPosition
            => activityCenter?.Position;
        [DataMember]
        public Vector2 ClosestNodePos { get; private set; }
        [DataMember]
        public double EnergyPropor { get; private set; }
        public IReadOnlyDictionary<ActivityType, TimeSpan> LastActivityTimes
            => lastActivityTimes;
        [DataMember]
        public readonly ulong weight;
        [DataMember]
        public readonly double reqWatts;

        /// <summary>
        /// CURRENTLY UNUSED
        /// </summary>
        [field:NonSerialized]
        public event Action Deleted;

        /// <summary>
        /// is null just been let go from activity center
        /// </summary>
        [DataMember]
        private IPersonFacingActivityCenter activityCenter;
        [DataMember]
        private readonly TimeSpan seekChangeTime;
        [DataMember]
        private TimeSpan timeSinceActivitySearch;
        [DataMember]
        private readonly Dictionary<ActivityType, TimeSpan> lastActivityTimes;
        [DataMember]
        private Vector2 prevNodePos;

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

            if (reqWatts < CurWorldConfig.personMinReqWatts || reqWatts > CurWorldConfig.personMaxReqWatts)
                throw new ArgumentOutOfRangeException();
            this.reqWatts = reqWatts;

            EnergyPropor = 0;

            if (seekChangeTime < CurWorldConfig.personMinSeekChangeTime || seekChangeTime > CurWorldConfig.personMaxSeekChangeTime)
                throw new ArgumentOutOfRangeException();
            this.seekChangeTime = seekChangeTime;
            timeSinceActivitySearch = seekChangeTime;
            lastActivityTimes = Enum.GetValues<ActivityType>().ToDictionary
            (
                keySelector: activityType => activityType,
                elementSelector: activityType => TimeSpan.MinValue / 3
            );

            AddEnergyConsumer(energyConsumer: this);
            AddPerson(person: this);
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
                null => CurWorldConfig.personDefaultEnergyPriority,
                // if person has higher priority then activityCenter,
                // then activityCenter most likely will can't work at full capacity
                // so will not use all the available energyicity
                not null => Math.Min(CurWorldConfig.personDefaultEnergyPriority, activityCenter.EnergyPriority)
            };

        Vector2 IEnergyConsumer.NodePos
            => prevNodePos;
    }
}
