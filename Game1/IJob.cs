using System;

namespace Game1
{
    public interface IJob
    {
        public IndustryType IndustryType { get; }
        //public TimeSpan SearchStart { get; }
        public double OpenSpace();
        public void Hire(Person person);
    }
}
