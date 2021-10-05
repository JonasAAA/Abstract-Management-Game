using Game1.Industries;

namespace Game1
{
    public interface IJob : IActivityCenter
    {
        public IndustryType IndustryType { get; }
    }
}
