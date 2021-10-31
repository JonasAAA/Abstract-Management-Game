using Game1.UI;
using Microsoft.Xna.Framework;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [DataContract]
    public abstract class Industry : IEnergyConsumer
    {
        // all fields and properties in this and derived classes must have unchangeable state
        [DataContract]
        public abstract class Params
        {
            [DataMember]
            public readonly IndustryType industryType;
            [DataMember]
            public readonly string name;
            [DataMember]
            public readonly ulong energyPriority;
            [DataMember]
            public readonly double reqSkill;
            [DataMember]
            public readonly string explanation;

            public Params(IndustryType industryType, string name, ulong energyPriority, double reqSkill, string explanation)
            {
                this.industryType = industryType;
                this.name = name;
                if ((industryType is IndustryType.PowerPlant && energyPriority is not 0)
                    || (industryType is not IndustryType.PowerPlant && energyPriority is 0))
                    throw new ArgumentException();
                this.energyPriority = energyPriority;
                if (reqSkill <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqSkill = reqSkill;
                this.explanation = explanation;
            }

            public Industry MakeAndInitIndustry(NodeState state)
            {
                var industry = MakeIndustry(state: state);
                industry.Initialize();
                return industry;
            }

            protected abstract Industry MakeIndustry(NodeState state);
        }

        [DataContract]
        private class Employer : ActivityCenter
        {
            [DataMember]
            public double CurSkillPropor { get; private set; }

            [DataMember]
            private readonly Params parameters;
            // must be >= 0
            [DataMember]
            private TimeSpan avgVacancyDuration;
            [DataMember]
            private double curUnboundedSkillPropor, workingPropor;

            public Employer(ulong energyPriority, NodeState state, Params parameters)
                : base(activityType: ActivityType.Working, energyPriority: energyPriority, state: state)
            {
                this.parameters = parameters;

                CurSkillPropor = 0;
                curUnboundedSkillPropor = 0;
                avgVacancyDuration = TimeSpan.Zero;
                workingPropor = 0;
            }

            public void StartUpdate(NodeState state)
            {
                double totalHiredSkill = HiredSkill();
                if (totalHiredSkill >= parameters.reqSkill)
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

                    while (allEmployeesPriorQueue.Count > 0 && totalHiredSkill - allEmployeesPriorQueue.First.skills[parameters.industryType] >= parameters.reqSkill)
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

                    Debug.Assert(HiredSkill() >= parameters.reqSkill);
                }

                double openSpace = OpenSpace();
                if (openSpace is double.NegativeInfinity)
                    avgVacancyDuration = TimeSpan.Zero;
                else
                    avgVacancyDuration += Elapsed;
            }

            public void EndUpdate()
            {
                curUnboundedSkillPropor = peopleHere.Sum(person => person.skills[parameters.industryType]) / parameters.reqSkill;
                CurSkillPropor = Math.Min(1, curUnboundedSkillPropor);
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
                person.skills[parameters.industryType] = 1 - (1 - person.skills[parameters.industryType]) * Math.Pow(1 - person.talents[parameters.industryType], Elapsed.TotalSeconds * workingPropor * CurWorldConfig.personTimeSkillCoeff);
                Debug.Assert(C.IsInSuitableRange(value: person.skills[parameters.industryType]));
            }

            public override bool CanPersonLeave(Person person)
                => true;

            public void SetEnergyPropor(double energyPropor)
            {
                if (!C.IsInSuitableRange(value: energyPropor))
                    throw new ArgumentOutOfRangeException();
                workingPropor = energyPropor / Math.Max(1, curUnboundedSkillPropor);
            }

            public string GetText()
                => $"have {peopleHere.Sum(person => person.skills[parameters.industryType]) / parameters.reqSkill * 100:0.}% skill\ndesperation {(IsFull() ? 0 : Desperation() * 100):0.}%\nemployed {peopleHere.Count}\n";

            private double HiredSkill()
                => allPeople.Sum(person => person.skills[parameters.industryType]);

            private double OpenSpace()
            {
                double hiredSkill = HiredSkill();
                if (hiredSkill >= parameters.reqSkill)
                    return double.NegativeInfinity;
                double result = 1 - hiredSkill / parameters.reqSkill;
                Debug.Assert(C.IsInSuitableRange(result));
                return result;
            }

            private double Desperation()
            {
                Debug.Assert(avgVacancyDuration >= TimeSpan.Zero);
                double openSpace = OpenSpace();
                if (openSpace is double.NegativeInfinity)
                    return double.NegativeInfinity;
                return Math.Tanh(avgVacancyDuration.TotalSeconds * openSpace * CurWorldConfig.jobVacDespCoeff);
            }

            // each parameter must be between 0 and 1 or double.NegativeInfinity
            // larger means this pair is more likely to work
            // must be between 0 and 1 or double.NegativeInfinity
            private double NewEmploymentScore(Person person)
                => CurWorldConfig.personJobEnjoymentCoeff * PersonScoreOfThis(person: person)
                + CurWorldConfig.personTalentCoeff * person.talents[parameters.industryType]
                + CurWorldConfig.personSkillCoeff * person.skills[parameters.industryType]
                + CurWorldConfig.jobDesperationCoeff * Desperation()
                + CurWorldConfig.PlayerToJobDistCoeff * DistanceToHere(person: person);

            private double CurrentEmploymentScore(Person person)
            {
                if (!IsPersonQueuedOrHere(person: person))
                    throw new ArgumentException();
                return CurWorldConfig.personJobEnjoymentCoeff * PersonScoreOfThis(person: person)
                    + CurWorldConfig.personTalentCoeff * person.talents[parameters.industryType]
                    + CurWorldConfig.personSkillCoeff * person.skills[parameters.industryType];
            }
        }

        [field:NonSerialized]
        public event Action Deleted;

        public ulong EnergyPriority
            => IsBusy() switch
            {
                true => parameters.energyPriority,
                false => ulong.MaxValue
            };

        Vector2 IEnergyConsumer.NodePos
            => state.position;

        public IEnumerable<Person> PeopleHere
            => employer.PeopleHere;

        public IHUDElement<NearRectangle> UIElement
            => UIPanel;
        
        //[DataMember]
        //protected readonly NodeState state;
        [DataMember]
        protected bool CanStartProduction { get; private set; }
        protected double CurSkillPropor
            => employer.CurSkillPropor;

        [DataMember]
        protected readonly NodeState state;
        [DataMember]
        private readonly Params parameters;
        [DataMember]
        private readonly Employer employer;
        [DataMember]
        private double energyPropor;
        [DataMember]
        private bool deleted;

        [field:NonSerialized]
        protected UIRectPanel<IHUDElement<NearRectangle>> UIPanel { get; private set; }
        [NonSerialized]
        private TextBox textBox;

        protected Industry(NodeState state, Params parameters)
        {
            this.state = state;
            this.parameters = parameters;

            employer = new
            (
                state: state,
                energyPriority: parameters.energyPriority,
                parameters: parameters
            );

            CanStartProduction = true;
            energyPropor = 0;

            deleted = false;

            AddEnergyConsumer(energyConsumer: this);
        }
        
        public void Initialize()
        {
            textBox = new();
            UIPanel = new UIRectVertPanel<IHUDElement<NearRectangle>>(color: Color.White, childHorizPos: HorizPos.Left);
            UIPanel.AddChild(child: textBox);
            UIPanel.AddChild
            (
                child: new Button<MyRectangle>
                (
                    shape: new
                    (
                        width: 60,
                        height: 30
                    )
                    {
                        Color = Color.Red
                    },
                    explanation: "deletes this industry",
                    action: () => deleted = true,
                    text: "delete"
                )
            );
        }

        public abstract ULongArray TargetStoredResAmounts();

        protected abstract bool IsBusy();

        public Industry Update()
        {
            if (deleted)
            {
                PlayerDelete();
                return null;
            }

            employer.StartUpdate(state: state);

            var result = Update(workingPropor: energyPropor * CurSkillPropor);

            employer.EndUpdate();

            textBox.Text = GetText();

            return result;
        }

        protected abstract Industry Update(double workingPropor);

        protected virtual void PlayerDelete()
            => Delete();

        protected virtual void Delete()
        {
            employer.Delete();
            Deleted?.Invoke();
        }

        public virtual string GetText()
            => CurOverlay switch
            {
                <= MaxRes => "",
                Overlay.AllRes => "",
                Overlay.Power => "",
                Overlay.People => employer.GetText(),
                _ => throw new Exception(),
            };

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