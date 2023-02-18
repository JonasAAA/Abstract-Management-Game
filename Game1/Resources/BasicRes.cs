﻿namespace Game1.Resources
{
    [Serializable]
    public sealed class BasicRes : IResource
    {
        public BasicResInd resInd;
        public Mass Mass { get; }
        public HeatCapacity HeatCapacity { get; }
        public ulong Area { get; }
        public Propor Reflectance { get; }
        
        public readonly Color color;

        private readonly ResAmounts basicIngredients;

        public BasicRes(BasicResInd resInd, Mass mass, HeatCapacity heatCapacity, ulong area, Propor reflectance, Color color)
        {
            this.resInd = resInd;
            if (mass.IsZero)
                throw new ArgumentOutOfRangeException();
            Mass = mass;
            if (heatCapacity.IsZero)
                throw new ArgumentOutOfRangeException();
            HeatCapacity = heatCapacity;
            Reflectance = reflectance;
            if (area is 0)
                throw new ArgumentOutOfRangeException();
            Area = area;
            if (color.A != byte.MaxValue)
                throw new ArgumentException();
            this.color = color;

            basicIngredients = new()
            {
                [resInd] = 1
            };
        }

        ResInd IResource.ResInd
            => resInd;

        ResAmounts IResource.BasicIngredients
            => basicIngredients;
    }
}
