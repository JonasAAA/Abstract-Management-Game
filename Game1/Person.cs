using Game1.Delegates;
using Game1.Industries;
using static Game1.WorldManager;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// person must be unhappy when don't get enough energy
    /// </summary>
    [Serializable]
    public class Person : IEnergyConsumer
    {
        public static Person GeneratePerson(MyVector2 nodePos)
            => new
            (
                nodePos: nodePos,
                enjoyments: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => Score.GenerateRandom()
                ),
                talents: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => Score.GenerateRandom()
                ),
                skills: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => Score.GenerateRandom()
                ),
                // TODO: get rid of hard-coded constant
                weight: 10,
                reqWatts: C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                seekChangeTime: C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime)
            );

        public static Person GenerateChild(Person person1, Person person2, MyVector2 nodePos)
        {
            return new Person
            (
                nodePos: nodePos,
                enjoyments: CreateIndustryScoreDict
                (
                    personalScore: (person, indType) => person.enjoyments[indType]
                ),
                talents: CreateIndustryScoreDict
                (
                    personalScore: (person, indType) => person.talents[indType]
                ),
                skills: Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType => Score.lowest
                ),
                // TODO: get rid of hard-coded constant
                weight: 10,
                reqWatts:
                    CurWorldConfig.parentContribToChildPropor * (person1.reqWatts + person2.reqWatts) * (UDouble).5
                    + CurWorldConfig.parentContribToChildPropor.Opposite() * C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                seekChangeTime:
                    CurWorldConfig.parentContribToChildPropor * (person1.seekChangeTime + person2.seekChangeTime) * (UDouble).5
                    + CurWorldConfig.parentContribToChildPropor.Opposite() * C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime)
            );

            Dictionary<IndustryType, Score> CreateIndustryScoreDict(Func<Person, IndustryType, Score> personalScore)
                => Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType
                        => Score.WightedAverageOfTwo
                        (
                            score1: Score.Average(personalScore(person1, indType), personalScore(person2, indType)),
                            score2: Score.GenerateRandom(),
                            score1Propor: CurWorldConfig.parentContribToChildPropor
                        )
                );
        }

        public readonly ReadOnlyDictionary<IndustryType, Score> enjoyments;
        public readonly ReadOnlyDictionary<IndustryType, Score> talents;
        public readonly Dictionary<IndustryType, Score> skills;
        
        public MyVector2? ActivityCenterPosition
            => activityCenter?.Position;
        public MyVector2 ClosestNodePos { get; private set; }
        public Propor EnergyPropor { get; private set; }
        public IReadOnlyDictionary<ActivityType, TimeSpan> LastActivityTimes
            => lastActivityTimes;
        public readonly ulong weight;
        public readonly UDouble reqWatts;

        /// <summary>
        /// CURRENTLY UNUSED
        /// </summary>
        public IEvent<IDeletedListener> Deleted
            => deleted;

        /// <summary>
        /// is null just been let go from activity center
        /// </summary>
        private IPersonFacingActivityCenter activityCenter;
        private readonly TimeSpan seekChangeTime;
        private TimeSpan timeSinceActivitySearch;
        private readonly Dictionary<ActivityType, TimeSpan> lastActivityTimes;
        private MyVector2 prevNodePos;
        private readonly Event<IDeletedListener> deleted;

        private Person(MyVector2 nodePos, Dictionary<IndustryType, Score> enjoyments, Dictionary<IndustryType, Score> talents, Dictionary<IndustryType, Score> skills, ulong weight, UDouble reqWatts, TimeSpan seekChangeTime)
        {
            prevNodePos = nodePos;
            ClosestNodePos = nodePos;
            this.enjoyments = new(enjoyments);
            this.talents = new(talents);
            this.skills = new(skills);

            activityCenter = null;
            this.weight = weight;

            if (reqWatts < CurWorldConfig.personMinReqWatts || reqWatts > CurWorldConfig.personMaxReqWatts)
                throw new ArgumentOutOfRangeException();
            this.reqWatts = reqWatts;

            EnergyPropor = Propor.empty;

            if (seekChangeTime < CurWorldConfig.personMinSeekChangeTime || seekChangeTime > CurWorldConfig.personMaxSeekChangeTime)
                throw new ArgumentOutOfRangeException();
            this.seekChangeTime = seekChangeTime;
            timeSinceActivitySearch = seekChangeTime;
            lastActivityTimes = Enum.GetValues<ActivityType>().ToDictionary
            (
                keySelector: activityType => activityType,
                elementSelector: activityType => TimeSpan.MinValue / 3
            );

            deleted = new();

            CurWorldManager.AddEnergyConsumer(energyConsumer: this);
            CurWorldManager.AddPerson(person: this);
        }

        public void Arrived()
            => activityCenter.TakePerson(person: this);

        public void Update(MyVector2 prevNodePos, MyVector2 closestNodePos)
        {
            this.prevNodePos = prevNodePos;
            ClosestNodePos = closestNodePos;
            if (activityCenter is not null && activityCenter.IsPersonHere(person: this))
            {
                activityCenter.UpdatePerson(person: this);
                lastActivityTimes[activityCenter.ActivityType] = CurWorldManager.CurTime;
                timeSinceActivitySearch += CurWorldManager.Elapsed;
            }
            else
                IActivityCenter.UpdatePersonDefault(person: this);
        }

        UDouble IEnergyConsumer.ReqWatts()
            => reqWatts;

        void IEnergyConsumer.ConsumeEnergy(Propor energyPropor)
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

        EnergyPriority IEnergyConsumer.EnergyPriority
            => activityCenter switch
            {
                null => CurWorldConfig.personDefaultEnergyPriority,
                // if person has higher priority then activityCenter,
                // then activityCenter most likely will can't work at full capacity
                // so will not use all the available energyicity
                not null => MyMathHelper.Min(CurWorldConfig.personDefaultEnergyPriority, activityCenter.EnergyPriority)
            };

        MyVector2 IEnergyConsumer.NodePos
            => prevNodePos;
    }
}
