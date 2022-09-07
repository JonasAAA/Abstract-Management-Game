using Game1.Delegates;
using Game1.Industries;
using System.Diagnostics.CodeAnalysis;
using static Game1.WorldManager;

namespace Game1.Inhabitants
{
    /// <summary>
    /// TODO:
    /// person must be unhappy when don't get enough energy
    /// </summary>
    [Serializable]
    public sealed class RealPerson : IEnergyConsumer
    {
        [Serializable]
        public readonly record struct UpdateLocationParams(NodeID LastNodeID, NodeID ClosestNodeID);

        public static void GeneratePersonByMagic(NodeID nodeID, ReservedResPile resSource, RealPeople childDestin)
            => childDestin.AddByMagic
            (
                realPerson: new
                (
                    nodeID: nodeID,
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
                    reqWatts: C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                    seekChangeTime: C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime),
                    resSource: resSource,
                    consistsOfResAmounts: resAmountsPerPerson,
                    startingHappiness: CurWorldConfig.startingHappiness
                )
            );

        public static void GenerateChild(VirtualPerson parent1, VirtualPerson parent2, NodeID nodeID, ReservedResPile resSource, RealPeople parentSource, RealPeople childDestin)
        {
            if (!parentSource.Contains(person: parent1) || !parentSource.Contains(person: parent2))
                throw new ArgumentException();
            childDestin.AddByMagic
            (
                realPerson: new
                (
                    nodeID: nodeID,
                    enjoyments: CreateIndustryScoreDict
                    (
                        personalScore: (person, indType) => person.Enjoyments[indType]
                    ),
                    talents: CreateIndustryScoreDict
                    (
                        personalScore: (person, indType) => person.Talents[indType]
                    ),
                    skills: Enum.GetValues<IndustryType>().ToDictionary
                    (
                        keySelector: indType => indType,
                        elementSelector: indType => Score.lowest
                    ),
                    reqWatts:
                        CurWorldConfig.parentContribToChildPropor * (parent1.ReqWatts + parent2.ReqWatts) * (UDouble).5
                        + CurWorldConfig.parentContribToChildPropor.Opposite() * C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                    seekChangeTime:
                        CurWorldConfig.parentContribToChildPropor * (parent1.SeekChangeTime + parent2.SeekChangeTime) * (UDouble).5
                        + CurWorldConfig.parentContribToChildPropor.Opposite() * C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime),
                    resSource: resSource,
                    consistsOfResAmounts: resAmountsPerPerson,
                    startingHappiness: Score.Average(parent1.Happiness, parent2.Happiness)
                )
            );

            return;

