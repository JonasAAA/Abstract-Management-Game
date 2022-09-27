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
    public sealed class RealPerson : IEnergyConsumer, IWithRealPeopleStats
    {
        [Serializable]
        public readonly record struct UpdateLocationParams(NodeID LastNodeID, NodeID ClosestNodeID);

        public static void GeneratePersonByMagic(NodeID nodeID, ReservedResPile resSource, RealPeople childDestin)
            => childDestin.AddByMagic
            (
                realPerson: new
                (
                    realPeopleStats: RealPeopleStats.ForNewPerson
                    (
                        totalMass: resSource.Mass,
                        age: TimeSpan.FromSeconds(C.Random(min: 0, max: 200)),
                        startingHappiness: CurWorldConfig.startingHappiness,
                        enjoyments: new(selector: indType => Score.GenerateRandom()),
                        talents: new(selector: indType => Score.GenerateRandom()),
                        skills: new(selector: indType => Score.GenerateRandom())
                    ),
                    nodeID: nodeID,
                    reqWatts: C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                    seekChangeTime: C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime),
                    resSource: resSource,
                    consistsOfResAmounts: resAmountsPerPerson
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
                    realPeopleStats: RealPeopleStats.ForNewPerson
                    (
                        totalMass: resSource.Mass,
                        age: TimeSpan.Zero,
                        startingHappiness: Score.Average(parent1.Happiness, parent2.Happiness),
                        enjoyments: CreateIndustryScoreDict
                        (
                            personalScore: (person, indType) => person.Enjoyments[indType]
                        ),
                        talents: CreateIndustryScoreDict
                        (
                            personalScore: (person, indType) => person.Talents[indType]
                        ),
                        skills: new(selector: indType => Score.lowest)

                    ),
                    nodeID: nodeID,
                    reqWatts:
                        CurWorldConfig.parentContribToChildPropor * (parent1.ReqWatts + parent2.ReqWatts) * (UDouble).5
                        + CurWorldConfig.parentContribToChildPropor.Opposite() * C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                    seekChangeTime:
                        CurWorldConfig.parentContribToChildPropor * (parent1.SeekChangeTime + parent2.SeekChangeTime) * (UDouble).5
                        + CurWorldConfig.parentContribToChildPropor.Opposite() * C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime),
                    resSource: resSource,
                    consistsOfResAmounts: resAmountsPerPerson
                )
            );

            return;

            EnumDict<IndustryType, Score> CreateIndustryScoreDict(Func<VirtualPerson, IndustryType, Score> personalScore)
                => new
                (
                    selector: indType => Score.WeightedAverageOfTwo
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
        
        public NodeID? ActivityCenterNodeID
            => activityCenter?.NodeID;
        public NodeID ClosestNodeID { get; private set; }
        public Propor EnergyPropor { get; private set; }
        public EnumDict<ActivityType, TimeSpan> LastActivityTimes { get; private set; }
        public RealPeopleStats RealPeopleStats { get; private set; }
        public readonly UDouble reqWatts;
        public readonly TimeSpan seekChangeTime;
        /// <summary>
        /// CURRENTLY UNUSED
        /// If used, needs to transfer the resources the person consists of somewhere else
        /// </summary>
        public IEvent<IDeletedListener> Deleted
            => deleted;

        [MemberNotNullWhen(returnValue: true, member: nameof(activityCenter))]
        private bool IsInActivityCenter
            => activityCenter?.IsPersonHere(person: asVirtual) ?? false;
        /// <summary>
        /// is null if just been let go from activity center
        /// </summary>
        private IPersonFacingActivityCenter? activityCenter;
        private TimeSpan timeSinceActivitySearch;
        private NodeID lastNodeID;
        private readonly Event<IDeletedListener> deleted;
        private readonly ReservedResPile consistsOfResPile;
        private LocationCounters locationCounters;
        
        private RealPerson(RealPeopleStats realPeopleStats, NodeID nodeID, UDouble reqWatts, TimeSpan seekChangeTime, [DisallowNull] ReservedResPile? resSource, ResAmounts consistsOfResAmounts)
        {
            RealPeopleStats = realPeopleStats;
            lastNodeID = nodeID;
            ClosestNodeID = nodeID;

            activityCenter = null;

            if (reqWatts < CurWorldConfig.personMinReqWatts || reqWatts > CurWorldConfig.personMaxReqWatts)
                throw new ArgumentOutOfRangeException();
            this.reqWatts = reqWatts;

            EnergyPropor = Propor.empty;

            if (seekChangeTime < CurWorldConfig.personMinSeekChangeTime || seekChangeTime > CurWorldConfig.personMaxSeekChangeTime)
                throw new ArgumentOutOfRangeException();
            this.seekChangeTime = seekChangeTime;
            timeSinceActivitySearch = seekChangeTime;
            LastActivityTimes = new(selector: activityType => TimeSpan.MinValue / 3);
            if (resSource.ResAmounts != consistsOfResAmounts)
                throw new ArgumentException();
            consistsOfResPile = ReservedResPile.CreateFromSource(source: ref resSource);
            // The counters here don't matter as this person will be immediately transfered to RealPeople where this person's Mass and NumPeople will be transferred to the appropriate counters
            asVirtual = new(realPerson: this);
            deleted = new();
            
            locationCounters = LocationCounters.CreatePersonCounterByMagic(numPeople: this.RealPeopleStats.totalNumPeople);

            CurWorldManager.AddEnergyConsumer(energyConsumer: this);
            CurWorldManager.AddPerson(realPerson: this);
        }

        public void Arrived(RealPeople realPersonSource)
            => (activityCenter ?? throw new InvalidOperationException()).TakePersonFrom(realPersonSource: realPersonSource, realPerson: this);

        public void SetLocationCounters(LocationCounters locationCounters)
        {
            consistsOfResPile.LocationCounters = locationCounters;
            // Mass transfer is zero in the following line as the previous line did the mass transfer of this person already
            locationCounters.TransferFrom(source: this.locationCounters, mass: Mass.zero, numPeople: RealPeopleStats.totalNumPeople);
            this.locationCounters = locationCounters;
        }

        /// <param name="updateSkillsParams">if null, will use default update</param>
        public void Update(UpdateLocationParams updateLocationParams, UpdatePersonSkillsParams? updateSkillsParams)
        {
            lastNodeID = updateLocationParams.LastNodeID;
            ClosestNodeID = updateLocationParams.ClosestNodeID;
            var timeCoeff = ReqWatts.IsCloseTo(other: 0) ? Propor.empty : EnergyPropor;
            var elapsed = timeCoeff * CurWorldManager.Elapsed;
            var momentaryHappiness = CalculateMomentaryHappiness();
#warning implement update for unused skills
            updateSkillsParams ??= new UpdatePersonSkillsParams();
            RealPeopleStats = new
            (
                totalMass: consistsOfResPile.Mass,
                totalNumPeople: RealPeopleStats.totalNumPeople,
                timeCoefficient: timeCoeff,
                age: RealPeopleStats.age + elapsed,
                happiness: Score.BringCloser
                (
                    current: RealPeopleStats.happiness,
                    paramsOfChange: new Score.ParamsOfChange
                    (
                        target: momentaryHappiness,
                        elapsed: elapsed,
                        halvingDifferenceDuration: CurWorldConfig.happinessDifferenceHalvingDuration
                    )
                ),
                momentaryHappiness: momentaryHappiness,
                enjoyments: RealPeopleStats.enjoyments,
                talents: RealPeopleStats.talents,
                skills: RealPeopleStats.skills.Update
                (
                    newValues:
                        from updateSkillParams in updateSkillsParams
                        select
                        (
                            updateSkillParams.industryType,
                            Score.BringCloser
                            (
                                current: RealPeopleStats.skills[updateSkillParams.industryType],
                                paramsOfChange: updateSkillParams.paramsOfSkillChange
                            )
                        )
                )
            );
            if (IsInActivityCenter)
            {
                LastActivityTimes = LastActivityTimes.Update(key: activityCenter.ActivityType, newValue: RealPeopleStats.age);
                timeSinceActivitySearch += elapsed;
            }
        }

        private Score CalculateMomentaryHappiness()
        {
            if (!IsInActivityCenter)
                // When person is asleep, happiness doesn't change
                return RealPeopleStats.happiness;

            // TODO: include how much space they get, gravity preference, other's happiness maybe, etc.
            return activityCenter.PersonEnjoymentOfThis(person: asVirtual);
        }

        private UDouble ReqWatts
            => IsInActivityCenter ? reqWatts : 0;

        UDouble IEnergyConsumer.ReqWatts()
            => ReqWatts;

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
            if (newActivityCenter is not null)
                timeSinceActivitySearch = TimeSpan.Zero;
            if (activityCenter == newActivityCenter)
                return;

            activityCenter?.RemovePerson(person: asVirtual);
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
