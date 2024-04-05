using Game1.Collections;
using Game1.GlobalTypes;
using Game1.Industries;
using Game1.PrimitiveTypeWrappers;

namespace Game1
{
    public static class ResAndIndustryAlgos
    {
        /// <summary>
        /// Primitive material is material composed from single raw material
        /// </summary>
        public static string PrimitiveMaterialName(RawMaterialID rawMatID)
            => $"{rawMatID.Name()} material";

        // Want max density to be 1
        public static readonly AreaInt rawMaterialArea = AreaInt.CreateFromMetSq(valueInMetSq: RawMaterialMass(rawMatID: 0).valueInKg);

        // Formula is like this for the following reasons:
        // * Want max mass to be quite small in order to have larger amount of blocks in the world
        // * Want mass to decrease with ind
        // * Want subsequent raw mat masses to have not-so-close of a ratio
        // CHANGING masses would still get the same amount of energy released in fusion per unit area, ASSUMING that gravity stays the same
        public static Mass RawMaterialMass(RawMaterialID rawMatID)
            => Mass.CreateFromKg
            (
                rawMatID switch
                {
                    RawMaterialID.Firstium => 12,
                    RawMaterialID.Secondium => 8,
                    RawMaterialID.Thirdium => 6,
                    RawMaterialID.Fourthium => 4,
                    //RawMaterialID.Fifthium => 3,
                    //RawMaterialID.Sixthium => 2
                }
            );

        private static readonly Mass maxRawMatMass = Enum.GetValues<RawMaterialID>().Max(RawMaterialMass);

        // The same area of stuff will always have thse same heat capacity
        // As said in https://en.wikipedia.org/wiki/Specific_heat_capacity#Monatomic_gases
        // heat capacity per mole is the same for all monatomic gases
        // That's because the atoms have nowhere else to store energy, other than in kinetic energy (and hence temperature)
        public static HeatCapacity RawMaterialHeatCapacity(RawMaterialID rawMatID)
#warning Make this more interesting?
            => HeatCapacity.CreateFromJPerK(valueInJPerK: 1);

        // Want the amount of energy generated from fusion unit area to be proportional to 1 / (ind + 1),
        // i.e. decreasing with ind quite fast
        public static UDouble RawMaterialFusionReactionStrengthCoeff(RawMaterialID rawMatID)
            => rawMatID.Next() switch
            {
                RawMaterialID nextRawMatID => (UDouble)0.00000000000000000005 * rawMaterialArea.valueInMetSq / ((RawMaterialMass(rawMatID: rawMatID) - RawMaterialMass(rawMatID: nextRawMatID)).valueInKg * (rawMatID.Ind() + 1)),
                null => 0
            };

        public const ulong energyInJPerKgOfMass = 1;

        public const ulong maxProductIndInClass = 9;

        // Means all products must be from 1, 2, 3, or 6 components (some of which could be the same)
        public const ulong productRecipeInputAmountMultiple = 6;

        public static readonly ulong materialCompositionDivisor = ProductRawMatCompositionDivisor(prodIndInClass: 0) * productRecipeInputAmountMultiple;

        public static ulong ProductRawMatCompositionDivisor(ulong prodIndInClass)
            => MyMathHelper.Pow(productRecipeInputAmountMultiple, maxProductIndInClass - prodIndInClass);

        public static readonly AreaInt blockArea = rawMaterialArea * 100 * materialCompositionDivisor;

        private const ulong temperatureScaling = 10000;

        public static Temperature CalculateTemperature(HeatEnergy heatEnergy, HeatCapacity heatCapacity)
            => Temperature.CreateFromK(valueInK: temperatureScaling * (UDouble)heatEnergy.ValueInJ() / heatCapacity.valueInJPerK);

        public static HeatEnergy HeatEnergyFromTemperature(Temperature temperature, HeatCapacity heatCapacity)
            => HeatEnergy.CreateFromJoules(valueInJ: MyMathHelper.Round(heatCapacity.valueInJPerK * temperature.valueInK / temperatureScaling));

