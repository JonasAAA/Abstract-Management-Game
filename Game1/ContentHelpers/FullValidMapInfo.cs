namespace Game1.ContentHelpers
{
    /// <summary>
    /// Contains all needed data and the data is valid
    /// </summary>
    [Serializable]
    public readonly struct FullValidMapInfo
    {
        public static GeneralEnum<FullValidMapInfo, IEnumerable<string>> Create(ValidMapInfo mapInfo)
            => GeneralEnum.CallFunc
            (
                func: (arg1, arg2, arg3) => new FullValidMapInfo(arg1, arg2, arg3),
                arg1: GeneralEnum.CallFunc
                (
                    func: Enumerable.ToArray,
                    arg: mapInfo.CosmicBodies.Select(FullValidCosmicBodyInfo.Create).Collect()
                ),
                arg2: GeneralEnum.CallFunc
                (
                    func: Enumerable.ToArray,
                    arg: mapInfo.Links.Select(FullValidLinkInfo.Create).Collect()
                ),
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
