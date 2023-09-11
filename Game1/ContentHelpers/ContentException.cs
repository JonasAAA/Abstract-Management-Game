using System.Runtime.Serialization;

namespace Game1.ContentHelpers
{
    [Serializable]
    public sealed class ContentException : Exception
    {
        public ContentException()
            : base()
        { }

        public ContentException(string message)
            : base(message: message)
        { }

        public ContentException(string message, Exception innerException)
            : base(message, innerException)
        { }

        // Needed since Exception implements ISerializable
        private ContentException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        { }
    }
}
