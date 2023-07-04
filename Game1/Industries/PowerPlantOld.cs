//using static Game1.WorldManager;

//namespace Game1.Industries
//{
//    [Serializable]
//    public sealed class PowerPlant : ProductiveIndustry, IEnergyProducer
//    {
//        [Serializable]
//        public new sealed class Factory : ProductiveIndustry.Factory, IFactoryForIndustryWithBuilding
//        {
//            public readonly Propor conversionPropor;
//            private readonly ResAmounts buildingCostPerUnitSurface;

//            public Factory(string Name, UDouble reqSkillPerUnitSurface, Propor conversionPropor, ResAmounts buildingCostPerUnitSurface)
//                : base
//                (
//                    industryType: IndustryType.PowerPlant,
//                    energyPriority: EnergyPriority.mostImportant,
//                    Name: Name,
//                    Color: Color.Blue,
//                    reqSkillPerUnitSurface: reqSkillPerUnitSurface
//                )
//            {
//                if (conversionPropor.IsCloseTo(other: Propor.empty))
//                    throw new ArgumentOutOfRangeException();
//                this.conversionPropor = conversionPropor;
//                if (buildingCostPerUnitSurface.IsEmpty())
//                    throw new ArgumentException();
//                this.buildingCostPerUnitSurface = buildingCostPerUnitSurface;
//            }

//            public override GeneralParams CreateParams(IIndustryFacingNodeState state)
//                => new(state: state, factory: this);

//            ResAmounts IFactoryForIndustryWithBuilding.BuildingCost(IIndustryFacingNodeState state)
//                => state.SurfaceLength * buildingCostPerUnitSurface;

//            Industry IFactoryForIndustryWithBuilding.CreateIndustry(IIndustryFacingNodeState state, BuildingShape building)
//                => new PowerPlant(parameters: CreateParams(state: state), building: building);
//        }

//        [Serializable]
//        public new sealed class GeneralParams : ProductiveIndustry.GeneralParams
//        {
//            public readonly Propor conversionPropor;

//            // TODO: may improve the tooltip text by showing the actual produced amount
//            public override string TooltipText
//                => $"""
//                {base.TooltipText}
//                {nameof(conversionPropor)}: {conversionPropor}
//                """;

//            public GeneralParams(IIndustryFacingNodeState state, Factory factory)
//                : base(state: state, factory: factory)
//            {
//                conversionPropor = factory.conversionPropor;
//            }
//        }

//        public override bool PeopleWorkOnTop
//            => false;

//        protected override UDouble Height
//            => CurWorldConfig.defaultIndustryHeight;

//        private readonly GeneralParams parameters;
//        private ElectricalEnergy prodEnergy;
//        private readonly HistoricRounder producedEnergyRounder;
//        private readonly CachedValue<Propor> radiantToElectricalEnergyProporCached;

//        private PowerPlant(GeneralParams parameters, BuildingShape building)
//            : base(parameters: parameters, building: building)
//        {
//            this.parameters = parameters;
//            prodEnergy = ElectricalEnergy.zero;
//            producedEnergyRounder = new();
//            radiantToElectricalEnergyProporCached = new();

//            CurWorldManager.AddEnergyProducer(energyProducer: this);
//        }

//        protected override BoolWithExplanationIfFalse CalculateIsBusy()
//            => base.CalculateIsBusy() & BoolWithExplanationIfFalse.Create
//            (
//                value: parameters.state.RadiantEnergyPile.Amount.ValueInJ * RadiantToElectricalEnergyPropor >= combinedEnergyConsumer.ReqEnergy().ValueInJ + 1,
//                explanationIfFalse: "Don't get enough starlight to function"
//            );

//        private Propor RadiantToElectricalEnergyPropor
//            => radiantToElectricalEnergyProporCached.Get
//            (
//                computeValue: () => parameters.conversionPropor * CurSkillPropor,
//                curTime: CurWorldManager.CurTime
//            );

//        public override ResAmounts TargetStoredResAmounts()
//            => ResAmounts.empty;

//        protected override PowerPlant InternalUpdate(Propor workingPropor)
//        {
//            if ((bool)IsBusy() && !MyMathHelper.AreClose(workingPropor, CurSkillPropor))
//                throw new Exception();
//            return this;
//        }

//        protected override string GetBusyInfo()
//            => $"produce {prodEnergy.ValueInJ / CurWorldManager.Elapsed.TotalSeconds:0.##} W\n";

//        protected override UDouble ReqWatts()
//            => 0;

//        void IEnergyProducer.ProduceEnergy(EnergyPile<ElectricalEnergy> destin)
//        {
//            prodEnergy = (bool)IsBusy() switch
//            {
//                true => parameters.state.RadiantEnergyPile.TransformProporTo
//                (
//                    destin: destin,
//                    propor: RadiantToElectricalEnergyPropor,
//                    amountToTransformRoundFunc: amount => producedEnergyRounder.Round(value: amount, curTime: CurWorldManager.CurTime)
//                ),
//                false => ElectricalEnergy.zero
//            };
//        }
//    }
//}