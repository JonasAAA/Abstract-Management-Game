using System.Runtime.Serialization;

namespace Game1.Industries
{
    [DataContract]
    public abstract class BuildingIndustry : Industry
    {
        [DataContract]
        public abstract new class Params : Industry.Params
        {
            protected Params(IndustryType industryType, string name, ulong energyPriority, double reqSkill, string explanation)
                : base(industryType: industryType, name: name, energyPriority: energyPriority, reqSkill: reqSkill, explanation: explanation)
            { }

            public abstract override BuildingIndustry MakeIndustry(NodeState state);
        }

        protected BuildingIndustry(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        { }
    }
}