            Dictionary<IndustryType, Score> CreateIndustryScoreDict(Func<VirtualPerson, IndustryType, Score> personalScore)
                => Enum.GetValues<IndustryType>().ToDictionary
                (
                    keySelector: indType => indType,
                    elementSelector: indType
                        => Score.WeightedAverageOfTwo
                        (
                            score1: Score.Average(personalScore(parent1, indType), personalScore(parent2, indType)),
                            score2: Score.GenerateRandom(),
                            score1Propor: CurWorldConfig.parentContribToChildPropor
                        )
                );
        }

        // TODO: move to some config file
        // Long-term, make each person require different amount of resources
        public static readonly ResAmounts resAmountsPerPerson;

        static RealPerson()
            => resAmountsPerPerson = new()
            {
                [(ResInd)0] = 10
            };

        public readonly VirtualPerson asVirtual;

        // TODO(performance): could replace these with array-backed custom dictionaries when use enums as keys
        /// <summary>
        /// At least one enjoyment will have Score.highest value, and at leat one enjoyment will value Score.lowest value
        /// </summary>
        public readonly IReadOnlyDictionary<IndustryType, Score> enjoyments;
        public readonly IReadOnlyDictionary<IndustryType, Score> talents;
        public IReadOnlyDictionary<IndustryType, Score> Skills
            => skills;
        public NodeID? ActivityCenterNodeID
            => activityCenter?.NodeID;
        public NodeID ClosestNodeID { get; private set; }
        public Propor EnergyPropor { get; private set; }
        public IReadOnlyDictionary<ActivityType, TimeSpan> LastActivityTimes
            => lastActivityTimes;
        public Mass Mass
            => consistsOfResPile.Mass;
        public readonly UDouble reqWatts;
        public readonly TimeSpan seekChangeTime;
        // Currently only influences productivity
        public Score Happiness { get; private set; }
        public Score MomentaryHappiness { get; private set; }
        /// <summary>
        /// CURRENTLY UNUSED
        /// If used, needs to transfer the resources the person consists of somewhere else
        /// </summary>
        public IEvent<IDeletedListener> Deleted
            => deleted;

        [MemberNotNullWhen(returnValue: true, member: nameof(activityCenter))]
        private bool IsInActivityCenter
            => activityCenter?.IsPersonHere(person: asVirtual) ?? false;
        private readonly Dictionary<IndustryType, Score> skills;
        /// <summary>
        /// is null if just been let go from activity center
        /// </summary>
        private IPersonFacingActivityCenter? activityCenter;
        private TimeSpan timeSinceActivitySearch;
        private readonly Dictionary<ActivityType, TimeSpan> lastActivityTimes;
        private NodeID lastNodeID;
        private readonly Event<IDeletedListener> deleted;
        private readonly ReservedResPile consistsOfResPile;

        private RealPerson(NodeID nodeID, Dictionary<IndustryType, Score> enjoyments, Dictionary<IndustryType, Score> talents, Dictionary<IndustryType, Score> skills, UDouble reqWatts, TimeSpan seekChangeTime, [DisallowNull] ReservedResPile? resSource, ResAmounts consistsOfResAmounts, Score startingHappiness)
        {
            lastNodeID = nodeID;
            ClosestNodeID = nodeID;
            Score.ScaleToHaveHighestAndLowest(scores: enjoyments);
            this.enjoyments = enjoyments;
            this.talents = talents;
            this.skills = skills;

            activityCenter = null;

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
            if (resSource.ResAmounts != consistsOfResAmounts)
                throw new ArgumentException();
            consistsOfResPile = ReservedResPile.CreateFromSource(source: ref resSource);
            asVirtual = new(realPerson: this);
            deleted = new();
            Happiness = startingHappiness;

            CurWorldManager.AddEnergyConsumer(energyConsumer: this);
            CurWorldManager.AddPerson(realPerson: this);
        }

        public void Arrived(RealPeople realPersonSource)
            => (activityCenter ?? throw new InvalidOperationException()).TakePersonFrom(realPersonSource: realPersonSource, realPerson: this);

        public void SetLocationMassCounter(MassCounter locationMassCounter)
            => consistsOfResPile.LocationMassCounter = locationMassCounter;

        /// <param name="updateSkillsParams">if null, will use default update</param>
        public void Update(UpdateLocationParams updateLocationParams, UpdatePersonSkillsParams? updateSkillsParams)
        {
            lastNodeID = updateLocationParams.LastNodeID;
            ClosestNodeID = updateLocationParams.ClosestNodeID;
            if (IsInActivityCenter)
            {
                lastActivityTimes[activityCenter.ActivityType] = CurWorldManager.CurTime;
                timeSinceActivitySearch += CurWorldManager.Elapsed;
            }
            MomentaryHappiness = CalculateMomentaryHappiness();
            var elapsed = CurWorldManager.Elapsed * EnergyPropor;
            Happiness = Score.BringCloser
            (
                current: Happiness,
                paramsOfChange: new Score.ParamsOfChange
                (
                    target: MomentaryHappiness,
                    elapsed: elapsed,
                    halvingDifferenceDuration: CurWorldConfig.happinessDifferenceHalvingDuration
                )
            );
            if (updateSkillsParams is null)
            {
#warning implement default update
                return;
            }
            foreach (var (industryType, paramsOfSkillChange) in updateSkillsParams)
                skills[industryType] = Score.BringCloser
                (
                    current: Skills[industryType],
                    paramsOfChange: paramsOfSkillChange
                );
        }

        private Score CalculateMomentaryHappiness()
        {
            if (!IsInActivityCenter)
                // When person is asleep, happiness doesn't change
                return Happiness;

            // TODO: include how much space they get, gravity preference, other's happiness maybe, etc.
            return activityCenter.PersonEnjoymentOfThis(person: asVirtual);
        }

        public Score ActualSkill(IndustryType industryType)
            => Score.WeightedAverageOfTwo
            (
                score1: Happiness,
                score2: skills[industryType],
                score1Propor: CurWorldConfig.actualSkillHappinessWeight
            );

        UDouble IEnergyConsumer.ReqWatts()
            => IsInActivityCenter ? reqWatts : 0;

        void IEnergyConsumer.ConsumeEnergy(Propor energyPropor)
            => EnergyPropor = energyPropor;

        public bool IfSeeksNewActivity()
            => activityCenter is null || timeSinceActivitySearch >= seekChangeTime && activityCenter.CanPersonLeave(person: asVirtual);

        public IPersonFacingActivityCenter ChooseActivityCenter(IEnumerable<IPersonFacingActivityCenter> activityCenters)
        {
            if (!IfSeeksNewActivity())
                throw new InvalidOperationException();

            // TODO: should probably include activityCenter.DistanceToHereAsPerson(person: asVirtual) into happiness calculation somehow as a person
            // motivated (only?) by their own happiness
            var bestActivityCenter = activityCenters.ArgMaxOrDefault
            (
                activityCenter => Score.WeightedAverage
                (
                    (weight: CurWorldConfig.personInertiaWeight, score: (activityCenter == this.activityCenter) ? Score.highest : Score.lowest),
                    (weight: CurWorldConfig.personEnjoymentWeight, score: activityCenter.PersonEnjoymentOfThis(person: asVirtual)),
                    (weight: CurWorldConfig.personTravelCostWeight, score: activityCenter.DistanceToHereAsPerson(person: asVirtual))
                )
            );
            if (bestActivityCenter is null)
                throw new ArgumentException("have no place to go");
            SetActivityCenter(newActivityCenter: bestActivityCenter);
            Debug.Assert(activityCenter is not null);
            return activityCenter;
        }

        private void SetActivityCenter(IPersonFacingActivityCenter? newActivityCenter)
        {
            timeSinceActivitySearch = TimeSpan.Zero;
            if (activityCenter == newActivityCenter)
                return;

            if (activityCenter is not null)
                activityCenter.RemovePerson(person: asVirtual);
            activityCenter = newActivityCenter;
        }

        public void LetGoFromActivityCenter()
            => SetActivityCenter(newActivityCenter: null);

        EnergyPriority IEnergyConsumer.EnergyPriority
            => IsInActivityCenter switch
            {
                // if person has higher priority then activityCenter,
                // then activityCenter most likely can't work at full capacity
                // so will not use all the available energy
                true => MyMathHelper.Min(CurWorldConfig.personDefaultEnergyPriority, activityCenter.EnergyPriority),
                false => CurWorldConfig.personDefaultEnergyPriority
            };

        NodeID IEnergyConsumer.NodeID
            => lastNodeID;

#if DEBUG2
#pragma warning disable CA1821 // Remove empty Finalizers
        ~RealPerson()
#pragma warning restore CA1821 // Remove empty Finalizers
            => throw new();
#endif
    }
}
