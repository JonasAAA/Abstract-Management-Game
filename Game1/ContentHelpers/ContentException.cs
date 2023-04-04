namespace Game1.ContentHelpers
{
    [Serializable]
    public class ContentException : Exception
    {
        public ContentException(string message)
            : base(message: message)
        { }
    }
}