        public static RawMatAmounts CreateMatCompositionFromRawMatPropors(RawMatAmounts rawMatPropors)
        {
            ulong totalWeights = rawMatPropors.Sum(resAmount => resAmount.amount);
            if (totalWeights is 0)
                throw new ArgumentException();
            if (totalWeights > 20)
                throw new ArgumentException();
            ulong smallCompositionsInBlock = materialCompositionDivisor;
            Debug.Assert(blockArea.valueInMetSq % (smallCompositionsInBlock * rawMaterialArea.valueInMetSq) is 0);
            ulong rawMatTotalAmountInSmallComposition = blockArea.valueInMetSq / (smallCompositionsInBlock * rawMaterialArea.valueInMetSq);

            RawMatAmounts smallComposition = new
            (
                resAmounts: rawMatPropors.Select
                (
                    rawMatAmount => new ResAmount<RawMaterial>
                    (
                        res: rawMatAmount.res,
                        amount: rawMatAmount.amount * rawMatTotalAmountInSmallComposition / totalWeights
                    )
                )
            );
            RawMatAmounts composition = smallComposition * smallCompositionsInBlock;
            Debug.Assert(composition.Area() == blockArea);
            Debug.Assert(composition.All(rawMatAmount => rawMatAmount.amount % materialCompositionDivisor is 0));
            return composition;
        }

        public static MechComplexity ProductMechComplexity(ProductClass productClass, ulong materialPaletteAmount, ulong indInClass, EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts)
#warning Complete this
            => new(complexity: 10);

        private static Propor CombineRawMatProperties(RawMatAmounts rawMatAmounts, Temperature temperature, Func<RawMaterial, Temperature, Propor> rawMatProperty)
            => rawMatAmounts.WeightedAverage
            (
                rawMatAmount =>
                (
                    weight: rawMatAmount.amount,
                    value: rawMatProperty
                    (
                        rawMatAmount.res,
                        temperature
                    )
                )
            );

        private static UDouble CombineRawMatProperties(RawMatAmounts rawMatAmounts, Temperature temperature, Func<RawMaterial, Temperature, UDouble> rawMatProperty)
            => rawMatAmounts.WeightedAverage
            (
                rawMatAmount =>
                (
                    weight: rawMatAmount.amount,
                    value: rawMatProperty
                    (
                        rawMatAmount.res,
                        temperature
                    )
                )
            );

        private static Propor RawMatStartingStrength(RawMaterialID rawMatID)
            => (Propor)((UDouble)rawMatID.Ind() / RawMaterialIDUtil.lastRawMatID.Ind());

        private static (Temperature temperature, Propor strength) RawMatMaxStrength(RawMaterialID rawMatID)
            => 
            (
                temperature: Temperature.CreateFromK(valueInK: 100 + rawMatID.Ind() * 400),
                strength: Propor.full
            );

        /// <summary>
        /// I.e. temperature after which raw material strength will be (close to) zero
        /// </summary>
        private static Temperature RawMatMeltingPoint(RawMaterialID rawMatID)
            => Temperature.CreateFromK(valueInK: 2 * RawMatMaxStrength(rawMatID: rawMatID).temperature.valueInK);

        /// <summary>
        /// Currently piecewise linear
        /// </summary>
        private static Propor RawMatStrength(RawMaterialID rawMatID, Temperature temperature)
        {
            var (maxStrengthTemper, maxStrength) = RawMatMaxStrength(rawMatID: rawMatID);
            if (temperature <= maxStrengthTemper)
                return Algorithms.Interpolate
                (
                    normalized: Algorithms.Normalize(value: temperature.valueInK, start: 0, stop: maxStrengthTemper.valueInK),
                    start: RawMatStartingStrength(rawMatID: rawMatID),
                    stop: maxStrength
                );
            var meltingPoint = RawMatMeltingPoint(rawMatID: rawMatID);
            if (temperature <= meltingPoint)
                return Algorithms.Interpolate
                (
                    normalized: Algorithms.Normalize(value: temperature.valueInK, start: maxStrengthTemper.valueInK, stop: meltingPoint.valueInK),
                    start: maxStrength,
                    stop: Propor.empty
                );
            return Propor.empty;
        }

