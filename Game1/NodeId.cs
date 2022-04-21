namespace Game1
{
    [Serializable]
    public class NodeId
    {
        public static NodeId Create()
            => new();

        // TODO: delete this extra logic
        private static ulong nextId = 1;

        private readonly ulong id;

        private NodeId()
        {
            id = nextId;
            nextId++;
        }
    }
}
