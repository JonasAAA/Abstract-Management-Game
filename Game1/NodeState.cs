using Microsoft.Xna.Framework;

namespace Game1
{
    public class NodeState
    {
        public readonly Vector2 position;
        public UIntArray stored, arrived;

        public NodeState(Vector2 position)
        {
            this.position = position;
            stored = new();
            arrived = new();
        }
    }
}
