namespace Game1
{
    public interface IJob
    {
        public IndustryType IndustryType { get; }
        public bool IsFull();
        public void Hire(Person person);
    }
}
