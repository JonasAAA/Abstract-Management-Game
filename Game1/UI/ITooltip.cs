using Game1.Shapes;

namespace Game1.UI
{
    public interface ITooltip
    {
        public NearRectangle Shape { get; }

        public void Update();

        public void Draw();
    }
}
