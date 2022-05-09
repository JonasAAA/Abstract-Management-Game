namespace Game1
{
    [Serializable]
    public class NodeID
    {
        public static NodeID Create()
            => new();

        private NodeID()
        { }
    }
}
