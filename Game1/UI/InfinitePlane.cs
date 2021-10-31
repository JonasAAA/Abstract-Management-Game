using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class InfinitePlane : Shape
    {
        public InfinitePlane()
            => Color = Color.Transparent;

        public override bool Contains(Vector2 position)
            => true;

        protected override void Draw(Color color)
        { }
    }
}
