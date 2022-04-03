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

        public void SwitchStatement(Action<ResInd> singleResCase, Action allResCase, Action powerCase, Action peopleCase)
        {
            switch (this)
            {
                case ResInd resInd:
                    singleResCase(resInd);
                    break;
                case IAllResOverlay:
                    allResCase();
                    break;
                case IPowerOverlay:
                    powerCase();
                    break;
                case IPeopleOverlay:
                    peopleCase();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public T SwitchExpression<T>(Func<ResInd, T> singleResCase, Func<T> allResCase, Func<T> powerCase, Func<T> peopleCase)
            => this switch
            {
                ResInd resInd => singleResCase(resInd),
                IAllResOverlay => allResCase(),
                IPowerOverlay => powerCase(),
                IPeopleOverlay => peopleCase(),
                _ => throw new ArgumentOutOfRangeException()
            };
    }

    public interface IAllResOverlay : IOverlay
    { }

    public interface IPowerOverlay : IOverlay
    { }

    public interface IPeopleOverlay : IOverlay
    { }
}
