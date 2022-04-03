using Game1.PrimitiveTypeWrappers;

namespace Game1
{
    [Serializable]
    public class WorldConfig
    {
        public readonly double personJobEnjoymentCoeff, personTalentCoeff, personSkillCoeff, jobDesperationCoeff, playerToJobDistCoeff, minAcceptablePersonScore, personTimeSkillCoeff, jobVacDespCoeff;
        public readonly EnergyPriority linkEnergyPriority;
        public readonly float standardStarRadius, scrollSpeed;
        public readonly double personMomentumCoeff, personMinReqWatts, personMaxReqWatts, randConrtribToChild, parentContribToChild;
        /// <summary>
        /// MUST always be the same for all people
        /// as the way industry deals with required energy requires that
        /// </summary>
        public readonly EnergyPriority personDefaultEnergyPriority;
        public readonly TimeSpan personMinSeekChangeTime, personMaxSeekChangeTime;
        public readonly double personDistanceTimeCoeff, personDistanceEnergyCoeff, resDistanceTimeCoeff, resDistanceEnergyCoeff;
        public readonly int lightTextureWidth;
        public readonly ulong lightLayer, nodeLayer, linkLayer;
        public readonly double brightStarTextureBrigthness, dimStarTextureBrightness;
        public readonly double startingWorldScale;
        public readonly float screenBoundWidthForMapMoving;
        public readonly float minSafeDist;
        public readonly ulong resDistribArrowsUILayer;
        public readonly UFloat linkWidth;
        public readonly double planetMassPerUnitArea;
        // So gravitational force between masses M1 and M2 at distance R is gravitConst * M1 * M2 / (R ^ gravitPower)
        public readonly double gravitPower, gravitConst;

        public WorldConfig()
        {
            personJobEnjoymentCoeff = .2;
            personTalentCoeff = .3;
            personSkillCoeff = .2;
            playerToJobDistCoeff = .1;
            jobDesperationCoeff = .2;
            minAcceptablePersonScore = .2;
            personTimeSkillCoeff = .1;
            jobVacDespCoeff = .1;
            linkEnergyPriority = new EnergyPriority(energyPriority: 10);
            standardStarRadius = 50;
            scrollSpeed = 60;

            personMomentumCoeff = .2;
            personMinReqWatts = .1;
            personMaxReqWatts = 1;
            randConrtribToChild = .1;
            parentContribToChild = 1 - randConrtribToChild;
            personDefaultEnergyPriority = new EnergyPriority(energyPriority: 100);
            personMinSeekChangeTime = TimeSpan.FromSeconds(5);
            personMaxSeekChangeTime = TimeSpan.FromSeconds(30);

            personDistanceTimeCoeff = 1;
            personDistanceEnergyCoeff = 0;
            resDistanceTimeCoeff = 0;
            resDistanceEnergyCoeff = 1;

            lightTextureWidth = 2048;
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

            planetMassPerUnitArea = 1;

            gravitPower = 1.5;
            gravitConst = 1;
        }
    }
}