        private static Propor Strength(Material material, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: material.RawMatComposition,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) => RawMatStrength(rawMatID: rawMat.RawMatID, temperature: temperature)
            );

        // Only public to be testable
        public static (Temperature temperature, Propor resistivity) RawMatResistivityMin(RawMaterialID rawMatID)
            =>
            (
                temperature: Temperature.CreateFromK(valueInK: 200 + rawMatID.Ind() * 1000),
                // basically further raw materials will have more extreme minimums
                resistivity: RawMatResistivityMid(rawMatID: rawMatID)
                //Algorithms.Interpolate
                //(
                //    normalized: rawMatID.Normalize(),
                //    start: RawMatResistivityMid(rawMatID: rawMatID),
                //    stop: Propor.empty
                //)
            );

        // Only public to be testable
        public static Propor RawMatResistivityMid(RawMaterialID rawMatID)
        {
            long oneOrMinusOne = 2 * ((long)rawMatID.Ind() % 2) - 1;
            UDouble betweenZeroAndOne = Algorithms.Interpolate(normalized: rawMatID.Normalized(), start: (UDouble)0.25, 1u);

            return (Propor)((1 + oneOrMinusOne * betweenZeroAndOne) / 2);
        }
        //=> rawMatID == RawMaterialID.Firstium ? Propor.empty : (Propor)((2.0 + (rawMatID.Ind() % 2)) / 5);

        // Only public to be testable
        public static (Temperature temperature, Propor resistivity) RawMatResistivityMax(RawMaterialID rawMatID)
            =>
            (
                temperature: Temperature.CreateFromK(valueInK: 50 + rawMatID.Ind() * 1000),
                // basically further raw materials will have more extreme maximums
                resistivity: Algorithms.Interpolate
                (
                    normalized: rawMatID.Normalized(),
                    start: RawMatResistivityMid(rawMatID: rawMatID),
                    stop: Propor.full
                )
            );

        public static Propor RawMatResistivity(RawMaterialID rawMatID, Temperature temperature)
        {
            var midResistivity = RawMatResistivityMid(rawMatID: rawMatID);
            return (Propor)((double)midResistivity + Bump(RawMatResistivityMin(rawMatID: rawMatID)) + Bump(RawMatResistivityMax(rawMatID: rawMatID)));

            double Bump((Temperature temperature, Propor resistivity) resistPoint)
            {
                double scaledTemperDiff = ((double)temperature.valueInK - resistPoint.temperature.valueInK) / 1000;
                return ((double)resistPoint.resistivity - (double)midResistivity) / (1 + scaledTemperDiff * scaledTemperDiff);
            }
        }

        private static Propor MaterialResistivityMid(Material material)
            => material.RawMatComposition.WeightedAverage
            (
                rawMatAmount =>
                (
                    weight: rawMatAmount.amount,
                    value: RawMatResistivityMid(rawMatID: rawMatAmount.res.RawMatID)
                )
            );

        /// <summary>
        /// Must be from 0 to 1
        /// </summary>
        private static Propor Resistivity(Material material, Temperature temperature)
            => CombineRawMatProperties
            (
                rawMatAmounts: material.RawMatComposition,
                temperature: temperature,
                rawMatProperty: static (rawMat, temperature) => RawMatResistivity(rawMatID: rawMat.RawMatID, temperature: temperature)
            );

        /// <summary>
        /// Throughput is the input/output area of building per unit time
        /// </summary>
        public static Propor Throughput(MaterialPalette materialPalette, Temperature temperature)
            => materialPalette.productClass.SwitchExpression
            (
                mechanical: () => Strength(material: materialPalette.materialChoices[MaterialPurpose.mechanical], temperature: temperature),
                electronics: () => Propor.CreateByClamp
                (
                    value: (double)Resistivity
                    (
                        material: materialPalette.materialChoices[MaterialPurpose.electricalInsulator],
                        temperature: temperature
                    ) - (double)Resistivity
                    (
                        material: materialPalette.materialChoices[MaterialPurpose.electricalConductor],
                        temperature: temperature
                    )
                )
            );

