using Microsoft.Xna.Framework.Input;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// deal with the fact that industry and it's personnel may get different proportions of electricity
    /// </summary>
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
        private readonly KeyButton togglePauseButton;
        // must be >= 0
        private TimeSpan avgVacancyDuration;
        private double curUnboundedSkillPropor, electrPropor;

        protected Industry(Params parameters, NodeState state)
        {
            this.parameters = parameters;
            this.state = state;
            CanStartProduction = true;
            togglePauseButton = new
            (
                key: Keys.P,
                action: () => CanStartProduction = !CanStartProduction
            );
            CurSkillPropor = 0;
            curUnboundedSkillPropor = 0;
            avgVacancyDuration = TimeSpan.Zero;
            electrPropor = 0;

            ElectricityDistributor.AddElectrConsumer(electrConsumer: this);
        }

        private double HiredSkill()
            => state.employees.Concat(state.travelingEmployees).Sum(person => person.skills[IndustryType]);

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
            => state.travelingEmployees.Add(person);

        public IJob CreateJob()
            => this;

        public abstract ULongArray TargetStoredResAmounts();

        protected abstract bool IsBusy();

        public void ActiveUpdate()
            => togglePauseButton.Update();

        public Industry Update(TimeSpan elapsed)
        {
            if (elapsed < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException();

            if (IsBusy())
            {
                foreach (var person in state.employees)
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

                SimplePriorityQueue<Person, double> allEmployees = new();
                foreach (var person in state.employees.Concat(state.travelingEmployees))
                    allEmployees.Enqueue
                    (
                        item: person,
                        priority: JobMatching.CurrentEmploymentScore(employer: this, person: person)
                    );

                HashSet<Person> firedPeople = new();
                while (allEmployees.Count > 0 && totalHiredSkill - allEmployees.First.skills[IndustryType] >= parameters.reqSkill)
                {
                    totalHiredSkill -= allEmployees.First.skills[IndustryType];
                    firedPeople.Add(allEmployees.Dequeue());
                }

                state.FireAllMatching(person => firedPeople.Contains(person));

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

            curUnboundedSkillPropor = state.employees.Sum(person => person.skills[IndustryType]) / parameters.reqSkill;
            CurSkillPropor = Math.Min(1, curUnboundedSkillPropor);

            return result;
        }

        protected abstract Industry Update(TimeSpan elapsed, double workingPropor);

        public virtual void Clear()
        {
            state.FireAll();
            ElectricityDistributor.RemoveElectrConsumer(electrConsumer: this);
        }

        public virtual string GetText()
            => $"have {state.employees.Sum(person => person.skills[IndustryType]) / parameters.reqSkill * 100:0.}% skill\ndesperation {(Desperation() is double.NegativeInfinity ? 0 : Desperation() * 100):0.}%\n";

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