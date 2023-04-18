namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct ValidLinkInfo
    {
        public static ValidLinkInfo CreateOrThrow(LinkInfo linkInfo)
            =>  CreateOrThrow(from: linkInfo.From, to: linkInfo.To);

        public static ValidLinkInfo CreateOrThrow(string from, string to)
        {
            if (from == to)
                throw new ContentException($"""The two link endpoints can't be the same. Link from "{from}" to "{to}" violates this.""");
            return new(from: from, to: to);
        }

        public string From { get; }
        public string To { get; }

        private ValidLinkInfo(string from, string to)
        {
            From = from;
            To = to;
        }

        public LinkInfo ToJsonable()
            => new()
            {
                From = From,
                To = To
            };
    }
}
