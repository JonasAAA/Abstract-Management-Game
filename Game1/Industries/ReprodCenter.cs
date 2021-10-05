//using Game1.UI;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;

//namespace Game1.Industries
//{
//    public class ReprodCenter : Industry, IReprodCenter
//    {
//        public new class Params : Industry.Params
//        {
//            public readonly double reqWattsPerSecPerChild;
//            public readonly ulong maxCouples;
//            public readonly ConstULongArray resPerChild;
//            public readonly TimeSpan birthDuration;
//            // reqWattsPerSec doesn't make sence as this industry can accomodate variuos number of couples.
//            public Params(string name, ulong electrPriority, double reqSkill, ulong reqWattsPerSecPerChild, ulong maxCouples, ConstULongArray resPerChild, TimeSpan birthDuration)
//                : base
//                (
//                    industryType: IndustryType.Reproduction,
//                    name: name,
//                    electrPriority: electrPriority,
//                    reqSkill: reqSkill,
//                    explanation: $"requires {reqSkill} skill\nneeds {reqWattsPerSecPerChild} W/s for a child\nsupports no more than {maxCouples} couples\nrequires {resPerChild} resources for a child\nchildbirth takes {birthDuration} s"
//                )
//            {
//                this.reqWattsPerSecPerChild = reqWattsPerSecPerChild;
//                this.maxCouples = maxCouples;
//                this.resPerChild = resPerChild;
//                this.birthDuration = birthDuration;
//            }

//            public override Industry MakeIndustry(NodeState state)
//                => new ReprodCenter(parameters: this, state: state);
//        }

//        private readonly Params parameters;
//        private readonly TimedQueue<Person> birthQueue;
//        private readonly HashSet<Person> customersHere, allCustomers;

//        Vector2 IReprodCenter.Position
//            => state.position;

//        public ReprodCenter(Params parameters, NodeState state)
//            : base
//            (
//                parameters: parameters,
//                state: state,
//                UIPanel: new UIRectVertPanel<IUIElement<NearRectangle>>(color: Color.White, childHorizPos: HorizPos.Left)
//            )
//        {
//            this.parameters = parameters;
//            birthQueue = new(duration: parameters.birthDuration);
//            customersHere = new();
//            allCustomers = new();
//            PeopleReproduction.AddReprodCenter(reprodCenter: this);
//        }

//        public override bool IfNeeds(Person person)
//            => allCustomers.Contains(person) || base.IfNeeds(person);

//        public override void Take(Person person)
//        {
//            if (allCustomers.Contains(person))
//            {
//                customersHere.Add(person);
//                // if their partner is already here, do something
//                throw new NotImplementedException();
//                return;
//            }
//            base.Take(person);
//        }

//        public override ULongArray TargetStoredResAmounts()
//            => parameters.maxCouples * parameters.resPerChild * state.maxBatchDemResStored;

//        protected override bool IsBusy()
//            => birthQueue.Count > 0;

//        protected override Industry Update(TimeSpan elapsed, double workingPropor)
//        {
//            throw new NotImplementedException();
//        }

//        protected override void Delete()
//        {
//            base.Delete();
//            // release all the people. some them have jobs, so need to travel there
//            throw new NotImplementedException();
//        }

//        public override double ReqWattsPerSec()
//            => birthQueue.Count * parameters.reqWattsPerSecPerChild * CurSkillPropor;

//        bool IReprodCenter.IsFull()
//        {
//            throw new NotImplementedException();
//        }

//        void IReprodCenter.AddCouple(Person person1, Person person2)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
