namespace Game1
{
    [Serializable]
    public class StarId
    {
        public static StarId Create()
            => new();

        private StarId()
        { }
    }
}
