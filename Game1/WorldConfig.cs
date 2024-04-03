using Game1.GlobalTypes;
using Game1.Shapes;
using static Game1.GlobalTypes.GameConfig;

namespace Game1
{
    [Serializable]
    public sealed class WorldConfig
    {
        // Used to calculate how job values person
        public readonly ulong
            personJobEnjoymentWeight = 2,
            personTalentWeight = 3,
            personSkillWeight = 2,
            jobDesperationWeight = 2,
            personToJobDistWeight = 1;
        public readonly UDouble
            jobVacDespValueConsideredAverage = 10;
        public readonly Score
            minAcceptablePersonScore = Score.CreateOrThrow(value: .1);
        public readonly ulong
            personMinReqWatts = 1,
            personMaxReqWatts = 10;
        public readonly Propor
            parentContribToChildPropor = (Propor).9;
        public readonly Score
            startingHappiness = Score.CreateOrThrow(value: .2);
        public readonly TimeSpan
            happinessDifferenceHalvingDuration = TimeSpan.FromSeconds(10);
        public readonly Propor
            productivityHappinessWeight = (Propor).2;
        // Used to calculate score for each potential activity
        public readonly ulong
            personInertiaWeight = 1,
            personEnjoymentWeight = 10,
            personTravelCostWeight = 2;
        public readonly TimeSpan
            personMinSeekChangeTime = TimeSpan.FromSeconds(5),
            personMaxSeekChangeTime = TimeSpan.FromSeconds(30);
        public readonly UDouble
            personDistanceTimeCoeff = 1,
            personDistanceEnergyCoeff = 0,
            resDistanceTimeCoeff = 0,
            resDistanceEnergyCoeff = 1;
        public readonly int
            lightTextureWidthAndHeight = 2048;
        public readonly ulong
            lightLayer = 5,
            nodeLayer = 10,
            linkLayer = 0;
        public readonly Length
            startingPixelLength = Length.CreateFromM(200000);
        public readonly Length
            minSafeDist = Length.CreateFromM(10);
        public readonly Length linkWidth, diskBuildingHeight;
        // So gravitational force between masses M1 and M2 at distance R is gravitConst * M1 * M2 / (R ^ gravitExponent)
        public readonly UDouble
            gravitExponent = 1,
            gravitConst;
        public readonly Propor
            desperationMemoryPropor = (Propor).9;
        public readonly AreaInt
            minPlanetArea;
        public readonly ulong
            startingPersonNumInHouseCosmicBody = 15,
            startingPersonNumInPowerPlantCosmicBody = 5;
        public readonly UDouble
            linkTravelSpeed,
            linkJoulesPerUnitGravitAccel,
            linkJoulesPerMeterOfDistance;

        public readonly EnergyPriority
            linkEnergyPrior = new(value: 90),
            industryConstructionEnergyPrior = new(value: 90),
            industryOperationEnergyPrior = new(value: 80),
            reprodIndustryOperationEnergyPrior = new(value: 89),
            // MUST always be the same for all people
            // as the way industry deals with required energy requires that
            personEnergyPrior = new(value: 10);

        public readonly ulong
            worldSecondsInGameSecond = 3600;

        public readonly Propor
            planetTransformRadiantToElectricalEnergyPropor = Propor.empty; //(Propor).001;

        public readonly UDouble
            fusionReactionTemperatureExponent = 2,
            fusionReactionSurfaceGravityExponent = 2,
            fusionReactionStrengthCoeff;

