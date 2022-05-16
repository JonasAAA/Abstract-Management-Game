namespace Game1
{
    [Serializable]
    public sealed class StarID
    {
        public static StarID Create()
            => new();

        private StarID()
        { }
    }
}
