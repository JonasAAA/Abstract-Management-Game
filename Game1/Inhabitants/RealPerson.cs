﻿using Game1.Delegates;
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
    public sealed class RealPerson : IEnergyConsumer, IHasMass
    {
        [Serializable]
        public readonly record struct UpdateParams(NodeID LastNodeID, NodeID ClosestNodeID);

        public static void GeneratePersonByMagic(NodeID nodeID, ReservedResPile resSource, RealPeople personDestin)
            => personDestin.AddByMagic
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
                    consistsOfResAmounts: resAmountsPerPerson
                )
            );

        public static void GenerateChild(VirtualPerson parent1, VirtualPerson parent2, NodeID nodeID, ReservedResPile resSource, RealPeople parentSource, RealPeople childDestin)
        {
            if (!parentSource.Contains(virtualPerson: parent1) || !parentSource.Contains(virtualPerson: parent2))
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
                    consistsOfResAmounts: resAmountsPerPerson
                )
            );

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

        public readonly ReadOnlyDictionary<IndustryType, Score> enjoyments;
        public readonly ReadOnlyDictionary<IndustryType, Score> talents;
        public readonly Dictionary<IndustryType, Score> skills;

        public NodeID? ActivityCenterNodeID
            => activityCenter?.NodeID;
        public NodeID ClosestNodeID { get; private set; }
        public Propor EnergyPropor { get; private set; }
        public IReadOnlyDictionary<ActivityType, TimeSpan> LastActivityTimes
            => lastActivityTimes;
        public ulong Mass
            => consistsOfResPile.Mass;
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
            => activityCenter is not null && activityCenter.IsPersonHere(person: asVirtual);
        /// <summary>
        /// is null if just been let go from activity center
        /// </summary>
        private IPersonFacingActivityCenter? activityCenter;
        private TimeSpan timeSinceActivitySearch;
        private readonly Dictionary<ActivityType, TimeSpan> lastActivityTimes;
        private NodeID lastNodeID;
        private readonly Event<IDeletedListener> deleted;
        private readonly ReservedResPile consistsOfResPile;

        private RealPerson(NodeID nodeID, Dictionary<IndustryType, Score> enjoyments, Dictionary<IndustryType, Score> talents, Dictionary<IndustryType, Score> skills, UDouble reqWatts, TimeSpan seekChangeTime, [DisallowNull] ReservedResPile? resSource, ResAmounts consistsOfResAmounts)
        {
            lastNodeID = nodeID;
            ClosestNodeID = nodeID;
            this.enjoyments = new(enjoyments);
            this.talents = new(talents);
            this.skills = new(skills);

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
            consistsOfResPile = ReservedResPile.Create(source: ref resSource);
            asVirtual = new(realPerson: this);
            deleted = new();

            CurWorldManager.AddEnergyConsumer(energyConsumer: this);
            CurWorldManager.AddPerson(person: this);
        }

        public void Arrived(RealPeople personSource)
            => (activityCenter ?? throw new InvalidOperationException()).TakePersonFrom(personSource: personSource, person: this);

        // TODO: could take list of required changes as parameter.
        // This would make it so that the only way to change person is to call Update()
        /// <param name="update">if null, will use default update</param>
        public void Update(UpdateParams updateParams, Action? update)
        {
            lastNodeID = updateParams.LastNodeID;
            ClosestNodeID = updateParams.ClosestNodeID;
            if (IsInActivityCenter)
            {
                lastActivityTimes[activityCenter.ActivityType] = CurWorldManager.CurTime;
                timeSinceActivitySearch += CurWorldManager.Elapsed;
            }
            if (update is null)
                IActivityCenter.UpdatePersonDefault(person: this);
            else
                update.Invoke();
        }

        UDouble IEnergyConsumer.ReqWatts()
            => reqWatts;

        void IEnergyConsumer.ConsumeEnergy(Propor energyPropor)
            => EnergyPropor = energyPropor;

        public bool IfSeeksNewActivity()
            => activityCenter is null || timeSinceActivitySearch >= seekChangeTime && activityCenter.CanPersonLeave(person: asVirtual);

        public IPersonFacingActivityCenter ChooseActivityCenter(IEnumerable<IPersonFacingActivityCenter> activityCenters)
        {
            if (!IfSeeksNewActivity())
                throw new InvalidOperationException();

            var bestActivityCenter = activityCenters.ArgMaxOrDefault(activityCenter => activityCenter.PersonScoreOfThis(person: asVirtual));
            if (bestActivityCenter is null)
                throw new ArgumentException();
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

#if DEBUG
#pragma warning disable CA1821 // Remove empty Finalizers
        ~RealPerson()
#pragma warning restore CA1821 // Remove empty Finalizers
            => throw new();
#endif
    }
}
