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
            : base(message)
        { }
    }
}
