using Game1.ChangingValues;
using Priority_Queue;

using static Game1.WorldManager;

namespace Game1.Industries
{
    [Serializable]
    public abstract class ProductiveIndustry : Industry, IEnergyConsumer
    {
        [Serializable]
        public abstract new class Params : Industry.Params
        {
            public readonly IndustryType industryType;
            public readonly EnergyPriority energyPriority;
            public readonly UDouble reqSkillPerUnitSurface;

            protected Params(IndustryType industryType, string name, EnergyPriority energyPriority, UDouble reqSkillPerUnitSurface, string explanation)
                : base(name: name, explanation: explanation)
            {
                this.industryType = industryType;
                if ((industryType is IndustryType.PowerPlant && energyPriority != EnergyPriority.minimal)
                    || (industryType is not IndustryType.PowerPlant && energyPriority == EnergyPriority.minimal))
                    throw new ArgumentException();
                this.energyPriority = energyPriority;
                if (reqSkillPerUnitSurface.IsCloseTo(other: 0))
                    throw new ArgumentOutOfRangeException();
                this.reqSkillPerUnitSurface = reqSkillPerUnitSurface;
            }

            public abstract override ProductiveIndustry MakeIndustry(NodeState state);
        }

        [Serializable]
        private class Employer : ActivityCenter
        {
            public Propor CurSkillPropor { get; private set; }

            private readonly Params parameters;
            private readonly IReadOnlyChangingUDouble reqSkill;
            private Score desperationScore;
            private UDouble curUnboundedSkillPropor;
            private Propor workingPropor;

            public Employer(EnergyPriority energyPriority, NodeState state, Params parameters)
                : base(activityType: ActivityType.Working, energyPriority: energyPriority, state: state)
            {
                this.parameters = parameters;

                reqSkill = state.approxSurfaceLength * parameters.reqSkillPerUnitSurface;
                CurSkillPropor = Propor.empty;
                curUnboundedSkillPropor = 0;
                // TODO: could initialize to some other value
                desperationScore = Score.lowest;
                workingPropor = Propor.empty;
            }

            public void StartUpdate()
            {
                UDouble totalHiredSkill = HiredSkill();
                if (totalHiredSkill >= reqSkill.Value)
                {
                    // if can, fire the worst people

                    SimplePriorityQueue<Person, Score> allEmployeesPriorQueue = new();
                    foreach (var person in allPeople)
                        allEmployeesPriorQueue.Enqueue
                        (
                            item: person,
                            priority: CurrentEmploymentScore(person: person)
                        );

                    while (allEmployeesPriorQueue.Count > 0 && totalHiredSkill >= (UDouble)allEmployeesPriorQueue.First.skills[parameters.industryType] + reqSkill.Value)
                    {
                        var person = allEmployeesPriorQueue.Dequeue();
                        totalHiredSkill = (UDouble)((double)totalHiredSkill - (double)person.skills[parameters.industryType]);
                        RemovePerson(person: person);
                    }

                    Debug.Assert(HiredSkill() >= reqSkill.Value);
                }
                
                desperationScore = Score.BringCloser
                (
                    current: desperationScore,
                    target: IsFull() ? Score.lowest : Score.WightedAverageOfTwo
                    (
                        score1: (Score)OpenSpacePropor(),
                        score2: Score.highest,
                        // TODO: get rid of hard-coded constant
                        score1Propor: (Propor).5
                    ),
                    elapsed: CurWorldManager.Elapsed,
                    // TODO: get rid of hard-coded constant
                    halvingDifferenceDuration: TimeSpan.FromSeconds(20)
                );
            }

            public void EndUpdate()
            {
                curUnboundedSkillPropor = peopleHere.Sum(person => (UDouble)person.skills[parameters.industryType]) / reqSkill.Value;
                CurSkillPropor = (Propor)MyMathHelper.Min((UDouble)1, curUnboundedSkillPropor);
            }

            public override bool IsFull()
                => OpenSpacePropor().IsCloseTo(other: Propor.empty);

            public override Score PersonScoreOfThis(Person person)
                => Score.WightedAverageOfTwo
                (
                    score1: IsPersonHere(person: person) ? Score.highest : Score.lowest,
                    score2: Score.WeightedAverage
                    (
                        (weight: 9, score: person.enjoyments[parameters.industryType]),
                        (weight: 1, score: DistanceToHere(person: person))
                    ),
                    score1Propor: CurWorldConfig.personMomentumPropor
                );

