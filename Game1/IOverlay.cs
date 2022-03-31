namespace Game1
{
    public interface IOverlay
    {
        [Serializable]
        private class AllResOverlay : IAllResOverlay
        {
            public override string ToString()
                => "AllRes";
        }

        [Serializable]
        private class PowerOverlay : IPowerOverlay
        {
            public override string ToString()
                => "Power";
        }

        [Serializable]
        private class PeopleOverlay : IPeopleOverlay
        {
            public override string ToString()
                => "People";
        }

        // TODO: rename to lower case
        public static readonly IAllResOverlay allRes;
        public static readonly IPowerOverlay power;
        public static readonly IPeopleOverlay people;

        public static readonly IReadOnlyCollection<IOverlay> all;

        static IOverlay()
        {
            allRes = new AllResOverlay();
            power =  new PowerOverlay();
            people = new PeopleOverlay();

            List<IOverlay> allTemp = new();
            
            foreach (var resInd in ResInd.All)
                allTemp.Add(resInd);

            allTemp.AddRange(new List<IOverlay>
            {
                allRes,
                power,
                people
            });

            all = allTemp;
        }
    }

    public interface IAllResOverlay : IOverlay
    { }

    public interface IPowerOverlay : IOverlay
    { }

    public interface IPeopleOverlay : IOverlay
    { }
}
