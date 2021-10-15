using System;

namespace Game1
{
    public class WorldConfig
    {
        public readonly double personJobEnjoymentCoeff, personTalentCoeff, personSkillCoeff, jobDesperationCoeff, PlayerToJobDistCoeff, minAcceptablePersonScore, personTimeSkillCoeff, jobVacDespCoeff;
        public readonly ulong linkEnergyPriority;
        public readonly float standardStarRadius, scrollSpeed;
        public readonly double personMomentumCoeff, personMinReqWatts, personMaxReqWatts, randConrtribToChild, parentContribToChild;
        /// <summary>
        /// MUST always be the same for all people
        /// as the way industry deals with required energy requires that
        /// </summary>
        public readonly ulong personDefaultEnergyPriority;
        public readonly TimeSpan personMinSeekChangeTime, personMaxSeekChangeTime;
        public readonly double personDistanceTimeCoeff, personDistanceEnergyCoeff, resDistanceTimeCoeff, resDistanceEnergyCoeff;
        public readonly int lightTextureWidth;
        public readonly ulong lightLayer, nodeLayer, linkLayer;
        public readonly double brightStarTextureBrigthness, dimStarTextureBrightness;

        public readonly double startingWorldScale;
        public readonly float screenBoundWidthForMapMoving;

        public WorldConfig()
        {
            personJobEnjoymentCoeff = .2;
            personTalentCoeff = .1;
            personSkillCoeff = .2;
            PlayerToJobDistCoeff = .1;
            jobDesperationCoeff = .4;
            minAcceptablePersonScore = .4;
            personTimeSkillCoeff = .1;
            jobVacDespCoeff = .1;
            linkEnergyPriority = 10;
            standardStarRadius = 50;
            scrollSpeed = 60;

            personMomentumCoeff = .2;
            personMinReqWatts = .1;
            personMaxReqWatts = 1;
            randConrtribToChild = .1;
            parentContribToChild = 1 - randConrtribToChild;
            personDefaultEnergyPriority = 100;
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
        }
    }
}
