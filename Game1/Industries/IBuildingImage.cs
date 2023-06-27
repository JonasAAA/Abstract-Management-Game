using Game1.Lighting;

namespace Game1.Industries
{
    public interface IBuildingImage : ILightBlockingObject
    {
        public AreaDouble Area { get; }

        public bool Contains(MyVector2 position);

        public void Draw(Color otherColor, Propor otherColorPropor);
    }
}
