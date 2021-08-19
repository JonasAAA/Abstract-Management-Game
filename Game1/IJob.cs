using System;

namespace Game1
{
    public interface IJob
    {
        public IndustryType IndustryType { get; }
        /// <returns>between 0 and 1 or double.NegativeInfinity</returns>
        //public double OpenSpace();
        /// <returns>between 0 and 1 or double.NegativeInfinity</returns>
        public double Desperation();
        public void Hire(Person person);
    }
}
