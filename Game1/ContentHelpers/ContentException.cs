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
    }
}
