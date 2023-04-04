namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidLinkInfo
    {
        public static GeneralEnum<FullValidLinkInfo, IEnumerable<string>> Create(ValidLinkInfo linkInfo)
            => new(value1: new(from: linkInfo.From, to: linkInfo.To));

        public string From { get; }
        public string To { get; }

        private FullValidLinkInfo(string from, string to)
        {
            From = from;
            To = to;
        }
    }
}
