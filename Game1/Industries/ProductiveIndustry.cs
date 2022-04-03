using Game1.ChangingValues;
using Game1.PrimitiveTypeWrappers;
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
            public readonly UFloat reqSkillPerUnitSurface;

            protected Params(IndustryType industryType, string name, EnergyPriority energyPriority, UFloat reqSkillPerUnitSurface, string explanation)
                : base(name: name, explanation: explanation)
            {
                this.industryType = industryType;
                if ((industryType is IndustryType.PowerPlant && energyPriority != EnergyPriority.minimal)
                    || (industryType is not IndustryType.PowerPlant && energyPriority == EnergyPriority.minimal))
                    throw new ArgumentException();
                this.energyPriority = energyPriority;
                if (reqSkillPerUnitSurface <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqSkillPerUnitSurface = reqSkillPerUnitSurface;
            }

            public abstract override ProductiveIndustry MakeIndustry(NodeState state);
        }

        [Serializable]
        private class Employer : ActivityCenter
        {
            public double CurSkillPropor { get; private set; }

            private readonly Params parameters;
            private readonly IReadOnlyChangingUFloat reqSkill;
            // must be >= 0
            private TimeSpan avgVacancyDuration;
            private double curUnboundedSkillPropor, workingPropor;

            public Employer(EnergyPriority energyPriority, NodeState state, Params parameters)
                : base(activityType: ActivityType.Working, energyPriority: energyPriority, state: state)
            {
                this.parameters = parameters;

                reqSkill = state.approxSurfaceLength * parameters.reqSkillPerUnitSurface;
                CurSkillPropor = 0;
                curUnboundedSkillPropor = 0;
                avgVacancyDuration = TimeSpan.Zero;
                workingPropor = 0;
            }

            public void StartUpdate()
            {
                double totalHiredSkill = HiredSkill();
                if (totalHiredSkill >= reqSkill.Value)
                {
                    // if can, fire the worst people
                    double oldOpenSpace = OpenSpace();

                    SimplePriorityQueue<Person, double> allEmployeesPriorQueue = new();
                    foreach (var person in allPeople)
                        allEmployeesPriorQueue.Enqueue
                        (
                            item: person,
                            priority: CurrentEmploymentScore(person: person)
                        );

                    while (allEmployeesPriorQueue.Count > 0 && totalHiredSkill - allEmployeesPriorQueue.First.skills[parameters.industryType] >= reqSkill.Value)
                    {
                        var person = allEmployeesPriorQueue.Dequeue();
                        totalHiredSkill -= person.skills[parameters.industryType];
                        RemovePerson(person: person);
                    }

                    double curOpenSpace = OpenSpace();
                    if (oldOpenSpace is double.NegativeInfinity)
                        avgVacancyDuration = TimeSpan.Zero;
                    else
                        avgVacancyDuration *= oldOpenSpace / curOpenSpace;

                    Debug.Assert(HiredSkill() >= reqSkill.Value);
                }

                if (IsFull())
                    avgVacancyDuration = TimeSpan.Zero;
                else
                    avgVacancyDuration += CurWorldManager.Elapsed;
            }

            public void EndUpdate()
            {
                curUnboundedSkillPropor = peopleHere.Sum(person => person.skills[parameters.industryType]) / reqSkill.Value;
                CurSkillPropor = MathHelper.Min(1, curUnboundedSkillPropor);
            }

            public override bool IsFull()
                => OpenSpace() is double.NegativeInfinity;

            public override double PersonScoreOfThis(Person person)
                => CurWorldConfig.personMomentumCoeff * (IsPersonHere(person: person) ? 1 : 0)
                + (.9 * person.enjoyments[parameters.industryType] + .1 * DistanceToHere(person: person)) * (1 - CurWorldConfig.personMomentumCoeff);

            public override bool IsPersonSuitable(Person person)
            {
                if (IsPersonQueuedOrHere(person: person))
                    return true;

                return NewEmploymentScore(person: person) >= CurWorldConfig.minAcceptablePersonScore;
            }

            public override void UpdatePerson(Person person)
            {
                if (!C.IsInSuitableRange(value: workingPropor))
                    throw new ArgumentOutOfRangeException();

                Debug.Assert(C.IsInSuitableRange(value: person.skills[parameters.industryType]));
                person.skills[parameters.industryType] = 1 - (1 - person.skills[parameters.industryType]) * MathHelper.Pow(1 - person.talents[parameters.industryType], CurWorldManager.Elapsed.TotalSeconds * workingPropor * CurWorldConfig.personTimeSkillCoeff);
                Debug.Assert(C.IsInSuitableRange(value: person.skills[parameters.industryType]));
            }

            public override bool CanPersonLeave(Person person)
                => true;

            public void SetEnergyPropor(double energyPropor)
            {
                if (!C.IsInSuitableRange(value: energyPropor))
                    throw new ArgumentOutOfRangeException();
                workingPropor = energyPropor / MathHelper.Max(1, curUnboundedSkillPropor);
            }

            public string GetInfo()
                => $"have {peopleHere.Sum(person => person.skills[parameters.industryType]) / reqSkill.Value * 100:0.}% skill\ndesperation {(IsFull() ? 0 : Desperation() * 100):0.}%\nemployed {peopleHere.Count}\n";

            private double HiredSkill()
                => allPeople.Sum(person => person.skills[parameters.industryType]);

            private double OpenSpace()
            {
                double hiredSkill = HiredSkill();
                if (hiredSkill >= reqSkill.Value)
                    return double.NegativeInfinity;
                double result = 1 - hiredSkill / reqSkill.Value;
                Debug.Assert(C.IsInSuitableRange(result));
                return result;
            }

            private double Desperation()
            {
                Debug.Assert(avgVacancyDuration >= TimeSpan.Zero);
                double openSpace = OpenSpace();
                if (openSpace is double.NegativeInfinity)
                    return double.NegativeInfinity;
                return MathHelper.Tanh(avgVacancyDuration.TotalSeconds * openSpace * CurWorldConfig.jobVacDespCoeff);
            }

            // each parameter must be between 0 and 1 or double.NegativeInfinity
            // larger means this pair is more likely to work
            // must be between 0 and 1 or double.NegativeInfinity
            private double NewEmploymentScore(Person person)
                => CurWorldConfig.personJobEnjoymentCoeff * PersonScoreOfThis(person: person)
                + CurWorldConfig.personTalentCoeff * person.talents[parameters.industryType]
                + CurWorldConfig.personSkillCoeff * person.skills[parameters.industryType]
                + CurWorldConfig.jobDesperationCoeff * Desperation()
                + CurWorldConfig.playerToJobDistCoeff * DistanceToHere(person: person);

            private double CurrentEmploymentScore(Person person)
            {
                if (!IsPersonQueuedOrHere(person: person))
                    throw new ArgumentException();
                return CurWorldConfig.personJobEnjoymentCoeff * PersonScoreOfThis(person: person)
                    + CurWorldConfig.personTalentCoeff * person.talents[parameters.industryType]
                    + CurWorldConfig.personSkillCoeff * person.skills[parameters.industryType];
            }
        }

        public EnergyPriority EnergyPriority
            => IsBusy() switch
            {
                true => parameters.energyPriority,
                false => EnergyPriority.maximal
            };

        Vector2 IEnergyConsumer.NodePos
            => state.position;

        public override IEnumerable<Person> PeopleHere
            => employer.PeopleHere;
        
        protected bool CanStartProduction { get; private set; }
        protected double CurSkillPropor
            => employer.CurSkillPropor;

        private readonly Params parameters;
        private readonly Employer employer;
        private double energyPropor;

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
            energyPropor = 0;

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

        protected abstract Industry InternalUpdate(double workingPropor);

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

        public abstract double ReqWatts();

        void IEnergyConsumer.ConsumeEnergy(double energyPropor)
        {
            if (!C.IsInSuitableRange(value: energyPropor))
                throw new ArgumentOutOfRangeException();
            this.energyPropor = energyPropor;
            employer.SetEnergyPropor(energyPropor: energyPropor);
        }
    }
}