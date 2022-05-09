namespace Game1
{
    [Serializable]
    public class StarID
    {
        public static StarID Create()
            => new();

        private StarID()
        { }
    }
}
