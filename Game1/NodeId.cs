namespace Game1
{
    [Serializable]
    public class NodeId
    {
        public static NodeId Create()
            => new();

        private NodeId()
        { }
    }
}
