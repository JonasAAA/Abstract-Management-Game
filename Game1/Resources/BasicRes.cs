//namespace Game1.Resources
//{
//    [Serializable]
//    public sealed class BasicRes : IResource
//    {
//        public string Name { get; }
//        public BasicResInd resInd;
//        public Mass Mass { get; }
//        public HeatCapacity HeatCapacity { get; }
//        public ulong Area { get; }
//        public Propor Reflectance { get; }
//        public Propor Emissivity { get; }
        
//        public readonly Color Color;
//        private readonly ResAmounts basicIngredients;

//        public BasicRes(string Name, BasicResInd resInd, Mass splittingMass, HeatCapacity heatCapacity, ulong area, Propor reflectance, Propor emissivity, Color Color)
//        {
//            Name = Name;
//            this.resInd = resInd;
//            if (splittingMass.IsZero)
//                throw new ArgumentOutOfRangeException();
//            Mass = splittingMass;
//            if (heatCapacity.IsZero)
//                throw new ArgumentOutOfRangeException();
//            HeatCapacity = heatCapacity;
//            Reflectance = reflectance;
//            Emissivity = emissivity;
//            if (area is 0)
//                throw new ArgumentOutOfRangeException();
//            Area = area;
//            if (Color.A != byte.MaxValue)
//                throw new ArgumentException();
//            this.Color = Color;

//            basicIngredients = new()
//            {
//                [resInd] = 1
//            };
//        }

//        ResInd IResource.ResInd
//            => resInd;

//        ResAmounts IResource.BasicIngredients
//            => basicIngredients;
//    }
//}
