namespace Game1
{
    public class NodeState
    {
        public UIntArray stored, arrived;

        public NodeState()
        {
            stored = new();
            arrived = new();
        }
    }
}
