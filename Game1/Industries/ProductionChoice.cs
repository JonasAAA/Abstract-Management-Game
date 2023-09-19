namespace Game1.Industries
{
    [Serializable]
    public readonly record struct ProductionChoice(object Choice)
    {
        public override string? ToString()
            => Choice.ToString();
    }
}
