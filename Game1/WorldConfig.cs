namespace Game1
{
    public class WorldConfig
    {
        public readonly double personJobEnjoymentCoeff, personTalentCoeff, personSkillCoeff, jobDesperationCoeff, PlayerToJobDistCoeff, minAcceptablePersonScore, personTimeSkillCoeff;
        public readonly ulong linkEnergyPriority;
        public readonly float standardStarRadius;

        public WorldConfig()
        {
            personJobEnjoymentCoeff = .2;
            personTalentCoeff = .1;
            personSkillCoeff = .2;
            PlayerToJobDistCoeff = .1;
            jobDesperationCoeff = .4;
            minAcceptablePersonScore = .4;
            personTimeSkillCoeff = .1;
            linkEnergyPriority = 10;
            standardStarRadius = 50;
        }
    }
}
