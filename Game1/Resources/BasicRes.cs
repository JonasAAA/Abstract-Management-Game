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
        
//        public readonly Color color;
//        private readonly ResAmounts basicIngredients;

//        public BasicRes(string name, BasicResInd resInd, Mass mass, HeatCapacity heatCapacity, ulong area, Propor reflectance, Propor emissivity, Color color)
//        {
//            Name = name;
//            this.resInd = resInd;
//            if (mass.IsZero)
//                throw new ArgumentOutOfRangeException();
//            Mass = mass;
//            if (heatCapacity.IsZero)
//                throw new ArgumentOutOfRangeException();
//            HeatCapacity = heatCapacity;
//            Reflectance = reflectance;
//            Emissivity = emissivity;
//            if (area is 0)
//                throw new ArgumentOutOfRangeException();
//            Area = area;
//            if (color.A != byte.MaxValue)
//                throw new ArgumentException();
//            this.color = color;

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
