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
        public readonly UDouble
            standardStarRadius = 100;
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
        public readonly UDouble
            brightStarTextureBrigthness = (UDouble)1.2,
            dimStarTextureBrightness = (UDouble).6,
            metersPerStartingPixel = 100,
            screenBoundWidthForMapMoving = 10,
            scrollSpeed = 60,
            minSafeDist = 10;
        public readonly ulong
            resDistribArrowsUILayer = 1;
        public readonly UDouble linkWidth;
        // So gravitational force between masses M1 and M2 at distance R is gravitConst * M1 * M2 / (R ^ gravitExponent)
        public readonly UDouble
            gravitExponent = 1,
            gravitConst = 1;
        public readonly Propor
            desperationMemoryPropor = (Propor).9;
        public readonly AreaInt
            minPlanetArea = AreaInt.CreateFromMetSq(valueInMetSq: 100);
        public readonly UDouble
            defaultIndustryHeight = 500;
        public readonly ulong
            startingPersonNumInHouseCosmicBody = 15,
            startingPersonNumInPowerPlantCosmicBody = 5;
        public readonly UDouble
            linkTravelSpeed = (UDouble)100,
            linkJoulesPerNewtonOfGravity = (UDouble).001,
            linkJoulesPerMeterOfDistance = (UDouble).00001;

        public readonly ulong
            energyInJPerKgOfMass = 1000000;

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
            planetTransformRadiantToElectricalEnergyPropor = (Propor).001;

        public readonly UDouble stefanBoltzmannConstant = (UDouble).0000001;
        public readonly ulong temperatureExponentInStefanBoltzmannLaw = 4;
        public readonly Temperature
            allHeatMaxTemper = Temperature.CreateFromK(valueInK: 500),
            halfHeatTemper = Temperature.CreateFromK(valueInK: 2000);
        public readonly UDouble
            heatEnergyDropoffExponent = 2,
            reactionStrengthCoeff = (UDouble)0.000000000001;
        public readonly Propor
            nonReactingProporForUnitReactionStrengthUnitTime = (Propor)0.99;
        public readonly AreaInt minUsefulBuildingComponentAreaToRemove = AreaInt.CreateFromMetSq(valueInMetSq: 30);

        public WorldConfig()
        {
            linkWidth = 10 * metersPerStartingPixel;
        }
    }
}
