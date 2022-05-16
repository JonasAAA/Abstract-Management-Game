namespace Game1
{
    [Serializable]
    public class WorldConfig
    {
        public readonly ulong personJobEnjoymentWeight, personTalentWeight, personSkillWeight, jobDesperationWeight, personToJobDistWeight;
        public readonly UDouble jobVacDespValueConsideredAverage;
        public readonly Score minAcceptablePersonScore;
        public readonly EnergyPriority linkEnergyPriority;
        public readonly UDouble standardStarRadius, scrollSpeed;
        public readonly UDouble personMinReqWatts, personMaxReqWatts;
        public readonly Propor parentContribToChildPropor;
        public readonly Propor personMomentumPropor;
        /// <summary>
        /// MUST always be the same for all people
        /// as the way industry deals with required energy requires that
        /// </summary>
        public readonly EnergyPriority personDefaultEnergyPriority;
        public readonly TimeSpan personMinSeekChangeTime, personMaxSeekChangeTime;
        public readonly UDouble personDistanceTimeCoeff, personDistanceEnergyCoeff, resDistanceTimeCoeff, resDistanceEnergyCoeff;
        public readonly int lightTextureWidthAndHeight;
        public readonly ulong lightLayer, nodeLayer, linkLayer;
        public readonly UDouble brightStarTextureBrigthness, dimStarTextureBrightness;
        public readonly UDouble startingWorldScale;
        public readonly UDouble screenBoundWidthForMapMoving;
        public readonly UDouble minSafeDist;
        public readonly ulong resDistribArrowsUILayer;
        public readonly UDouble linkWidth;
        // So gravitational force between masses M1 and M2 at distance R is gravitConst * M1 * M2 / (R ^ gravitPower)
        public readonly UDouble gravitPower, gravitConst;
        public readonly Propor desperationMemoryPropor;
        public readonly ulong minResAmountInPlanet;
        public readonly UDouble defaultIndustryHeight;

        public WorldConfig()
        {
            personJobEnjoymentWeight = 2;
            personTalentWeight = 3;
            personSkillWeight = 2;
            jobDesperationWeight = 2;
            personToJobDistWeight = 1;

            jobVacDespValueConsideredAverage = 10;
            minAcceptablePersonScore = (Score).1;
            linkEnergyPriority = new EnergyPriority(value: 10);
            standardStarRadius = 50;
            scrollSpeed = 60;

            personMomentumPropor = (Propor).2;
            personMinReqWatts = (UDouble).1;
            personMaxReqWatts = 1;
            parentContribToChildPropor = (Propor).9;
            personDefaultEnergyPriority = new EnergyPriority(value: 100);
            personMinSeekChangeTime = TimeSpan.FromSeconds(5);
            personMaxSeekChangeTime = TimeSpan.FromSeconds(30);

            personDistanceTimeCoeff = 1;
            personDistanceEnergyCoeff = 0;
            resDistanceTimeCoeff = 0;
            resDistanceEnergyCoeff = 1;

            lightTextureWidthAndHeight = 2048;
            lightLayer = 5;
            nodeLayer = 10;
            linkLayer = 0;
            brightStarTextureBrigthness = 100;
            dimStarTextureBrightness = 1;

            startingWorldScale = 1;
            screenBoundWidthForMapMoving = 10;

            minSafeDist = 1;

            resDistribArrowsUILayer = 1;

            linkWidth = 10;

            gravitPower = (UDouble)1.5;
            gravitConst = 1;

            desperationMemoryPropor = (Propor).9;

            minResAmountInPlanet = 10;

            defaultIndustryHeight = 10;
        }
    }
}
