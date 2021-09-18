using Microsoft.Xna.Framework.Input;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public abstract class Industry : IEmployer, IElectrConsumer, IJob
    {
        // all fields and properties in this and derived classes must have unchangeable state
        public abstract class Params
        {
            public readonly IndustryType industryType;
            public readonly string name;
            public readonly ulong electrPriority;
            public readonly double reqSkill, reqWattsPerSec;

            public Params(IndustryType industryType, string name, ulong electrPriority, double reqSkill, double reqWattsPerSec)
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
                if (reqWattsPerSec < 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerSec = reqWattsPerSec;
            }

            public abstract Industry MakeIndustry(NodeState state);
        }

        public static readonly uint TypeCount;
        public static readonly ReadOnlyCollection<Construction.Params> constrBuildingParams;
        private static readonly double vacDespCoeff;

        static Industry()
        {
            vacDespCoeff = .1;

            TypeCount = (uint)Enum.GetValues<IndustryType>().Length;

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
                    }
                ),
            });
        }

        public IndustryType IndustryType
            => parameters.industryType;

        protected readonly NodeState state;
        protected bool CanStartProduction { get; private set; }
        protected double CurSkillPropor { get; private set; }

        private readonly Params parameters;
        // must be >= 0
        private TimeSpan avgVacancyDuration;
        private double curUnboundedSkillPropor, electrPropor;
        private readonly HashSet<Person> employeesHere, allEmployees;

        protected Industry(Params parameters, NodeState state)
        {
            this.parameters = parameters;
            this.state = state;
            CanStartProduction = true;
            CurSkillPropor = 0;
            curUnboundedSkillPropor = 0;
            avgVacancyDuration = TimeSpan.Zero;
            electrPropor = 0;

            employeesHere = new();
            allEmployees = new();

            ElectricityDistributor.AddElectrConsumer(electrConsumer: this);
        }

        public bool IfEmploys(Person person)
            => allEmployees.Contains(person);

        public void Take(Person person)
        {
            if (!IfEmploys(person: person) || !employeesHere.Add(person))
                throw new InvalidOperationException();
        }

        private double HiredSkill()
            => allEmployees.Sum(person => person.skills[IndustryType]);

        private double OpenSpace()
        {
            double hiredSkill = HiredSkill();
            if (hiredSkill >= parameters.reqSkill)
                return double.NegativeInfinity;
            double result = 1 - hiredSkill / parameters.reqSkill;
            Debug.Assert(C.IsInSuitableRange(result));
            return result;
        }

        public double Desperation()
        {
            Debug.Assert(avgVacancyDuration >= TimeSpan.Zero);
            double openSpace = OpenSpace();
            if (/*avgVacancyDuration is double.NegativeInfinity || */openSpace is double.NegativeInfinity)
                return double.NegativeInfinity;
            return Math.Tanh(avgVacancyDuration.TotalSeconds * openSpace * vacDespCoeff);
        }

        public void Hire(Person person)
            => allEmployees.Add(person);

        public IJob CreateJob()
            => this;

        public abstract ULongArray TargetStoredResAmounts();

        protected abstract bool IsBusy();

        public Industry Update(TimeSpan elapsed)
        {
            // employees with jobs should not want to travel anywhere
            Debug.Assert(employeesHere.All(person => person.Destination is null));

            // employees with jobs already traveled here, so should not want to travel here again
            Debug.Assert(employeesHere.All(person => (person.Destination is null || person.Destination != state.position)));

            if (elapsed < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();

            if (IsBusy())
            {
                foreach (var person in employeesHere)
                    person.UpdateWorking
                    (
                        elapsed: elapsed,
                        workingPropor: electrPropor / Math.Max(1, curUnboundedSkillPropor)
                    );
            }

            double totalHiredSkill = HiredSkill();
            if (totalHiredSkill >= parameters.reqSkill)
            {
                double oldOpenSpace = OpenSpace();

                SimplePriorityQueue<Person, double> allEmployeesPriorQueue = new();
                foreach (var person in allEmployees)
                    allEmployeesPriorQueue.Enqueue
                    (
                        item: person,
                        priority: JobMatching.CurrentEmploymentScore(employer: this, person: person)
                    );

                while (allEmployeesPriorQueue.Count > 0 && totalHiredSkill - allEmployeesPriorQueue.First.skills[IndustryType] >= parameters.reqSkill)
                {
                    totalHiredSkill -= allEmployeesPriorQueue.First.skills[IndustryType];
                    var person = allEmployeesPriorQueue.Dequeue();
                    person.Fire();
                    allEmployees.Remove(person);
                    if (employeesHere.Remove(person))
                        state.unemployedPeople.Add(person);
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

            var result = Update
            (
                elapsed: elapsed,
                workingPropor: electrPropor * CurSkillPropor
            );

            curUnboundedSkillPropor = employeesHere.Sum(person => person.skills[IndustryType]) / parameters.reqSkill;
            CurSkillPropor = Math.Min(1, curUnboundedSkillPropor);

            return result;
        }

        protected abstract Industry Update(TimeSpan elapsed, double workingPropor);

        public virtual void Clear()
        {
            foreach (var person in allEmployees)
                person.Fire();
            state.unemployedPeople.AddRange(employeesHere);
            employeesHere.Clear();
            allEmployees.Clear();
            ElectricityDistributor.RemoveElectrConsumer(electrConsumer: this);
        }

        public virtual string GetText()
            => Graph.World.Overlay switch
            {
                <= C.MaxRes => "",
                Overlay.AllRes => "",
                Overlay.People => $"have {employeesHere.Sum(person => person.skills[IndustryType]) / parameters.reqSkill * 100:0.}% skill\ndesperation {(Desperation() is double.NegativeInfinity ? 0 : Desperation() * 100):0.}%\nemployed {employeesHere.Count}\n",
                _ => throw new Exception(),
            };

        public double ReqWattsPerSec()
            // this is correct as if more important people get full electricity, this works
            // and if they don't, then the industry will get 0 electricity anyway
            => IsBusy() switch
            {
                true => parameters.reqWattsPerSec * CurSkillPropor,
                false => 0
            };

        public ulong ElectrPriority
            => IsBusy() switch
            {
                true => parameters.electrPriority,
                false => ulong.MaxValue
            };

        public void ConsumeElectr(double electrPropor)
        {
            if (!C.IsInSuitableRange(value: electrPropor))
                throw new ArgumentOutOfRangeException();
            this.electrPropor = electrPropor;
        }
    }
}