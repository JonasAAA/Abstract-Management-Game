namespace Game1
{
    [Serializable]
    public sealed class NodeID
    {
        public static NodeID Create()
            => new();

        private NodeID()
        { }
    }
}
