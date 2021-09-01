namespace Game1
{
    public interface IEmployer
    {
        public IndustryType IndustryType { get; }
        /// <returns>between 0 and 1 or double.NegativeInfinity</returns>
        public double Desperation();
        public void Hire(Person person);
        public IJob CreateJob();
    }
}
