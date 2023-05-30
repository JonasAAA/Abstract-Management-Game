using Game1.Collections;

namespace Game1.ContentHelpers
{
    /// <summary>
    /// Contains all needed data and the data is valid
    /// </summary>
    [Serializable]
    public readonly struct FullValidMapInfo
    {
        public static Result<FullValidMapInfo, TextErrors> Create(ValidMapInfo mapInfo)
            // The commented syntax will never work for what I want as it can't accumulate errors.
            // It calls SelectMany functions assuming that subsequent from clauses depend on the results of the previous one.
            // If indeed subsequent calls depend on previous ones, then can't accumulate errors as is some from statement fails, all
            // the subsequent ones become invalid.
            //=> from cosmicBodies in mapInfo.CosmicBodies.SelectMany(FullValidCosmicBodyInfo.Create).Select(func: Enumerable.ToArray)
            //   from links in mapInfo.Links.SelectMany(FullValidLinkInfo.Create).Select(func: Enumerable.ToArray)
            //   from startingInfo in FullValidStartingInfo.Create(startingInfo: mapInfo.StartingInfo)
            //   select new FullValidMapInfo(cosmicBodies, links, startingInfo);
            => Result.Lift
            (
                func: (arg1, arg2, arg3) => new FullValidMapInfo(cosmicBodies: arg1.ToArray(), links: arg2.ToArray(), startingInfo: arg3),
                arg1: mapInfo.CosmicBodies.SelectMany(FullValidCosmicBodyInfo.Create),
                arg2: mapInfo.Links.SelectMany(FullValidLinkInfo.Create),
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
