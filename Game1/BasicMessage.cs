using Game1.Delegates;
using Game1.UI;

namespace Game1
{
    [Serializable]
    public record BasicMessage : IMessage
    {
        private readonly NodeID nodeID;
        private readonly IFunction<IHUDElement> message;

        public BasicMessage(NodeID nodeID, IFunction<IHUDElement> message)
        {
            this.nodeID = nodeID;
            this.message = message;
        }

        public bool IsIdenticalTo(IMessage other)
            => other is BasicMessage otherBasicMessage && this == otherBasicMessage;
    }
}
