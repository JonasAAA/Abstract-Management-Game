namespace Game1
{
    [Serializable]
    public record BasicMessage : IMessage
    {
        private readonly NodeID nodeID;
        private readonly string message;

        public BasicMessage(NodeID nodeID, string message)
        {
            this.nodeID = nodeID;
            this.message = message;
        }

        public bool IsIdenticalTo(IMessage other)
            => other is BasicMessage otherBasicMessage && this == otherBasicMessage;
    }
}