        /// <summary>
        /// I.e. energy dissipation factor
        /// </summary>
        public readonly UDouble stefanBoltzmannConstant;
        public readonly ulong temperatureExponentInStefanBoltzmannLaw = 4;
        public readonly Temperature
            startingTemperature = Temperature.CreateFromK(valueInK: 250),
            allHeatMaxTemper = Temperature.CreateFromK(valueInK: 500),
            halfHeatTemper = Temperature.CreateFromK(valueInK: 1000);
        public readonly UDouble
            heatEnergyDropoffExponent = 2;
        public readonly AreaInt minUsefulBuildingComponentAreaToRemove = AreaInt.CreateFromMetSq(valueInMetSq: 30);
        public readonly Propor
            buildingComponentsProporOfBuildingArea = (Propor).2,
            productionProporOfBuildingArea = (Propor).05,
            inputStorageProporOfBuildingArea = (Propor).1,
            outputStorageProporOfBuildingArea = (Propor).1,
            storageProporOfBuildingAreaForStorageIndustry = (Propor).6,
            buildingComponentStorageProporOfInputStorageArea = (Propor).1;

        public readonly ulong magicUnlimitedStartingMaterialCount = ulong.MaxValue / 100;

        public readonly Temperature maxTemperatureShownInGraphs = Temperature.CreateFromK(valueInK: 3000);
        public readonly SurfaceGravity maxGravityShownInGraphs;

        public WorldConfig()
        {
            // Below idea works as basically the scaling metersPerStartingPixel by factor 10 means rewriting eveything
            // in terms of centimeters instead of meters. Considering kilograms as well because when distance increases
            // by some factor, area (and thus mass and energy) increases by that factor squared
            // All these factors could probably just be read from the units of measurement - if m^C*(kg)^D*J^E,
            // then need to multiply the constant by metersPerPixel^(C+2D+2E).
            // BE careful not to use real-world units though as my game is 2 dimensional

            // Since [linkWidth] ~ m
            linkWidth = startingPixelLength * CurGameConfig.linkPixelWidth;
            // Since [diskBuildingHeight] ~ m
            diskBuildingHeight = startingPixelLength * CurGameConfig.linkPixelWidth;
            // Since [minPlanetArea] ~ m^2
            minPlanetArea = DiskAlgos.Area(radius: startingPixelLength * CurGameConfig.minPlanetPixelRadius).RoundDown();
            // Even the smallest planets should be able to produce products.
            // Thus they must be able to hold all needed inputs in production.
            // *2 part is just to be sure that things like rounding errors will not make the number too small
            Debug.Assert
            (
                (DiskBuildingImage.ComputeBuildingArea
                (
                    planetArea: minPlanetArea,
                    buildingHeight: diskBuildingHeight
                ) * productionProporOfBuildingArea).RoundDown() >= 2 * ResAndIndustryAlgos.blockArea * ResAndIndustryAlgos.productRecipeInputAmountMultiple
            );

            // Since [gravitConst] ~ m^(1+gravitExponent)/kg
            gravitConst = MyMathHelper.Pow(@base: startingPixelLength.valueInM, exponent: (double)gravitExponent - 1);
            // Since [fusionReactionStrengthCoeff] ~ m^(-fusionReactionSurfaceGravityExponent)
            fusionReactionStrengthCoeff = MyMathHelper.Pow(@base: startingPixelLength.valueInM, exponent: -fusionReactionSurfaceGravityExponent);
            // Since [stefanBoltzmannConstant] ~ J/m
            stefanBoltzmannConstant = startingPixelLength.valueInM * (UDouble).000000000000000001;
            
            // Since [linkTravelSpeed] ~ m
            linkTravelSpeed = startingPixelLength.valueInM * (UDouble)0.01;
            // Since [linkJoulesPerNewtonOfGravity] ~ J/m
            linkJoulesPerUnitGravitAccel = startingPixelLength.valueInM * (UDouble).00000000000000001;
            // Since [linkJoulesPerMeterOfDistance] ~ J/m
            linkJoulesPerMeterOfDistance = startingPixelLength.valueInM * (UDouble).0000000000000000001;

            // Since [maxGravityShownInGraphs] ~ m
            maxGravityShownInGraphs = SurfaceGravity.CreateFromMetPerSecSq(startingPixelLength.valueInM * 1500);
        }
    }
}
