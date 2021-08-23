using System;

namespace Game1
{
    public class Factory : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly ConstULongArray supply, demand;
            public readonly TimeSpan prodDuration;

            public Params(string name, double reqSkill, ulong reqWattsPerSec, ConstULongArray supply, ConstULongArray demand, TimeSpan prodDuration)
                : base(industryType: IndustryType.Production, name: name, reqSkill: reqSkill, reqWattsPerSec: reqWattsPerSec, prodWattsPerSec: 0)
            {
                this.supply = supply;
                this.demand = demand;
                if (prodDuration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.prodDuration = prodDuration;
            }

            public override Industry MakeIndustry(NodeState state)
                => new Factory(parameters: this, state: state);
        }

        private readonly Params parameters;
        private TimeSpan? prodEndTime;

        private Factory(Params parameters, NodeState state)
            : base(parameters: parameters, state: state)
        {
            this.parameters = parameters;
            prodEndTime = null;
        }

        public override ULongArray TargetStoredResAmounts()
        {
            if (CanStartProduction)
                return parameters.demand * state.maxBatchDemResStored;
            return new();
        }

        protected override bool IsBusy()
            => prodEndTime.HasValue;

        public override Industry Update()
        {
            base.Update();

            if (prodEndTime.HasValue)
                prodEndTime += (1 - CurSkillPropor) * C.ElapsedGameTime;

            if (CanStartProduction && prodEndTime is null && state.storedRes >= parameters.demand)
            {
                state.storedRes -= parameters.demand;
                prodEndTime = C.TotalGameTime + parameters.prodDuration;
            }

            if (prodEndTime.HasValue && prodEndTime <= C.TotalGameTime)
            {
                state.waitingTravelPacket.Add(resAmounts: parameters.supply);
                //state.waitingRes += parameters.supply;
                prodEndTime = null;
            }

            return this;
        }

        public override string GetText()
        {
            string text = base.GetText() + $"{parameters.name}\n";
            if (prodEndTime is null)
                text += "idle";
            else
                text += $"producing {C.DonePart(endTime: prodEndTime.Value, duration: parameters.prodDuration) * 100: 0.}%";
            if (!CanStartProduction)
                text += "\nwill not start new";
            return text;
        }
    }
}


//using System;
//using System.Collections.Generic;

//namespace Game1
//{
//    public class Factory : Industry
//    {
//        public new class Params : Industry.Params
//        {
//            public readonly ConstULongArray supply, demand;
//            public readonly TimeSpan prodTime;
//            public readonly ulong reqWattsPerSec;
//            public readonly double reqSkill;

//            public Params(string name, List<Upgrade> upgrades, ConstULongArray supply, ConstULongArray demand, TimeSpan prodTime, ulong reqWattsPerSec, double reqSkill)
//                : base(name: name, industryType: IndustryType.Production, upgrades: upgrades)
//            {
//                this.supply = supply;
//                this.demand = demand;
//                if (prodTime < TimeSpan.Zero)
//                    throw new ArgumentException();
//                this.prodTime = prodTime;
//                if (reqWattsPerSec <= 0)
//                    throw new ArgumentOutOfRangeException();
//                this.reqWattsPerSec = reqWattsPerSec;
//                if (reqSkill <= 0)
//                    throw new ArgumentOutOfRangeException();
//                this.reqSkill = reqSkill;
//            }

//            public override Industry MakeIndustry(NodeState state)
//                => new Factory(parameters: this, state: state);
//        }

//        protected override bool IsProducing
//            => !production.Empty;

//        private readonly Params parameters;
//        private readonly TimedResQueue production;

//        public Factory(Params parameters, NodeState state)
//            : base(parameters: parameters, state: state)
//        {
//            this.parameters = parameters;
//            production = new(duration: parameters.prodTime);
//        }

//        public override ULongArray TargetStoredResAmounts()
//        {
//            ULongArray answer = base.TargetStoredResAmounts();
//            if (CanStartProduction)
//                answer += parameters.demand * state.maxBatchDemResStored;
//            return answer;
//        }

//        public override ulong ReqWattsPerSec()
//        {
//            ulong answer = base.ReqWattsPerSec();
//            if (!production.Empty)
//                answer += parameters.reqWattsPerSec;
//            return answer;
//        }

//        public override Industry Update()
//        {
//            if (CanStartProduction && production.Empty && state.storedRes >= parameters.demand)
//            {
//                state.storedRes -= parameters.demand;
//                production.Enqueue(newResAmounts: parameters.supply);
//            }

//            state.waitingRes += production.DoneResAmounts();

//            return base.Update();
//        }

//        //public override void StartProduction()
//        //{
//        //    base.StartProduction();

//        //    if (!CanStartProduction)
//        //        return;

//        //    if (production.Empty && state.storedRes >= parameters.demand)
//        //    {
//        //        state.storedRes -= parameters.demand;
//        //        production.Enqueue(newResAmounts: parameters.supply);
//        //    }
//        //}

//        //public override Industry FinishProduction()
//        //{
//        //    state.waitingRes += production.DoneResAmounts();

//        //    return base.FinishProduction();
//        //}

//        public override string GetText()
//        {
//            string text = $"{parameters.name}\n";
//            if (production.Empty)
//                text += "idle";
//            else
//                text += $"producing {production.PeekCompletionProp() * 100: 0.}%";
//            if (!CanStartProduction)
//                text += "\nwill not start new";
//            return text;
//        }
//    }
//}

