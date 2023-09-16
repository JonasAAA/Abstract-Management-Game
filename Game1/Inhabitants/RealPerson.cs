using Game1.Collections;
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
    public sealed class RealPerson : IWithRealPeopleStats
    {
        public static void GeneratePersonByMagic(NodeID closestNodeID, ResPile resSource, RealPeople childDestin)
            => childDestin.AddByMagic
            (
                realPerson: new
                (
                    realPeopleStats: RealPeopleStats.ForNewPerson
                    (
                        totalMass: resSource.Amount.Mass(),
                        reqWatts: C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts),
                        age: TimeSpan.FromSeconds(C.Random(min: 0, max: 200)),
                        startingHappiness: CurWorldConfig.startingHappiness,
                        enjoyments: new(selector: indType => Score.GenerateRandom()),
                        talents: new(selector: indType => Score.GenerateRandom()),
                        skills: new(selector: indType => Score.GenerateRandom())
                    ),
                    closestNodeID: closestNodeID,
                    seekChangeTime: C.Random(min: CurWorldConfig.personMinSeekChangeTime, max: CurWorldConfig.personMaxSeekChangeTime),
                    resSource: resSource,
                    consistsOfResAmounts: resAmountsPerPerson
                )
            );

        public static void GenerateChild(VirtualPerson parent1, VirtualPerson parent2, NodeID closestNodeID, ResPile resSource, RealPeople parentSource, RealPeople childDestin)
        {
            if (!parentSource.Contains(person: parent1) || !parentSource.Contains(person: parent2))
                throw new ArgumentException();

            UInt128 childReqWatts = MyMathHelper.Round
            (
                value: CurWorldConfig.parentContribToChildPropor * (parent1.ReqWatts + parent2.ReqWatts) * (UDouble).5
                    + CurWorldConfig.parentContribToChildPropor.Opposite() * C.Random(min: CurWorldConfig.personMinReqWatts, max: CurWorldConfig.personMaxReqWatts)
            );
            Debug.Assert(CurWorldConfig.personMinReqWatts <= childReqWatts && childReqWatts <= CurWorldConfig.personMaxReqWatts);
            childDestin.AddByMagic
            (
                realPerson: new
                (
                    realPeopleStats: RealPeopleStats.ForNewPerson
                    (
                        totalMass: resSource.Amount.Mass(),
                        age: TimeSpan.Zero,
                        reqWatts: childReqWatts,
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
                    closestNodeID: closestNodeID,
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
        public static readonly AllResAmounts resAmountsPerPerson = new
            (
                resAmounts: new List<ResAmount<IResource>>()
                {
                    new(res: RawMaterial.GetAndAddToCurResConfigIfNeeded(curResConfig: CurResConfig, ind: 0), amount: 10)
                }
            );

        public readonly VirtualPerson asVirtual;
        
        public NodeID? ActivityCenterNodeID
            => activityCenter?.NodeID;
        public NodeID ClosestNodeID { get; private set; }
        public EnumDict<ActivityType, TimeSpan> LastActivityTimes { get; private set; }
        public RealPeopleStats Stats { get; private set; }
        public readonly UInt128 reqWatts;
        public readonly TimeSpan seekChangeTime;

        [MemberNotNullWhen(returnValue: true, member: nameof(activityCenter))]
        private bool IsInActivityCenter
            => activityCenter?.IsPersonHere(person: asVirtual) ?? false;
        /// <summary>
        /// is null if just been let go from activity center
        /// </summary>
        private IPersonFacingActivityCenter? activityCenter;
        private TimeSpan timeSinceActivitySearch;
        private readonly ResPile consistsOfResPile;
        private LocationCounters locationCounters;
        
        private RealPerson(RealPeopleStats realPeopleStats, NodeID closestNodeID, TimeSpan seekChangeTime, ResPile resSource, AllResAmounts consistsOfResAmounts)
        {
            Stats = realPeopleStats;
            ClosestNodeID = closestNodeID;

            activityCenter = null;

            if (seekChangeTime < CurWorldConfig.personMinSeekChangeTime || seekChangeTime > CurWorldConfig.personMaxSeekChangeTime)
                throw new ArgumentOutOfRangeException();
            this.seekChangeTime = seekChangeTime;
            timeSinceActivitySearch = seekChangeTime;
            LastActivityTimes = new(selector: activityType => TimeSpan.MinValue / 3);
            if (resSource.Amount != consistsOfResAmounts)
                throw new ArgumentException();
            consistsOfResPile = resSource;
            // The counters here don't matter as this person will be immediately transfered to RealPeople where this person's Mass and NumPeople will be transferred to the appropriate counters
            asVirtual = new(realPerson: this);
            
            locationCounters = LocationCounters.CreateCounterByMagic<NumPeople>(amount: Stats.totalNumPeople);

            CurWorldManager.AddPerson(realPerson: this);
        }

        public void Arrived(RealPeople realPersonSource)
            => (activityCenter ?? throw new InvalidOperationException()).TakePersonFrom(realPersonSource: realPersonSource, realPerson: this);

        public void ChangeLocation(ThermalBody newThermalBody, NodeID closestNodeID)
        {
            consistsOfResPile.ChangeLocation(newThermalBody: newThermalBody);
            // Resource transfer is zero in the following line as the previous line did the resAmounts transfer of this person already
            newThermalBody.locationCounters.TransferFrom(source: locationCounters, amount: Stats.totalNumPeople);
            locationCounters = newThermalBody.locationCounters;

            ClosestNodeID = closestNodeID;
        }

        public void UpdateAllocEnergyPropor(Propor newAllocEnergyPropor)
            => Stats = Stats with { AllocEnergyPropor = newAllocEnergyPropor };

        /// <param Name="updateSkillsParams">if null, will use default update</param>
        public void Update(UpdatePersonSkillsParams? updateSkillsParams, Propor allocEnergyPropor)
        {
            var timeCoeff = Stats.totalReqWatts == 0 ? Propor.empty : allocEnergyPropor;
            var elapsed = timeCoeff * CurWorldManager.Elapsed;
            var momentaryHappiness = CalculateMomentaryHappiness();
#warning implement update for unused skills
            updateSkillsParams ??= new UpdatePersonSkillsParams();
            Stats = new
            (
                totalMass: consistsOfResPile.Amount.Mass(),
                totalNumPeople: Stats.totalNumPeople,
                totalReqWatts: Stats.totalReqWatts,
                timeCoefficient: timeCoeff,
                age: Stats.age + elapsed,
                allocEnergyPropor: allocEnergyPropor,
                happiness: Score.BringCloser
                (
                    current: Stats.happiness,
                    paramsOfChange: new Score.ParamsOfChange
                    (
                        target: momentaryHappiness,
                        elapsed: elapsed,
                        halvingDifferenceDuration: CurWorldConfig.happinessDifferenceHalvingDuration
                    )
                ),
                momentaryHappiness: momentaryHappiness,
                enjoyments: Stats.enjoyments,
                talents: Stats.talents,
                skills: Stats.skills.Update
                (
                    newValues:
                        from updateSkillParams in updateSkillsParams
                        select
                        (
                            updateSkillParams.industryType,
                            Score.BringCloser
                            (
                                current: Stats.skills[updateSkillParams.industryType],
                                paramsOfChange: updateSkillParams.paramsOfSkillChange
                            )
                        )
                )
            );
            if (IsInActivityCenter)
            {
                LastActivityTimes = LastActivityTimes.Update(key: activityCenter.ActivityType, newValue: Stats.age);
                timeSinceActivitySearch += elapsed;
            }
        }

        private Score CalculateMomentaryHappiness()
        {
            if (!IsInActivityCenter)
                // When person is asleep, happiness doesn't change
                return Stats.happiness;

            // TODO: include how much space they get, gravity preference, other's happiness maybe, etc.
            return activityCenter.PersonEnjoymentOfThis(person: asVirtual);
        }

        public bool IfSeeksNewActivity()
            => activityCenter is null || (timeSinceActivitySearch >= seekChangeTime && activityCenter.CanPersonLeave(person: asVirtual));

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
            ) ?? throw new ArgumentException("have no place to go");
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

#if DEBUG2
#pragma warning disable CA1821 // Remove empty Finalizers
        ~RealPerson()
#pragma warning restore CA1821 // Remove empty Finalizers
            => throw new();
#endif
    }
}
