using Game1.Lighting;
using Game1.Shapes;

namespace Game1.Industries
{
    public interface IBuildingImage : ILightBlockingObject
    {
        public AreaDouble Area { get; }

        public MyVector2 GetPosition(PosEnums origin);

        public bool Contains(MyVector2 position);

        public void Draw(Color otherColor, Propor otherColorPropor);
    }
}
