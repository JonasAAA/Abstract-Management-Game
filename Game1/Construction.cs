using System;

namespace Game1
{
    /// <summary>
    /// TODO:
    /// desperation should decrease with increasing completion percentage
    /// </summary>
    public class Construction : Industry
    {
        public new class Params : Industry.Params
        {
            public readonly Industry.Params industrParams;
            public readonly TimeSpan duration;
            public readonly ConstULongArray cost;

            public Params(string name, double reqSkill, ulong reqWattsPerSec, Industry.Params industrParams, TimeSpan duration, ConstULongArray cost)
                : base(industryType: IndustryType.Construction, name: name, reqSkill: reqSkill, reqWattsPerSec: reqWattsPerSec, prodWattsPerSec: 0)
            {
                this.industrParams = industrParams;
                if (duration < TimeSpan.Zero)
                    throw new ArgumentException();
                this.duration = duration;
                this.cost = cost;
            }

            public override Industry MakeIndustry(NodeState state)
                => new Construction(parameters: this, state: state);
        }

        private readonly Params parameters;
        private TimeSpan? constrEndTime;

        private Construction(Params parameters, NodeState state)
            : base(parameters: parameters, state: state)
        {
            this.parameters = parameters;
            constrEndTime = null;
        }

        public override ULongArray TargetStoredResAmounts()
            => parameters.cost.ToULongArray();

        protected override bool IsBusy()
            => constrEndTime.HasValue;

        public override Industry Update()
        {
            base.Update();

            if (constrEndTime.HasValue)
                constrEndTime += (1 - CurSkillPropor) * C.ElapsedGameTime;

            if (constrEndTime is null && state.storedRes >= parameters.cost)
            {
                state.storedRes -= parameters.cost;
                constrEndTime = C.TotalGameTime + parameters.duration;
            }

            if (constrEndTime.HasValue && constrEndTime <= C.TotalGameTime)
            {
                state.unemployedPeople.AddRange(state.employees);
                state.employees.Clear();
                foreach (var person in state.travelingEmployees)
                    person.StopTravelling();
                state.travelingEmployees.Clear();
                return parameters.industrParams.MakeIndustry(state: state);
            }
            return this;
        }

        public override string GetText()
            => base.GetText() + constrEndTime switch
            {
                null => "waiting to start costruction",
                not null => $"constructing {C.DonePart(endTime: constrEndTime.Value, duration: parameters.duration) * 100: 0.}%"
            };
    }
}
