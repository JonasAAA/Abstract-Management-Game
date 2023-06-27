//using Game1.Collections;
//using Game1.Shapes;
//using Game1.UI;
//using static Game1.WorldManager;

//namespace Game1.Industries
//{
//    [Serializable]
//    public sealed class MatSplitting : IIndustry
//    {
//        [Serializable]
//        public sealed class GeneralParams : IBuildingGeneralParams
//        {
//            public string Name { get; }
//            public EfficientReadOnlyDictionary<IMaterialPurpose, Propor> BuildingComponentMaterialPropors { get; }

//            public readonly DiskBuildingImage.Params buildingImageParams;
//            public readonly GeneralProdAndMatAmounts buildingCostPropors;
//            public readonly EnergyPriority energyPriority;

//            public GeneralParams(string name, GeneralProdAndMatAmounts buildingCostPropors, EnergyPriority energyPriority)
//            {
//                Name = name;
//                BuildingComponentMaterialPropors = buildingCostPropors.materialPropors;

//                buildingImageParams = new DiskBuildingImage.Params(finishedBuildingHeight: ResAndIndustryAlgos.DiskBuildingHeight, Color: ActiveUIManager.colorConfig.manufacturingBuildingColor);
//                if (buildingCostPropors.materialPropors[IMaterialPurpose.roofSurface].IsEmpty)
//                    throw new ArgumentException();
//                this.buildingCostPropors = buildingCostPropors;
//                if (energyPriority == EnergyPriority.mostImportant)
//                    throw new ArgumentException("Only power plants can have highest energy priority");
//                this.energyPriority = energyPriority;
//            }

//            public Result<IBuildingConcreteParams, EfficientReadOnlyHashSet<IMaterialPurpose>> CreateConcrete(IIndustryFacingNodeState nodeState, MaterialChoices neededBuildingMatChoices)
//            {
//                var buildingImage = buildingImageParams.CreateImage(nodeState);
//                return ResAndIndustryAlgos.BuildingCost
//                (
//                    buildingCostPropors: buildingCostPropors,
//                    buildingMatChoices: neededBuildingMatChoices,
//                    buildingArea: buildingImage.Area
//                ).Select<IBuildingConcreteParams>
//                (
//                    buildingCost => new ConcreteParams
//                    (
//                        nodeState: nodeState,
//                        generalParams: this,
//                        buildingImage: buildingImage,
//                        buildingCost: buildingCost,
//                        buildingMatChoices: neededBuildingMatChoices,
//                        surfaceMaterial: neededBuildingMatChoices[IMaterialPurpose.roofSurface]
//                    )
//                );
//            }
//        }

//        [Serializable]
//        public readonly struct ConcreteParams : IBuildingConcreteParams
//        {
//            public SomeResAmounts<IResource> BuildingCost { get; }

//            public readonly IIndustryFacingNodeState nodeState;
//            public readonly DiskBuildingImage buildingImage;
//            public readonly Material surfaceMaterial;
//            public readonly EnergyPriority energyPriority;
//            public readonly AreaDouble areaToSplit;

//            private readonly AreaDouble buildingArea;
//            private readonly GeneralParams generalParams;
//            private readonly MaterialChoices buildingMatChoices;

//            public ConcreteParams(IIndustryFacingNodeState nodeState, GeneralParams generalParams, DiskBuildingImage buildingImage, SomeResAmounts<IResource> buildingCost, MaterialChoices buildingMatChoices, Material surfaceMaterial)
//            {
//                BuildingCost = buildingCost;

//                this.nodeState = nodeState;
//                this.buildingImage = buildingImage;
//                this.surfaceMaterial = surfaceMaterial;
//                energyPriority = generalParams.energyPriority;
//                areaToSplit = ResAndIndustryAlgos.AreaInProduction(buildingArea: buildingImage.Area);
//                buildingArea = buildingImage.Area;
//                //maxProductAmount = ResAndIndustryAlgos.MaxAmountInProduction
//                //(
//                //    areaInProduction: ResAndIndustryAlgos.AreaInProduction(buildingArea: buildingArea),
//                //    itemTargetArea: productParams.targetArea
//                //);

//                this.generalParams = generalParams;
//                this.buildingMatChoices = buildingMatChoices;
//            }

//            /// <param Name="splittingMass">Mass of stuff in production</param>
//            public CurProdStats CurSplittingStats(Mass splittingMass)
//                => ResAndIndustryAlgos.CurMechProdStats
//                (
//                    buildingCostPropors: generalParams.buildingCostPropors,
//                    buildingMatChoices: buildingMatChoices,
//                    gravity: nodeState.SurfaceGravity,
//                    temperature: nodeState.Temperature,
//                    buildingArea: buildingArea,
//                    productionMass: splittingMass
//                );

//            IBuildingImage IIncompleteBuildingImage.IncompleteBuildingImage(Propor donePropor)
//                => buildingImage.IncompleteBuildingImage(donePropor: donePropor);

