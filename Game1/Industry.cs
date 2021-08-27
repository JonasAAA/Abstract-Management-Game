using Microsoft.Xna.Framework.Input;
using Priority_Queue;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public abstract class Industry : IJob
    {
        // all fields and properties in this and derived classes must have unchangeable state
        public abstract class Params
        {
            public readonly IndustryType industryType;
            public readonly string name;
            public readonly double reqSkill;
            public readonly double reqWattsPerSec, prodWattsPerSec;

            public Params(IndustryType industryType, string name, double reqSkill, double reqWattsPerSec, double prodWattsPerSec)
            {
                this.industryType = industryType;
                this.name = name;
                if (reqSkill <= 0)
                    throw new ArgumentOutOfRangeException();
                this.reqSkill = reqSkill;
                if (reqWattsPerSec < 0)
                    throw new ArgumentOutOfRangeException();
                this.reqWattsPerSec = reqWattsPerSec;
                if (prodWattsPerSec < 0 || (industryType is not IndustryType.PowerPlant && prodWattsPerSec > 0))
                    throw new ArgumentOutOfRangeException();
                this.prodWattsPerSec = prodWattsPerSec;
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
                    reqSkill: 10,
                    reqWattsPerSec: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory0_lvl1",
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
                    reqSkill: 10,
                    reqWattsPerSec: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory1_lvl1",
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
                    reqSkill: 10,
                    reqWattsPerSec: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory2_lvl1",
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
                    reqSkill: 10,
                    reqWattsPerSec: 100,
                    industrParams: new Factory.Params
                    (
                        name: "factory0_lvl2",
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
        private double avgVacancyDuration;
        
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
            avgVacancyDuration = 0;
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
            Debug.Assert(avgVacancyDuration >= 0);
            double openSpace = OpenSpace();
            if (avgVacancyDuration is double.NegativeInfinity || openSpace is double.NegativeInfinity)
                return double.NegativeInfinity;
            return Math.Tanh(avgVacancyDuration * openSpace * vacDespCoeff);
        }

        public void Hire(Person person)
            => state.travelingEmployees.Add(person);

        public void LetGo(Person person)
        {
            double oldOpenSpace = OpenSpace();
            state.Fire(person: person);
            double curOpenSpace = OpenSpace();
            if (oldOpenSpace is double.NegativeInfinity)
                avgVacancyDuration = 0;
            else
                avgVacancyDuration *= oldOpenSpace / curOpenSpace;
        }

        public abstract ULongArray TargetStoredResAmounts();

        protected abstract bool IsBusy();

        public double ReqWattsPerSec()
            => IsBusy() switch
            {
                true => parameters.reqWattsPerSec * CurSkillPropor,
                false => 0
            };

        //each node and link may have their own internal clock based on how much electricity they get
        //so e.g. power plant clock will tick at the same rate as normal clock
        //this clock would influence when products are produced/transported, how much skill people get for working one frame, how quickly buildings deteriorate etc.
        //but then what about unemployed people in power plant tile?
        //also, people could be unhappy about getting not enough electricity
        //(or else player could just take all unemplyed people to one node and shut them down by giving no electricity)

        public double ProdWattsPerSec()
            => IsBusy() switch
            {
                true => parameters.prodWattsPerSec * CurSkillPropor,
                false => 0
            };

        public void ActiveUpdate()
            => togglePauseButton.Update();

        public virtual Industry Update()
        {
            if (IsBusy())
                state.employees.ForEach(person => person.Work(industryType: IndustryType));

            double totalHiredSkill = HiredSkill();
            if (totalHiredSkill >= parameters.reqSkill)
            {
                SimplePriorityQueue<Person, double> allEmployees = new();
                foreach (var person in state.employees.Concat(state.travelingEmployees))
                    allEmployees.Enqueue
                    (
                        item: person,
                        priority: JobMatching.CurrentEmploymentScore(job: this, person: person)
                    );

                HashSet<Person> firedPeople = new();
                while (allEmployees.Count > 0 && totalHiredSkill - allEmployees.First.skills[IndustryType] >= parameters.reqSkill)
                {
                    totalHiredSkill -= allEmployees.First.skills[IndustryType];
                    firedPeople.Add(allEmployees.Dequeue());
                }

                state.FireAllMatching(person => firedPeople.Contains(person));
            }

            double openSpace = OpenSpace();
            if (openSpace is double.NegativeInfinity)
                avgVacancyDuration = 0;
            else
                avgVacancyDuration += C.ElapsedGameTime.TotalSeconds;

            CurSkillPropor = Math.Min(1, state.employees.Sum(person => person.skills[IndustryType]) / parameters.reqSkill);
            return this;
        }

        public virtual string GetText()
            => $"have {state.employees.Sum(person => person.skills[IndustryType]) / parameters.reqSkill * 100:0.}% skill\ndesperation {(Desperation() is double.NegativeInfinity ? 0 : Desperation() * 100):0.}%\n";
    }
}