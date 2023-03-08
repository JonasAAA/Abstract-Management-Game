using Game1.Inhabitants;
using Priority_Queue;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public abstract class ProductiveIndustry : Industry, IEnergyConsumer
    {
        [Serializable]
        public new abstract class Factory : Industry.Factory
        {
            public readonly IndustryType industryType;
            public readonly EnergyPriority energyPriority;
            public readonly UDouble reqSkillPerUnitSurface;

            protected Factory(IndustryType industryType, Color color, string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface)
                : base(name: name, color: color)
            {
                this.industryType = industryType;
                if ((industryType is IndustryType.PowerPlant && energyPriority != EnergyPriority.mostImportant)
                    || (industryType is not IndustryType.PowerPlant && energyPriority == EnergyPriority.mostImportant))
                    throw new ArgumentException();
                this.energyPriority = energyPriority;
                if (reqSkillPerUnitSurface.IsCloseTo(other: 0))
                    throw new ArgumentOutOfRangeException();
                this.reqSkillPerUnitSurface = reqSkillPerUnitSurface;
            }
        }

        [Serializable]
        public new abstract class Params : Industry.Params
        {
            public readonly IndustryType industryType;
            public readonly EnergyPriority energyPriority;
            public UDouble ReqSkill
                => state.ApproxSurfaceLength * factory.reqSkillPerUnitSurface;

            public override string TooltipText
                => $"""
                {base.TooltipText}
                {nameof(industryType)}: {industryType}
                {nameof(energyPriority)}: {energyPriority}
                {nameof(ReqSkill)}: {ReqSkill}
                """;

            private readonly Factory factory;

            public Params(IIndustryFacingNodeState state, Factory factory)
                : base(state: state, factory: factory)
            {
                this.factory = factory;

                industryType = factory.industryType;
                energyPriority = factory.energyPriority;
            }
        }

        [Serializable]
        private sealed class Employer : ActivityCenter
        {
            public Propor CurSkillPropor
                => (Propor)MyMathHelper.Min((UDouble)1, CurUnboundedSkillPropor);

            private UDouble CurUnboundedSkillPropor
                => realPeopleHere.Stats.totalProductivities[parameters.industryType] / parameters.ReqSkill;
            private readonly Params parameters;
            private Score desperationScore;
            /// <summary>
            /// What propor is each person working
            /// </summary>
            private Propor personalWorkingPropor;

            public Employer(Params parameters, IEnergyDistributor energyDistributor)
                : base(energyDistributor: energyDistributor, activityType: ActivityType.Working, energyPriority: parameters.energyPriority, state: parameters.state)
            {
                this.parameters = parameters;
                // TODO: could initialize to some other value
                desperationScore = Score.lowest;
                personalWorkingPropor = Propor.empty;
            }

            /// <summary>
            /// Returns industry working propor
            /// </summary>
            public Propor Update(bool isBusy, Propor industryEnergyPropor)
            {
                var finalIndustryEnergyPropor = MyMathHelper.Min(industryEnergyPropor, realPeopleHere.Stats.AllocEnergyPropor);
                personalWorkingPropor = isBusy switch
                {
                    true => Propor.Create
                    (
                        part: (UDouble)finalIndustryEnergyPropor,
                        whole: MyMathHelper.Max((UDouble)1, CurUnboundedSkillPropor)
                    )!.Value,
                    false => Propor.empty
                };
                UDouble totalHiredSkill = HiredSkill();
                if (totalHiredSkill >= parameters.ReqSkill)
                {
                    // if can, fire the worst people

                    SimplePriorityQueue<VirtualPerson, Score> allEmployeesPriorQueue = new();
                    foreach (var person in allPeople)
                        allEmployeesPriorQueue.Enqueue
                        (
                            item: person,
                            priority: CurrentEmploymentScore(person: person)
                        );

                    while (allEmployeesPriorQueue.Count > 0 && totalHiredSkill >= (UDouble)allEmployeesPriorQueue.First.Skills[parameters.industryType] + parameters.ReqSkill)
                    {
                        var person = allEmployeesPriorQueue.Dequeue();
                        totalHiredSkill = (UDouble)(totalHiredSkill - (double)person.Skills[parameters.industryType]);
                        RemovePerson(person: person);
                    }

                    Debug.Assert(HiredSkill() >= parameters.ReqSkill);
                    Debug.Assert(IsFull());
                }

                desperationScore = Score.BringCloser
                (
                    current: desperationScore,
                    paramsOfChange: new
                    (
                        target: IsFull() ? Score.lowest : Score.WeightedAverageOfTwo
                        (
                            score1: Score.Create(propor: OpenSpacePropor()),
                            score2: Score.highest,
                            // TODO: get rid of hard-coded constant
                            score1Propor: (Propor).5
                        ),
                        elapsed: CurWorldManager.Elapsed,
                        // TODO: get rid of hard-coded constant
                        halvingDifferenceDuration: TimeSpan.FromSeconds(20)
                    )
                );
                return finalIndustryEnergyPropor * CurSkillPropor;
            }

            public override bool IsFull()
                => OpenSpacePropor().IsCloseTo(other: Propor.empty);

            public override Score PersonEnjoymentOfThis(VirtualPerson person)
                => person.Enjoyments[parameters.industryType];

            public override bool IsPersonSuitable(VirtualPerson person)
            {
                if (IsPersonQueuedOrHere(person: person))
                    return true;

                return NewEmploymentScore(person: person) >= CurWorldConfig.minAcceptablePersonScore;
            }

            protected override UpdatePersonSkillsParams UpdatePersonSkillsParams
                => new()
                {
                    (
                        industryType: parameters.industryType,
                        paramsOfSkillChange: new Score.ParamsOfChange
                        (
                            target: Score.highest,
                            elapsed: CurWorldManager.Elapsed * personalWorkingPropor,
                            // TODO: get rid of hard-coded constant
                            halvingDifferenceDuration: TimeSpan.FromSeconds(20)
                        )
                    )
                };

            public override bool CanPersonLeave(VirtualPerson person)
                => true;

            public string GetInfo()
                => $"""
                have {realPeopleHere.Stats.totalProductivities[parameters.industryType] / parameters.ReqSkill * 100:0.}% skill
                desperation {(UDouble)desperationScore * 100:0.}%
                {PeopleHereStats}
                travel here {allPeople.Count - PeopleHereStats.totalNumPeople}
                
                """;

            private UDouble HiredSkill()
                => allPeople.Sum(person => (UDouble)person.Skills[parameters.industryType]);

            private Propor OpenSpacePropor()
                => Propor.Create(part: HiredSkill(), whole: parameters.ReqSkill) switch
                {
                    Propor hiredPropor => hiredPropor.Opposite(),
                    null => Propor.empty
                };

            private Score NewEmploymentScore(VirtualPerson person)
                => Score.WeightedAverage
                (
                    (weight: CurWorldConfig.personJobEnjoymentWeight, score: PersonEnjoymentOfThis(person: person)),
                    (weight: CurWorldConfig.personTalentWeight, score: person.Talents[parameters.industryType]),
                    (weight: CurWorldConfig.personSkillWeight, score: person.Skills[parameters.industryType]),
                    (weight: CurWorldConfig.jobDesperationWeight, score: desperationScore),
                    (weight: CurWorldConfig.personToJobDistWeight, score: (this as IPersonFacingActivityCenter).DistanceToHereAsPerson(person: person))
                );

            private Score CurrentEmploymentScore(VirtualPerson person)
            {
                if (!IsPersonQueuedOrHere(person: person))
                    throw new ArgumentException();
                return Score.WeightedAverage
                (
                    (weight: CurWorldConfig.personJobEnjoymentWeight, score: PersonEnjoymentOfThis(person: person)),
                    (weight: CurWorldConfig.personTalentWeight, score: person.Talents[parameters.industryType]),
                    (weight: CurWorldConfig.personSkillWeight, score: person.Skills[parameters.industryType])
                );
            }
        }

        public override RealPeopleStats Stats
            => employer.PeopleHereStats;

        protected Propor CurSkillPropor
            => employer.CurSkillPropor;

        protected readonly HistoricRounder reqEnergyHistoricRounder;

        private readonly Params parameters;
        private readonly Employer employer;
        private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
        /// <summary>
        /// How much of REQUESTED energy is given. So if request 0, get 0, the proportion is full
        /// </summary>
        private Propor allocEnergyPropor;

        protected ProductiveIndustry(Params parameters, Building? building)
            : base(parameters: parameters, building: building)
        {
            this.parameters = parameters;
            reqEnergyHistoricRounder = new();
            employer = new(parameters: parameters, energyDistributor: combinedEnergyConsumer);
            allocEnergyPropor = Propor.empty;
            electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: parameters.state.LocationCounters);

            combinedEnergyConsumer.AddEnergyConsumer(energyConsumer: this);
        }

        protected override void UpdatePeopleInternal()
            => employer.UpdatePeople();

        // TODO: Compute this value only once per frame
        protected virtual BoolWithExplanationIfFalse IsBusy()
            => CanBeBusy;

        private BoolWithExplanationIfFalse CanBeBusy
            => BoolWithExplanationIfFalse.Create
            (
                value: !parameters.state.TooManyResStored,
                explanationIfFalse: """
                    the planet contains
                    unwanted resources
                    """
            );

        protected sealed override Industry InternalUpdate()
        {
            parameters.state.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
            var workingPropor = employer.Update(isBusy: (bool)IsBusy(), industryEnergyPropor: allocEnergyPropor);
            return (bool)CanBeBusy ? InternalUpdate(workingPropor: workingPropor) : this;
        }

        protected abstract Industry InternalUpdate(Propor workingPropor);

        protected override void Delete()
        {
            employer.Delete();
            base.Delete();
        }

        public sealed override string GetInfo()
            => CurWorldManager.Overlay.SwitchExpression
            (
                singleResCase: resInd => "",
                allResCase: () => "",
                powerCase: () => $"have {allocEnergyPropor * 100.0:0.}% of required energy\n",
                peopleCase: () => employer.GetInfo()
            ) + $"{parameters.name}\n" + IsBusy().SwitchExpression
            (
                trueCase: GetBusyInfo,
                falseCase: explanation => "Idle because\n" + explanation
            );

        protected abstract string GetBusyInfo();

        protected abstract UDouble ReqWatts();

        EnergyPriority IEnergyConsumer.EnergyPriority
            => IsBusy().SwitchExpression
            (
                trueCase: () => parameters.energyPriority,
                falseCase: () => EnergyPriority.leastImportant
            );

        NodeID IEnergyConsumer.NodeID
            => parameters.state.NodeID;

        void IEnergyConsumer.ConsumeEnergyFrom(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
        {
            electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
            allocEnergyPropor = MyMathHelper.CreatePropor(part: electricalEnergy, whole: ReqEnergy());
        }

        private ElectricalEnergy ReqEnergy()
            => ElectricalEnergy.CreateFromJoules
            (
                valueInJ: reqEnergyHistoricRounder.Round
                (
                    value: (decimal)ReqWatts() * (decimal)CurWorldManager.Elapsed.TotalSeconds,
                    curTime: CurWorldManager.CurTime
                )
            );

        ElectricalEnergy IEnergyConsumer.ReqEnergy()
            => ReqEnergy();
    }
}