//            public IIndustry CreateIndustry(ResPile buildingResPile)
//                => new MatSplitting(buildingParams: this, buildingResPile: buildingResPile, productionParams: new(productParams: productParams));
//        }

//        [Serializable]
//        private sealed class State
//        {
//            public static (Result<State, TextErrors> state, HistoricCorrector<double> splitAreaHistoricCorrector) Create(ConcreteParams parameters, ResPile buildingResPile, HistoricCorrector<double> splitAreaHistoricCorrector, RawMatsMixAllocator splitResAllocator)
//            {
//                var minedAreaCorrectorWithTarget = splitAreaHistoricCorrector.WithTarget(target: parameters.AreaToSplit().valueInMetSq);

//                // Since will never mine more than requested, suggestion will never be smaller than parameters.AreaToMine(), thus will always be >= 0.
//                var maxAreaToMine = (UDouble)minedAreaCorrectorWithTarget.suggestion;
//                return parameters.nodeState.Mine
//                (
//                    maxArea: AreaDouble.CreateFromMetSq(valueInMetSq: maxAreaToMine),
//                    rawMatsMixAllocator: splitResAllocator
//                ).SwitchExpression<(Result<State, TextErrors> state, HistoricCorrector<double> splitAreaHistoricCorrector)>
//                (
//                    ok: miningRes =>
//                    (
//                        state: new(ok: new State(parameters: parameters, buildingResPile: buildingResPile, miningRes: miningRes)),
//                        splitAreaHistoricCorrector: minedAreaCorrectorWithTarget.WithValue(value: miningRes.Amount.rawMatsMix.Area().valueInMetSq)
//                    ),
//                    error: errors =>
//                    (
//                        state: new(errors: errors),
//                        splitAreaHistoricCorrector: new()
//                    )
//                );

//                //parameters.nodeState.Mine(maxArea: parameters.AreaToMine()).Select
//                //(
//                //    miningRes => new State(parameters: parameters, buildingResPile: buildingResPile, miningRes: miningRes)
//                //);
//            }

//            public ElectricalEnergy ReqEnergy { get; private set; }

//            public bool IsDone
//                => donePropor.IsFull;

//            private readonly ConcreteParams parameters;
//            private readonly ResPile buildingResPile, splittingRes;
//            /// <summary>
//            /// Mass in process of splitting
//            /// </summary>
//            private readonly Mass splittingMass;
//            /// <summary>
//            /// Area in process of splitting
//            /// </summary>
//            private readonly Area splittingArea;
//            private readonly EnergyPile<ElectricalEnergy> electricalEnergyPile;
//            private readonly HistoricRounder reqEnergyHistoricRounder;

//            private CurProdStats curSplittingStats;
//            private Propor donePropor, workingPropor;

//            private State(ConcreteParams parameters, ResPile buildingResPile, ResPile splittingRes)
//            {
//                this.parameters = parameters;
//                this.buildingResPile = buildingResPile;
//                this.splittingRes = splittingRes;
//                splittingMass = splittingRes.Amount.Mass();
//                splittingArea = splittingRes.Amount.RawMatComposition().Area();
//                electricalEnergyPile = EnergyPile<ElectricalEnergy>.CreateEmpty(locationCounters: parameters.nodeState.LocationCounters);
//                reqEnergyHistoricRounder = new();
//                donePropor = Propor.empty;
//            }

//            public void FrameStart()
//            {
//                curSplittingStats = parameters.CurSplittingStats(splittingMass: splittingMass);
//                ReqEnergy = ElectricalEnergy.CreateFromJoules
//                (
//                    valueInJ: reqEnergyHistoricRounder.Round
//                    (
//                        value: (decimal)curSplittingStats.ReqWatts * (decimal)CurWorldManager.Elapsed.TotalSeconds,
//                        curTime: CurWorldManager.CurTime
//                    )
//                );
//            }

//            public void ConsumeElectricalEnergy(Pile<ElectricalEnergy> source, ElectricalEnergy electricalEnergy)
//            {
//                electricalEnergyPile.TransferFrom(source: source, amount: electricalEnergy);
//                workingPropor = Propor.Create(part: electricalEnergy.ValueInJ, whole: ReqEnergy.ValueInJ)!.Value;
//            }

//            public void Update()
//            {
//                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);

//                AreaDouble areaSplit = workingPropor * (UDouble)CurWorldManager.Elapsed.TotalSeconds * curSplittingStats.ProducedAreaPerSec;
//                donePropor = Propor.CreateByClamp(value: (UDouble)donePropor + areaSplit / splittingArea.valueInMetSq);
//                if (donePropor.IsFull)
//                    parameters.nodeState.StoredResPile.TransferAllFrom(source: splittingRes);
//            }

//            public void Delete()
//            {
//                parameters.nodeState.ThermalBody.TransformAllEnergyToHeatAndTransferFrom(source: electricalEnergyPile);
//                parameters.nodeState.StoredResPile.TransferAllFrom(source: buildingResPile);
//                parameters.nodeState.EnlargeFrom(source: splittingRes, amount: splittingRes.Amount.RawMatComposition());
//            }
//        }
//    }
//}
