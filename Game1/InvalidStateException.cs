namespace Game1
{
    /// <summary>
    /// This means that invariant in the state doesn't hold because of bad implementation
    /// </summary>
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
    }
}
