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
                func: (arg1, arg2, arg3) =>
                {
                    var cosmicBodies = arg1.ToEfficientReadOnlyCollection();
                    var links = arg2.ToEfficientReadOnlyCollection();
                    return FindDisconnectedCosmicBodies(cosmicBodies: cosmicBodies, links: links) switch
                    {
                        var (cosmicBodyA, cosmicBodyB) => new(errors: new($"It must be possible to travel between any two cosmic bodies using links.\nThis is not the case for cosmic bodies named {cosmicBodyA} and {cosmicBodyB}")),
                        null => new Result<FullValidMapInfo, TextErrors>(ok: new FullValidMapInfo(cosmicBodies: cosmicBodies, links: links, startingInfo: arg3))
                    };
                },
                arg1: mapInfo.CosmicBodies.SelectMany(FullValidCosmicBodyInfo.Create),
                arg2: mapInfo.Links.SelectMany(FullValidLinkInfo.Create),
                arg3: FullValidStartingInfo.Create(startingInfo: mapInfo.StartingInfo)
            ).SelectMany(fullValidMapInfo => fullValidMapInfo);

        private static (string cosmicBodyA, string cosmicBodyB)? FindDisconnectedCosmicBodies(EfficientReadOnlyCollection<FullValidCosmicBodyInfo> cosmicBodies, EfficientReadOnlyCollection<FullValidLinkInfo> links)
        {
            // Disjoint set union algorithm
            var cosmicBodyToRepr = cosmicBodies.ToDictionary
            (
                keySelector: cosmicBody => cosmicBody.Name,
                elementSelector: cosmicBody => cosmicBody.Name
            );
            var setReprToSetCount = cosmicBodies.ToDictionary
            (
                keySelector: cosmicBody => cosmicBody.Name,
                elementSelector: cosmicBody => 1ul
            );
            foreach (var link in links)
            {
                var reprA = FindRepresentative(cosmicBody: link.From);
                var reprB = FindRepresentative(cosmicBody: link.To);
                if (reprA == reprB)
                    continue;
                if (setReprToSetCount[reprA] < setReprToSetCount[reprB])
                    (reprA, reprB) = (reprB, reprA);
                // it would be enough to simply state that reprB is represented by reprA, but this way it's a tiny bit more efficient
                SetNewRepresentative(cosmicBody: link.From, newRepr: reprA);
                SetNewRepresentative(cosmicBody: link.To, newRepr: reprA);
                cosmicBodyToRepr[reprB] = reprA;
                setReprToSetCount[reprA] += setReprToSetCount[reprB];
                // This removing in not necessary, strictly speaking
                setReprToSetCount.Remove(reprB);
            }

            string? overallRepr = null;
            foreach (var cosmicBody in cosmicBodies)
            {
                var repr = FindRepresentative(cosmicBody: cosmicBody.Name);
                SetNewRepresentative(cosmicBody: cosmicBody.Name, newRepr: repr);
                overallRepr ??= repr;
                if (overallRepr != repr)
                    return (cosmicBodyA: overallRepr, cosmicBodyB: repr);
            }

            return null;

            string FindRepresentative(string cosmicBody)
            {
                while (true)
                {
                    var repr = cosmicBodyToRepr[cosmicBody];
                    if (repr == cosmicBody)
                        return cosmicBody;
                    cosmicBody = repr;
                }
            }

            string SetNewRepresentative(string cosmicBody, string newRepr)
            {
                while (true)
                {
                    cosmicBodyToRepr[cosmicBody] = newRepr;
                    var repr = cosmicBodyToRepr[cosmicBody];
                    if (repr == cosmicBody)
                        return cosmicBody;
                    cosmicBody = repr;
                }
            }
        }

        public EfficientReadOnlyCollection<FullValidCosmicBodyInfo> CosmicBodies { get; }
        public EfficientReadOnlyCollection<FullValidLinkInfo> Links { get; }
        public FullValidStartingInfo StartingInfo { get; }

        private FullValidMapInfo(EfficientReadOnlyCollection<FullValidCosmicBodyInfo> cosmicBodies, EfficientReadOnlyCollection<FullValidLinkInfo> links, FullValidStartingInfo startingInfo)
        {
            CosmicBodies = cosmicBodies;
            Links = links;
            StartingInfo = startingInfo;
        }
    }
}
