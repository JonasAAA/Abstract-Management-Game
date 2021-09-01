namespace Game1
{
    public interface IJob
    {
        public IndustryType IndustryType { get; }
        public ulong ElectrPriority { get; }
    }
}
