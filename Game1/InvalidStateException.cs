using System.Runtime.Serialization;

namespace Game1
{
    /// <summary>
    /// This means that invariant in the state doesn't hold because of bad implementation
    /// </summary>
    [Serializable]
    public sealed class InvalidStateException : Exception
    {
        public InvalidStateException()
            : base()
        { }

        public InvalidStateException(string message)
            : base(message: message)
        { }

        public InvalidStateException(string message, Exception innerException)
            : base(message, innerException)
        { }

        // Needed since Exception implements ISerializable
        private InvalidStateException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        { }
    }
}
