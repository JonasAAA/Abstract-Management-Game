using Game1.UI;
using Microsoft.Xna.Framework;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1.Industries
{
    public abstract class Industry : IElectrConsumer //, IEmployer, IJob
    {
        // all fields and properties in this and derived classes must have unchangeable state
        public abstract class Params
        {
            public readonly IndustryType industryType;
            public readonly string name;
            public readonly ulong electrPriority;
            public readonly double reqSkill/*, reqWattsPerSec*/;
            public readonly string explanation;

            public Params(IndustryType industryType, string name, ulong electrPriority, double reqSkill, /*double reqWattsPerSec, */string explanation)
            {
                this.industryType = industryType;
                this.name = name;
                if ((industryType is IndustryType.PowerPlant && electrPriority is not 0)
                    || (industryType is not IndustryType.PowerPlant && electrPriority is 0))
                    throw new ArgumentException();
                this.electrPriority = electrPriority;
                if (reqSkill <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqSkill = reqSkill;
                //if (reqWattsPerSec < 0)
                //    throw new ArgumentOutOfRangeException();
                //this.reqWattsPerSec = reqWattsPerSec;
                this.explanation = explanation;
            }

            public abstract Industry MakeIndustry(NodeState state);
        }

        private class Employer : ActivityCenter
        {
            //explicitly implement as many functions as possible to reduce clutter
            //also can reorganize order and names of functions
            private static readonly double enjoymentCoeff, talentCoeff, skillCoeff, desperationCoeff, distCoeff, minAcceptableScore, personTimeSkillCoeff;

            static Employer()
            {
                enjoymentCoeff = .2;
                talentCoeff = .1;
                skillCoeff = .2;
                distCoeff = .1;
                desperationCoeff = .4;

                minAcceptableScore = .4;

                personTimeSkillCoeff = .1;
            }

            //public IndustryType IndustryType
            //    => parameters.industryType;

            public double CurSkillPropor { get; private set; }

            private readonly Params parameters;
            // must be >= 0
            private TimeSpan avgVacancyDuration;
            private double curUnboundedSkillPropor, workingPropor;

            public Employer(Vector2 position, ulong electrPriority, Action<Person> personLeft, Params parameters)
                : base(position: position, electrPriority: electrPriority, personLeft: personLeft)
            {
                this.parameters = parameters;

                CurSkillPropor = 0;
                curUnboundedSkillPropor = 0;
                avgVacancyDuration = TimeSpan.Zero;
                workingPropor = 0;
            }

            public void StartUpdate(TimeSpan elapsed)
            {
                if (elapsed < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException();

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
                        totalHiredSkill -= allEmployeesPriorQueue.First.skills[parameters.industryType];
                        var person = allEmployeesPriorQueue.Dequeue();
                        RemovePerson(person: person);
                    }

                    double curOpenSpace = OpenSpace();
                    if (oldOpenSpace is double.NegativeInfinity)
                        avgVacancyDuration = TimeSpan.Zero;
                    else
                        avgVacancyDuration *= oldOpenSpace / curOpenSpace;
                }

                double openSpace = OpenSpace();
                if (openSpace is double.NegativeInfinity)
                    avgVacancyDuration = TimeSpan.Zero;
                else
                    avgVacancyDuration += elapsed;
            }

            public void EndUpdate()
            {
                curUnboundedSkillPropor = peopleHere.Sum(person => person.skills[parameters.industryType]) / parameters.reqSkill;
                CurSkillPropor = Math.Min(1, curUnboundedSkillPropor);
            }

            public override bool IsFull()
                => OpenSpace() is double.NegativeInfinity;

            public override double PersonScoreOfThis(Person person)
                => Person.momentumCoeff * (IsPersonHere(person: person) ? 1 : 0)
                + (.9 * person.enjoyments[parameters.industryType] + .1 * DistanceToHere(person: person)) * (1 - Person.momentumCoeff);

            public override bool IsPersonSuitable(Person person)
            {
                if (IsPersonQueuedOrHere(person: person))
                    return true;

                return NewEmploymentScore(person: person) >= minAcceptableScore;
            }

            public override void UpdatePerson(Person person, TimeSpan elapsed)
            {
                if (!C.IsInSuitableRange(value: workingPropor))
                    throw new ArgumentOutOfRangeException();

                Debug.Assert(C.IsInSuitableRange(value: person.skills[parameters.industryType]));
                person.skills[parameters.industryType] = 1 - (1 - person.skills[parameters.industryType]) * Math.Pow(1 - person.talents[parameters.industryType], elapsed.TotalSeconds * workingPropor * personTimeSkillCoeff);
                Debug.Assert(C.IsInSuitableRange(value: person.skills[parameters.industryType]));
            }

            public void SetElectrPropor(double electrPropor)
            {
                if (!C.IsInSuitableRange(value: electrPropor))
                    throw new ArgumentOutOfRangeException();
                workingPropor = electrPropor / Math.Max(1, curUnboundedSkillPropor);
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
                return Math.Tanh(avgVacancyDuration.TotalSeconds * openSpace * vacDespCoeff);
            }

            // each parameter must be between 0 and 1 or double.NegativeInfinity
            // larger means this pair is more likely to work
            // must be between 0 and 1 or double.NegativeInfinity
            private double NewEmploymentScore(Person person)
                => enjoymentCoeff * PersonScoreOfThis(person: person)
                + talentCoeff * person.talents[parameters.industryType]
                + skillCoeff * person.skills[parameters.industryType]
                + desperationCoeff * Desperation()
                + distCoeff * DistanceToHere(person: person);

            private double CurrentEmploymentScore(Person person)
            {
                if (!IsPersonQueuedOrHere(person: person))
                    throw new ArgumentException();
                return enjoymentCoeff * PersonScoreOfThis(person: person)
                    + talentCoeff * person.talents[parameters.industryType]
                    + skillCoeff * person.skills[parameters.industryType];
            }
        }

        public event Action Deleted;

        public static readonly int TypeCount;
        public static readonly ReadOnlyCollection<Construction.Params> constrBuildingParams;
        private static readonly double vacDespCoeff;

        static Industry()
        {
            vacDespCoeff = .1;

            TypeCount = Enum.GetValues<IndustryType>().Length;

            constrBuildingParams = new(list: new Construction.Params[]
            {
                new
                (
                    name: "factory costruction",
                    electrPriority: 10,
                    reqSkill: 10,
                    reqWattsPerSec: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory0_lvl1",
                        electrPriority: 20,
                        reqSkill: 10,
                        reqWattsPerSec: 10,
                        supply: new()
                        {
                            [0] = 10,
                        },
                        demand: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "factory costruction",
                    electrPriority: 10,
                    reqSkill: 10,
                    reqWattsPerSec: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory1_lvl1",
                        electrPriority: 20,
                        reqSkill: 10,
                        reqWattsPerSec: 10,
                        supply: new()
                        {
                            [1] = 10,
                        },
                        demand: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "factory costruction",
                    electrPriority: 10,
                    reqSkill: 10,
                    reqWattsPerSec: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory2_lvl1",
                        electrPriority: 20,
                        reqSkill: 10,
                        reqWattsPerSec: 10,
                        supply: new()
                        {
                            [2] = 10,
                        },
                        demand: new(),
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "power plant costruction",
                    electrPriority: 10,
                    reqSkill: 30,
                    reqWattsPerSec: 100,
                    industrParams: new PowerPlant.Params
                    (
                        name: "power_plant_lvl1",
                        reqSkill: 20,
                        prodWattsPerSec: 1000
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                ),
                new
                (
                    name: "factory costruction",
                    electrPriority: 10,
                    reqSkill: 10,
                    reqWattsPerSec: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory0_lvl2",
                        electrPriority: 20,
                        reqSkill: 10,
                        reqWattsPerSec: 10,
                        supply: new()
                        {
                            [0] = 100,
                        },
                        demand: new()
                        {
                            [1] = 50,
                        },
                        prodDuration: TimeSpan.FromSeconds(value: 2)
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                    {
                        [1] = 20,
                        [2] = 10
                    }
                ),
            });
        }

        public IEnumerable<Person> PeopleHere
            //=> employeesHere;
            => employer.PeopleHere;
        //public IndustryType IndustryType
        //    => parameters.industryType;

        public readonly IUIElement<NearRectangle> UIElement;
        protected readonly UIRectPanel<IUIElement<NearRectangle>> UIPanel;
        protected readonly TextBox textBox;

        protected readonly NodeState state;
        protected bool CanStartProduction { get; private set; }
        protected double CurSkillPropor
            => employer.CurSkillPropor;
        //protected double CurSkillPropor { get; private set; }

        private readonly Params parameters;
        private readonly Employer employer;
        // must be >= 0
        //private TimeSpan avgVacancyDuration;
        private double /*curUnboundedSkillPropor, */electrPropor;
        //private readonly MyHashSet<Person> employeesHere, allEmployees;
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
                electrPriority: parameters.electrPriority,
                personLeft: person => state.waitingPeople.Add(person),
                parameters: parameters
            );

            CanStartProduction = true;
            //CurSkillPropor = 0;
            //curUnboundedSkillPropor = 0;
            //avgVacancyDuration = TimeSpan.Zero;
            electrPropor = 0;

            //employeesHere = new();
            //allEmployees = new();
            deleted = false;

            ElectricityDistributor.AddElectrConsumer(electrConsumer: this);

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

        //public virtual bool IfNeeds(Person person)
        //    => allEmployees.Contains(person);

        //public virtual void Take(Person person)
        //{
        //    if (!IfNeeds(person: person))
        //        throw new InvalidOperationException();
        //    employeesHere.Add(person);
        //}
        
        public abstract ULongArray TargetStoredResAmounts();

        protected abstract bool IsBusy();

        public Industry Update(TimeSpan elapsed)
        {
            //// employees with jobs should not want to travel anywhere
            //Debug.Assert(employeesHere.All(person => person.Destination is null));

            //// employees with jobs already traveled here, so should not want to travel here again
            //Debug.Assert(employeesHere.All(person => (person.Destination is null || person.Destination != state.position)));

            if (elapsed < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();

            if (deleted)
            {
                PlayerDelete();
                return null;
            }

            employer.StartUpdate(elapsed: elapsed);

            ////if (IsBusy())
            ////{
            ////    foreach (var person in employeesHere)
            ////        person.UpdateWorking
            ////        (
            ////            elapsed: elapsed,
            ////            workingPropor: electrPropor / Math.Max(1, curUnboundedSkillPropor)
            ////        );
            ////}

            //double totalHiredSkill = HiredSkill();
            //if (totalHiredSkill >= parameters.reqSkill)
            //{
            //    // if can, fire the forst person
            //    throw new NotImplementedException();
            //    //double oldOpenSpace = OpenSpace();

            //    //SimplePriorityQueue<Person, double> allEmployeesPriorQueue = new();
            //    //foreach (var person in allEmployees)
            //    //    allEmployeesPriorQueue.Enqueue
            //    //    (
            //    //        item: person,
            //    //        priority: JobMatching.CurrentEmploymentScore(employer: this, person: person)
            //    //    );

            //    //while (allEmployeesPriorQueue.Count > 0 && totalHiredSkill - allEmployeesPriorQueue.First.skills[IndustryType] >= parameters.reqSkill)
            //    //{
            //    //    totalHiredSkill -= allEmployeesPriorQueue.First.skills[IndustryType];
            //    //    var person = allEmployeesPriorQueue.Dequeue();
            //    //    person.Fire();
            //    //    allEmployees.Remove(person);
            //    //    if (employeesHere.Remove(person))
            //    //        state.unemployedPeople.Add(person);
            //    //}

            //    //double curOpenSpace = OpenSpace();
            //    //if (oldOpenSpace is double.NegativeInfinity)
            //    //    avgVacancyDuration = TimeSpan.Zero;
            //    //else
            //    //    avgVacancyDuration *= oldOpenSpace / curOpenSpace;
            //}

            //double openSpace = OpenSpace();
            //if (openSpace is double.NegativeInfinity)
            //    avgVacancyDuration = TimeSpan.Zero;
            //else
            //    avgVacancyDuration += elapsed;

            var result = Update
            (
                elapsed: elapsed,
                workingPropor: electrPropor * CurSkillPropor
            );

            employer.EndUpdate();

            return result;
        }

        protected abstract Industry Update(TimeSpan elapsed, double workingPropor);

        protected virtual void PlayerDelete()
            => Delete();

        protected virtual void Delete()
        {
            //foreach (var person in allEmployees)
            //    person.Fire();
            //state.unemployedPeople.AddRange(employeesHere);
            //employeesHere.Clear();
            //allEmployees.Clear();
            employer.Delete();
            Deleted?.Invoke();
            //ElectricityDistributor.RemoveElectrConsumer(electrConsumer: this);
        }

        public virtual string GetText()
            => Graph.Overlay switch
            {
                <= C.MaxRes => "",
                Overlay.AllRes => "",
                Overlay.People => employer.GetText(),
                _ => throw new Exception(),
            };

        public abstract double ReqWattsPerSec();

        //public double ReqWattsPerSec()
        //    // this is correct as if more important people get full electricity, this works
        //    // and if they don't, then the industry will get 0 electricity anyway
        //    => IsBusy() switch
        //    {
        //        true => parameters.reqWattsPerSec * CurSkillPropor,
        //        false => 0
        //    };

        public ulong ElectrPriority
            => IsBusy() switch
            {
                true => parameters.electrPriority,
                false => ulong.MaxValue
            };

        //private double HiredSkill()
        //    => allEmployees.Sum(person => person.skills[IndustryType]);

        //private double OpenSpace()
        //{
        //    double hiredSkill = HiredSkill();
        //    if (hiredSkill >= parameters.reqSkill)
        //        return double.NegativeInfinity;
        //    double result = 1 - hiredSkill / parameters.reqSkill;
        //    Debug.Assert(C.IsInSuitableRange(result));
        //    return result;
        //}

        //double IEmployer.Desperation()
        //{
        //    Debug.Assert(avgVacancyDuration >= TimeSpan.Zero);
        //    double openSpace = OpenSpace();
        //    if (openSpace is double.NegativeInfinity)
        //        return double.NegativeInfinity;
        //    return Math.Tanh(avgVacancyDuration.TotalSeconds * openSpace * vacDespCoeff);
        //}

        //void IEmployer.Hire(Person person)
        //    => allEmployees.Add(person);

        //IJob IEmployer.CreateJob()
        //    => this;

        void IElectrConsumer.ConsumeElectr(double electrPropor)
        {
            if (!C.IsInSuitableRange(value: electrPropor))
                throw new ArgumentOutOfRangeException();
            this.electrPropor = electrPropor;
            employer.SetElectrPropor(electrPropor: electrPropor);
        }

        //void IJob.TempLeave(Person person)
        //{
        //    if (!allEmployees.Contains(person))
        //        throw new ArgumentException();

        //    if (employeesHere.Remove(person))
        //        state.waitingPeople.Add(person);
        //}
    }
}