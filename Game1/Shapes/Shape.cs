using Game1.UI;

namespace Game1.Shapes
{
    [Serializable]
    public abstract class Shape
    {
        public abstract bool Contains(MyVector2 position);

        public abstract void Draw(Color color);

        public void Draw(Color baseColor, Color otherColor, Propor otherColorPropor)
        {
            Color color = UIAlgorithms.MixColors(baseColor: baseColor, otherColor: otherColor, otherColorPropor: otherColorPropor);
            if (!color.Transparent())
                Draw(color: color);
        }
    }
}
