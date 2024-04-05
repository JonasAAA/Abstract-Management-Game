using Game1.Collections;
using Game1.GlobalTypes;
using Game1.Industries;

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
                    RawMaterialID.Fifthium => 3,
                    RawMaterialID.Sixthium => 2
                }
            );

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
                temperature: Temperature.CreateFromK(valueInK: 200 + rawMatID.Ind() * 100),
                // basically further raw materials will have more extreme minimums
                resistivity: Algorithms.Interpolate
                (
                    normalized: Algorithms.Normalize(value: rawMatID.Ind(), start: 0, stop: RawMaterialIDUtil.lastRawMatID.Ind()),
                    start: RawMatResistivityMid(rawMatID: rawMatID),
                    stop: Propor.empty
                )
            );

        // Only public to be testable
        public static Propor RawMatResistivityMid(RawMaterialID rawMatID)
            => (Propor)((2.0 + (rawMatID.Ind() % 2)) / 5);

        // Only public to be testable
        public static (Temperature temperature, Propor resistivity) RawMatResistivityMax(RawMaterialID rawMatID)
            =>
            (
                temperature: Temperature.CreateFromK(valueInK: 50 + rawMatID.Ind() * 100),
                // basically further raw materials will have more extreme maximums
                resistivity: Algorithms.Interpolate
                (
                    normalized: Algorithms.Normalize(value: rawMatID.Ind(), start: 0, stop: RawMaterialIDUtil.lastRawMatID.Ind()),
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
                double scaledTemperDiff = ((double)temperature.valueInK - resistPoint.temperature.valueInK) / 50;
                return ((double)resistPoint.resistivity - (double)midResistivity) / (1 + scaledTemperDiff * scaledTemperDiff);
            }
        }

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

        private static UDouble BaseElectricalEnergyPerUnitAreaPhys(MaterialPalette electronicsMatPalette, Temperature temperature)
            => (UDouble)0.1 * Resistivity
            (
                material: electronicsMatPalette.materialChoices[MaterialPurpose.electricalConductor],
                temperature: temperature
            );

        /// <summary>
        /// Electrical energy needed to use/produce unit area of physical result.
        /// relevantMass here is the exact same thing as in MaxMechThroughput function
        /// </summary>
        private static UDouble ElectricalEnergyPerUnitAreaPhys(Propor electronicsProporInBuilding, MaterialPalette electronicsMatPalette, SurfaceGravity gravity, Temperature temperature, UDouble relevantMassPUBA)
#warning make this depend on complexity of production
            => 10 /* amount of useful work. */
                * (1 + electronicsProporInBuilding * BaseElectricalEnergyPerUnitAreaPhys(electronicsMatPalette: electronicsMatPalette, temperature: temperature))
                * (1 + (UDouble)0.1 * gravity.valueInMetPerSeqSq * relevantMassPUBA);

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

        public static Propor NeededElectricity(MaterialPalette materialPalette, SurfaceGravity gravity)
            => materialPalette.productClass.SwitchExpression
            (
                mechanical: () => (Propor).5,
                electronics: () => (Propor)1
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
        public static Propor TentativeNeededElectricity(SurfaceGravity gravity, Propor chosenTotalPropor, EfficientReadOnlyDictionary<ProductClass, MaterialPalette> matPaletteChoices,
            EfficientReadOnlyDictionary<ProductClass, Propor> buildingProdClassPropors)
            => TentativeStats(input: gravity, matPaletteStats: NeededElectricity, chosenTotalPropor: chosenTotalPropor, matPaletteChoices: matPaletteChoices, buildingProdClassPropors: buildingProdClassPropors);

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

        private static readonly UDouble neededElectricityFactor = (UDouble)0.0000001;

        /// <summary>
        /// The number of supplied buildings (assuming that the power plant has throughput of .5, the buildings have neededEnergy .5,
        /// and power plant and each building have the same building area).
        /// </summary>
        private static readonly UDouble powerPlantSuppliedBuildings = 30;

        private static UDouble CurReqWatts(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices, SurfaceGravity gravity, AreaDouble buildingArea)
            => buildingArea.valueInMetSq * TentativeNeededElectricity
            (
                gravity: gravity,
                chosenTotalPropor: Propor.full,
                matPaletteChoices: buildingMatPaletteChoices.Choices,
                buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
            ) * neededElectricityFactor;

        /// <summary>
        /// Mechanical production stats
        /// </summary>
        public static Result<MechProdStats, TextErrors> CurMechProdStatsOrPauseReasons(BuildingComponentsToAmountPUBA buildingComponentsToAmountPUBA, BuildingCostPropors buildingCostPropors,
            MaterialPaletteChoices buildingMatPaletteChoices, SurfaceGravity gravity, Temperature temperature, AreaDouble buildingArea, Mass productionMass)
        {
            var producedAreaPerSec = buildingArea * TentativeThroughput
            (
                temperature: temperature,
                chosenTotalPropor: Propor.full,
                matPaletteChoices: buildingMatPaletteChoices.Choices,
                buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
            ) * (UDouble)0.01;
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
                            buildingArea: buildingArea
                        ),
                        ProducedAreaPerSec: producedAreaPerSec
                    )
                )
            };
        }

        /// <summary>
        /// Note that this returns max production params as it doesn't know how much radiant energy is available to be transformed
        /// </summary>
        public static PowerPlantProdStats CurPowerPlantProdStats(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            SurfaceGravity gravity, Temperature temperature, AreaDouble buildingArea)
#warning Complete this
            => new
            (
                ReqWatts: CurReqWatts
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: gravity,
                    buildingArea: buildingArea
                ),
                ProdWatts: buildingArea.valueInMetSq * TentativeThroughput
                (
                    temperature: temperature,
                    chosenTotalPropor: Propor.full,
                    matPaletteChoices: buildingMatPaletteChoices.Choices,
                    buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
                ) * (neededElectricityFactor * powerPlantSuppliedBuildings)
            );

        /// <summary>
        /// Note that this returns max production params as it doesn't know how much radiant energy is available to be transformed
        /// </summary>
        public static LightRedirectionProdStats CurLightRedirectionProdStats(BuildingCostPropors buildingCostPropors, MaterialPaletteChoices buildingMatPaletteChoices,
            SurfaceGravity gravity, Temperature temperature, AreaDouble buildingArea)
            // COPIED from power plant for now. Later, probably just want to multiply the TentativeThroughput by a bigger constant as it makes
            // sense to redirect more light then could convert to electricity
#warning Complete this
            => new
            (
                ReqWatts: CurReqWatts
                (
                    buildingCostPropors: buildingCostPropors,
                    buildingMatPaletteChoices: buildingMatPaletteChoices,
                    gravity: gravity,
                    buildingArea: buildingArea
                ),
                RedirectWatts: buildingArea.valueInMetSq * TentativeThroughput
                (
                    temperature: temperature,
                    chosenTotalPropor: Propor.full,
                    matPaletteChoices: buildingMatPaletteChoices.Choices,
                    buildingProdClassPropors: buildingCostPropors.neededProductClassPropors
                ) * (neededElectricityFactor * powerPlantSuppliedBuildings)
            );

        public static MechProdStats CurConstrStats(AllResAmounts buildingCost, SurfaceGravity gravity, Temperature temperature, TimeSpan constructionDuration)
        {
            var buildingComponentsArea = buildingCost.Area();
#warning Complete this
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