            public override bool IsPersonSuitable(Person person)
            {
                if (IsPersonQueuedOrHere(person: person))
                    return true;

                return NewEmploymentScore(person: person) >= CurWorldConfig.minAcceptablePersonScore;
            }

            public override void UpdatePerson(Person person)
                => person.skills[parameters.industryType] = Score.BringCloser
                (
                    current: person.skills[parameters.industryType],
                    target: Score.highest,
                    elapsed: CurWorldManager.Elapsed * workingPropor,
                    // TODO: get rid of hard-coded constant
                    halvingDifferenceDuration: TimeSpan.FromSeconds(20)
                );
            
            public override bool CanPersonLeave(Person person)
                => true;

            public void SetEnergyPropor(Propor energyPropor)
                => workingPropor = Propor.Create((UDouble)energyPropor, MyMathHelper.Max((UDouble)1, curUnboundedSkillPropor)).Value;

            public string GetInfo()
                => $"have {peopleHere.Sum(person => (UDouble)person.skills[parameters.industryType]) / reqSkill.Value * 100:0.}% skill\ndesperation {(IsFull() ? 0 : (UDouble)desperationScore * 100):0.}%\nemployed {peopleHere.Count}\n";

            private UDouble HiredSkill()
                => allPeople.Sum(person => (UDouble)person.skills[parameters.industryType]);

            private Propor OpenSpacePropor()
                => Propor.Create(part: HiredSkill(), whole: reqSkill.Value) switch
                {
                    Propor hiredPropor => hiredPropor.Opposite(),
                    null => Propor.empty
                };

            private Score NewEmploymentScore(Person person)
                => Score.WeightedAverage
                (
                    (weight: CurWorldConfig.personJobEnjoymentWeight, score: PersonScoreOfThis(person: person)),
                    (weight: CurWorldConfig.personTalentWeight, score: person.talents[parameters.industryType]),
                    (weight: CurWorldConfig.personSkillWeight, score: person.skills[parameters.industryType]),
                    (weight: CurWorldConfig.jobDesperationWeight, score: desperationScore),
                    (weight: CurWorldConfig.playerToJobDistWeight, score: DistanceToHere(person: person))
                );

            private Score CurrentEmploymentScore(Person person)
            {
                if (!IsPersonQueuedOrHere(person: person))
                    throw new ArgumentException();
                return Score.WeightedAverage
                    (
                        (weight: CurWorldConfig.personJobEnjoymentWeight, score: PersonScoreOfThis(person: person)),
                        (weight: CurWorldConfig.personTalentWeight, score: person.talents[parameters.industryType]),
                        (weight: CurWorldConfig.personSkillWeight, score: person.skills[parameters.industryType])
                    );
            }
        }

        public EnergyPriority EnergyPriority
            => IsBusy() switch
            {
                true => parameters.energyPriority,
                false => EnergyPriority.maximal
            };

        MyVector2 IEnergyConsumer.NodePos
            => state.position;

        public override IEnumerable<Person> PeopleHere
            => employer.PeopleHere;
        
        protected bool CanStartProduction { get; private set; }
        protected Propor CurSkillPropor
            => employer.CurSkillPropor;

        private readonly Params parameters;
        private readonly Employer employer;
        private Propor energyPropor;

        protected ProductiveIndustry(NodeState state, Params parameters)
            : base(state: state)
        {
            this.parameters = parameters;

            employer = new
            (
                state: state,
                energyPriority: parameters.energyPriority,
                parameters: parameters
            );

            CanStartProduction = true;
            energyPropor = Propor.empty;

            CurWorldManager.AddEnergyConsumer(energyConsumer: this);
        }

        protected abstract bool IsBusy();

        protected override Industry InternalUpdate()
        {
            employer.StartUpdate();

            var result = InternalUpdate(workingPropor: energyPropor * CurSkillPropor);

            employer.EndUpdate();

            return result;
        }

        protected abstract Industry InternalUpdate(Propor workingPropor);

        protected override void Delete()
        {
            employer.Delete();
            base.Delete();
        }

        public override string GetInfo()
            => CurWorldManager.Overlay.SwitchExpression
            (
                singleResCase: resInd => "",
                allResCase: () => "",
                powerCase: () => "",
                peopleCase: () => employer.GetInfo()
            );

        public abstract UDouble ReqWatts();

        void IEnergyConsumer.ConsumeEnergy(Propor energyPropor)
        {
            this.energyPropor = energyPropor;
            employer.SetEnergyPropor(energyPropor: energyPropor);
        }
    }
}