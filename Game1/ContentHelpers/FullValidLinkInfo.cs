namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidLinkInfo
    {
        public static Result<FullValidLinkInfo, IEnumerable<string>> Create(ValidLinkInfo linkInfo)
            => new(ok: new(from: linkInfo.From, to: linkInfo.To));

        public string From { get; }
        public string To { get; }

        private FullValidLinkInfo(string from, string to)
        {
            From = from;
            To = to;
        }
    }
}
