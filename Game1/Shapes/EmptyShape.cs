using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Game1.Shapes
{
    [DataContract]
    public class EmptyShape : NearRectangle
    {
        public EmptyShape()
            : base(width: 0, height: 0)
        { }

        public override bool Contains(Vector2 position)
            => false;

        protected override void Draw(Color color)
        { }
    }
}
