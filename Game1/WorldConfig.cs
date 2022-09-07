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
        public readonly EnergyPriority
            linkEnergyPriority = new(value: 10);
        public readonly UDouble
            standardStarRadius = 50,
            scrollSpeed = 60;
        public readonly UDouble
            personMinReqWatts = (UDouble).1,
            personMaxReqWatts = 1;
        public readonly Propor
            parentContribToChildPropor = (Propor).9;
        public readonly Score
            startingHappiness = Score.CreateOrThrow(value: .2);
        public readonly TimeSpan
            happinessDifferenceHalvingDuration = TimeSpan.FromSeconds(10);
        public readonly Propor
            actualSkillHappinessWeight = (Propor).2;
        // Used to calculate score for each potential activity
        public readonly ulong
            personInertiaWeight = 1,
            personEnjoymentWeight = 10,
            personTravelCostWeight = 2;
        /// <summary>
        /// MUST always be the same for all people
        /// as the way industry deals with required energy requires that
        /// </summary>
        public readonly EnergyPriority
            personDefaultEnergyPriority = new(value: 100);
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
            brightStarTextureBrigthness = 100,
            dimStarTextureBrightness = 1,
            startingWorldScale = 1,
            screenBoundWidthForMapMoving = 10,
            minSafeDist = 1;
        public readonly ulong
            resDistribArrowsUILayer = 1;
        public readonly UDouble
            linkWidth = 10;
        // So gravitational force between masses M1 and M2 at distance R is gravitConst * M1 * M2 / (R ^ gravitExponent)
        public readonly UDouble
            gravitExponent = (UDouble)1.5,
            gravitConst = 1;
        public readonly Propor
            desperationMemoryPropor = (Propor).9;
        public readonly ulong
            minResAmountInPlanet = 100;
        public readonly UDouble
            defaultIndustryHeight = 10;

        public readonly Color
            cosmosBackgroundColor = Color.Black,
            houseIndustryColor = Color.Yellow,
            linkTravellerColor = Color.Black;
        public readonly UDouble
            linkTravelSpeed = (UDouble)100,
            linkJoulesPerNewtonOfGravity = (UDouble).001,
            linkJoulesPerMeterOfDistance = (UDouble).00001;
    }
}