        public static Propor NeededElectricity(MaterialPalette materialPalette, SurfaceGravity gravity, SurfaceGravity maxSurfaceGravity)
            => materialPalette.productClass.SwitchExpression
            (
                // This just means the higher the gravity and the more heavy the material palette, the more electricity is necessary for that building
                mechanical: () => Algorithms.Normalize
                (
                    value: gravity.valueInMetPerSeqSq,
                    start: 0,
                    stop: maxSurfaceGravity.valueInMetPerSeqSq
                ) * Algorithms.Normalize
                (
                    value: (UDouble)materialPalette.materialAmounts.Mass().valueInKg / materialPalette.materialAmounts.Area().valueInMetSq,
                    start: 0,
                    stop: (UDouble)maxRawMatMass.valueInKg / rawMaterialArea.valueInMetSq
                ),
                electronics: () => MaterialResistivityMid(material: materialPalette.materialChoices[MaterialPurpose.electricalConductor])
            );

        private const double statsPowerMeanExponent = 0;

        /// <summary>
        /// Throughput from possibly not all mat palette choices
        /// </summary>
        public static Propor TentativeThroughput(Temperature temperature, Propor chosenTotalPropor, EfficientReadOnlyDictionary<ProductClass, MaterialPalette> matPaletteChoices,
            EfficientReadOnlyDictionary<ProductClass, Propor> buildingProdClassPropors)
            => TentativeStats(input: temperature, matPaletteStats: Throughput, chosenTotalPropor: chosenTotalPropor, matPaletteChoices: matPaletteChoices, buildingProdClassPropors: buildingProdClassPropors);

        /// <summary>
        /// Needed electricity from possibly not all mat palette choices
        /// </summary>
        public static Propor TentativeNeededElectricity(SurfaceGravity gravity, SurfaceGravity maxSurfaceGravity, Propor chosenTotalPropor, EfficientReadOnlyDictionary<ProductClass, MaterialPalette> matPaletteChoices,
            EfficientReadOnlyDictionary<ProductClass, Propor> buildingProdClassPropors)
            => TentativeStats
            (
                input: gravity,
                matPaletteStats: (matPalette, gravity) => NeededElectricity
                (
                    materialPalette: matPalette,
                    gravity: gravity,
                    maxSurfaceGravity: maxSurfaceGravity
                ),
                chosenTotalPropor: chosenTotalPropor,
                matPaletteChoices: matPaletteChoices,
                buildingProdClassPropors: buildingProdClassPropors
           );

        private static Propor TentativeStats<TInput>(TInput input, Func<MaterialPalette, TInput, Propor> matPaletteStats, Propor chosenTotalPropor,
            EfficientReadOnlyDictionary<ProductClass, MaterialPalette> matPaletteChoices, EfficientReadOnlyDictionary<ProductClass, Propor> buildingProdClassPropors)
        {
            Debug.Assert(MyMathHelper.AreClose((UDouble)chosenTotalPropor, matPaletteChoices.Keys.Sum(prodClass => (UDouble)buildingProdClassPropors[prodClass])));
            return Propor.PowerMean
            (
                args: matPaletteChoices.Select
                (
                    matPaletteChoice =>
                    (
                        weight: (Propor)((UDouble)buildingProdClassPropors[matPaletteChoice.Key] / (UDouble)chosenTotalPropor),
                        value: matPaletteStats(matPaletteChoice.Value, input)
                    )
                ),
                exponent: statsPowerMeanExponent
            );
        }

        private static readonly UDouble neededElectricityFactor = (UDouble)0.00000003;

        /// <summary>
        /// The number of supplied buildings (assuming that the power plant has throughput of .5, the buildings have neededEnergy .5,
        /// and power plant and each building have the same building area).
        /// </summary>
        private static readonly UDouble powerPlantSuppliedBuildings = 30;

        private static readonly UDouble lightRedirectionFactorOverPowerPlant = 2;

