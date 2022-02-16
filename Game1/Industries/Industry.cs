﻿using Game1.Events;
using Game1.Shapes;
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
    [Serializable]
    public abstract class Industry : IEnergyConsumer
    {
        // all fields and properties in this and derived classes must have unchangeable state
        [Serializable]
        public abstract class Params
        {
            public readonly IndustryType industryType;
            public readonly string name;
            public readonly ulong energyPriority;
            public readonly double reqSkill;
            public readonly string explanation;

            protected Params(IndustryType industryType, string name, ulong energyPriority, double reqSkill, string explanation)
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

        [Serializable]
        private class Employer : ActivityCenter
        {
            public double CurSkillPropor { get; private set; }

            private readonly Params parameters;
            // must be >= 0
            private TimeSpan avgVacancyDuration;
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

                if (IsFull())
                    avgVacancyDuration = TimeSpan.Zero;
                else
                    avgVacancyDuration += CurWorldManager.Elapsed;
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
                person.skills[parameters.industryType] = 1 - (1 - person.skills[parameters.industryType]) * Math.Pow(1 - person.talents[parameters.industryType], CurWorldManager.Elapsed.TotalSeconds * workingPropor * CurWorldConfig.personTimeSkillCoeff);
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

            public string GetInfo()
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

        [Serializable]
        private record DeleteButtonClickedListener(Industry Industry) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => Industry.isDeleted = true;
        }

        public IEvent<IDeletedListener> Deleted
            => deleted;

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

        public IHUDElement UIElement
            => UIPanel;
        
        protected bool CanStartProduction { get; private set; }
        protected double CurSkillPropor
            => employer.CurSkillPropor;

        protected readonly NodeState state;
        private readonly Params parameters;
        private readonly Employer employer;
        private double energyPropor;
        private bool isDeleted;
        private readonly Event<IDeletedListener> deleted;

        protected readonly UIRectPanel<IHUDElement> UIPanel;
        private readonly TextBox textBox;

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

            isDeleted = false;
            deleted = new();

            textBox = new();
            UIPanel = new UIRectVertPanel<IHUDElement>(color: Color.White, childHorizPos: HorizPos.Left);
            UIPanel.AddChild(child: textBox);
            Button deleteButton = new
            (
                shape: new MyRectangle
                (
                    width: 60,
                    height: 30
                )
                {
                    Color = Color.Red
                },
                explanation: "deletes this industry",
                text: "delete"
            );
            deleteButton.clicked.Add(listener: new DeleteButtonClickedListener(Industry: this));
            UIPanel.AddChild(child: deleteButton);

            CurWorldManager.AddEnergyConsumer(energyConsumer: this);
        }

        public abstract ULongArray TargetStoredResAmounts();

        protected abstract bool IsBusy();

        public Industry Update()
        {
            if (isDeleted)
            {
                PlayerDelete();
                return null;
            }

            employer.StartUpdate();

            var result = Update(workingPropor: energyPropor * CurSkillPropor);

            employer.EndUpdate();

            textBox.Text = GetInfo();

            return result;
        }

        protected abstract Industry Update(double workingPropor);

        protected virtual void PlayerDelete()
            => Delete();

        protected virtual void Delete()
        {
            employer.Delete();
            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
        }

        public virtual string GetInfo()
            => CurWorldManager.Overlay switch
            {
                <= MaxRes => "",
                Overlay.AllRes => "",
                Overlay.Power => "",
                Overlay.People => employer.GetInfo(),
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