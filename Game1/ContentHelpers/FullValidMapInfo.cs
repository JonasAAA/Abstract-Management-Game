namespace Game1.ContentHelpers
{
    /// <summary>
    /// Contains all needed data and the data is valid
    /// </summary>
    [Serializable]
    public readonly struct FullValidMapInfo
    {
        public static Result<FullValidMapInfo, IEnumerable<string>> Create(ValidMapInfo mapInfo)
            => Result.CallFunc
            (
                func: (arg1, arg2, arg3) => new FullValidMapInfo(arg1, arg2, arg3),
                arg1: mapInfo.CosmicBodies.FlatMap(FullValidCosmicBodyInfo.Create).Map(func: Enumerable.ToArray),
                arg2: mapInfo.Links.FlatMap(FullValidLinkInfo.Create).Map(func: Enumerable.ToArray),
                arg3: FullValidStartingInfo.Create(startingInfo: mapInfo.StartingInfo)
            );


        public FullValidCosmicBodyInfo[] CosmicBodies { get; }
        public FullValidLinkInfo[] Links { get; }
        public FullValidStartingInfo StartingInfo { get; }

        private FullValidMapInfo(FullValidCosmicBodyInfo[] cosmicBodies, FullValidLinkInfo[] links, FullValidStartingInfo startingInfo)
        {
            CosmicBodies = cosmicBodies;
            Links = links;
            StartingInfo = startingInfo;
        }
    }
}