        private static UDouble CurReqWatts(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices, SurfaceGravity gravity, SurfaceGravity maxSurfaceGravity, AreaDouble buildingArea)
            => buildingArea.valueInMetSq * TentativeNeededElectricity
            (
                gravity: gravity,
                maxSurfaceGravity: maxSurfaceGravity,
                chosenTotalPropor: Propor.full,
                matPaletteChoices: buildingMatPaletteChoices.Choices,
                buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
            ) * neededElectricityFactor;

        /// <summary>
        /// Mechanical production stats
        /// </summary>
        public static Result<MechProdStats, TextErrors> CurMechProdStatsOrPauseReasons(BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA, BuildingCostPropors buildingCostPropors,
            MaterialPaletteChoices buildingMatPaletteChoices, SurfaceGravity gravity, SurfaceGravity maxSurfaceGravity, Temperature temperature, AreaDouble buildingArea, AreaDouble productionArea, TimeSpan targetProductionCycleDuration, Mass productionMass)
        {
            var producedAreaPerSec = productionArea * TentativeThroughput
            (
                temperature: temperature,
                chosenTotalPropor: Propor.full,
                matPaletteChoices: buildingMatPaletteChoices.Choices,
                buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
            ) * (UDouble)(1 / targetProductionCycleDuration.TotalSeconds);
            return producedAreaPerSec.IsZero switch
            {
                true => new(errors: new("The temperature is too high")),
                false => new
                (
                    ok: new
                    (
                        ReqWatts: CurReqWatts
                        (
                            buildingCostPropors: buildingCostPropors,
                            buildingMatPaletteChoices: buildingMatPaletteChoices,
                            gravity: gravity,
                            maxSurfaceGravity: maxSurfaceGravity,
                            buildingArea: buildingArea
                        ),
                        ProducedAreaPerSec: producedAreaPerSec
                    )
                )
            };
        }

        private static UDouble PowerPlantProdWatts(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
             Temperature temperature, AreaDouble buildingArea)
            => buildingArea.valueInMetSq * TentativeThroughput
            (
                temperature: temperature,
                chosenTotalPropor: Propor.full,
                matPaletteChoices: buildingMatPaletteChoices.Choices,
                buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
            ) * (neededElectricityFactor * powerPlantSuppliedBuildings);

        /// <summary>
        /// Note that this returns max production params as it doesn't know how much radiant energy is available to be transformed
        /// </summary>
        public static PowerPlantProdStats CurPowerPlantProdStats(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            SurfaceGravity gravity, SurfaceGravity maxSurfaceGravity, Temperature temperature, AreaDouble buildingArea)
            => new
            (
                ReqWatts: CurReqWatts
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: gravity,
                    maxSurfaceGravity: maxSurfaceGravity,
                    buildingArea: buildingArea
                ),
                ProdWatts: PowerPlantProdWatts
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    temperature: temperature,
                    buildingArea: buildingArea
                )
            );

        /// <summary>
        /// Note that this returns max production params as it doesn't know how much radiant energy is available to be transformed
        /// </summary>
        public static LightRedirectionProdStats CurLightRedirectionProdStats(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            SurfaceGravity gravity, SurfaceGravity maxSurfaceGravity, Temperature temperature, AreaDouble buildingArea)
            => new
            (
                ReqWatts: CurReqWatts
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: gravity,
                    maxSurfaceGravity: maxSurfaceGravity,
                    buildingArea: buildingArea
                ),
                RedirectWatts: PowerPlantProdWatts
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    temperature: temperature,
                    buildingArea: buildingArea
                ) * lightRedirectionFactorOverPowerPlant
            );

        public static MechProdStats CurConstrStats(AllResAmounts buildingCost, SurfaceGravity gravity, Temperature temperature, TimeSpan constructionDuration)
        {
            var buildingComponentsArea = buildingCost.Area();
            return new
            (
                ReqWatts: buildingComponentsArea.valueInMetSq * neededElectricityFactor,
                ProducedAreaPerSec: buildingComponentsArea.ToDouble() * (1 / (UDouble)constructionDuration.TotalSeconds)
            );
        }

        public static ulong MaxAmount(AreaDouble availableArea, AreaInt itemArea)
            => (ulong)availableArea.valueInMetSq / itemArea.valueInMetSq;
    }
}
