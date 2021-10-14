using Game1.UI;
using Microsoft.Xna.Framework;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Game1.WorldManager;

namespace Game1.Industries
{
    public abstract class Industry : IEnergyConsumer
    {
        // all fields and properties in this and derived classes must have unchangeable state
        public abstract class Params
        {
            public readonly IndustryType industryType;
            public readonly string name;
            public readonly ulong energyPriority;
            public readonly double reqSkill;
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

            public abstract Industry MakeIndustry(NodeState state);
        }

        private class Employer : ActivityCenter
        {
            public double CurSkillPropor { get; private set; }

            private readonly Params parameters;
            // must be >= 0
            private TimeSpan avgVacancyDuration;
            private double curUnboundedSkillPropor, workingPropor;

            public Employer(Vector2 position, ulong energyPriority, Action<Person> personLeft, Params parameters)
                : base(activityType: ActivityType.Working, position: position, energyPriority: energyPriority, personLeft: personLeft)
            {
                this.parameters = parameters;

                CurSkillPropor = 0;
                curUnboundedSkillPropor = 0;
                avgVacancyDuration = TimeSpan.Zero;
                workingPropor = 0;
            }

            public void StartUpdate()
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

        public readonly IUIElement<NearRectangle> UIElement;
        protected readonly UIRectPanel<IUIElement<NearRectangle>> UIPanel;
        private readonly TextBox textBox;

        protected readonly NodeState state;
        protected bool CanStartProduction { get; private set; }
        protected double CurSkillPropor
            => employer.CurSkillPropor;
        
        private readonly Params parameters;
        private readonly Employer employer;
        private double energyPropor;
        private bool deleted;
        
        protected Industry(Params parameters, NodeState state, UIRectPanel<IUIElement<NearRectangle>> UIPanel)
        {
            this.parameters = parameters;
            this.state = state;
            UIElement = UIPanel;
            this.UIPanel = UIPanel;

            employer = new
            (
                position: state.position,
                energyPriority: parameters.energyPriority,
                personLeft: PersonLeft,
                parameters: parameters
            );

            CanStartProduction = true;
            energyPropor = 0;

            deleted = false;

            AddEnergyConsumer(energyConsumer: this);

            textBox = new();
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

            employer.StartUpdate();

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

        protected void PersonLeft(Person person)
            => state.waitingPeople.Add(person);

        void IEnergyConsumer.ConsumeEnergy(double energyPropor)
        {
            if (!C.IsInSuitableRange(value: energyPropor))
                throw new ArgumentOutOfRangeException();
            this.energyPropor = energyPropor;
            employer.SetEnergyPropor(energyPropor: energyPropor);
        }
    }
}