using Microsoft.Xna.Framework;
using System;

namespace Game1
{
    public class Position
    {
        public readonly float x, y;
        
        public Position(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Position(Vector2 position)
            : this(x: position.X, y: position.Y)
        { }

        public Vector2 ToVector2()
            => new(x, y);

        public double DistanceTo(Position position)
        {
            float diffX = x - position.x,
                diffY = y - position.y;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }
    }
}
