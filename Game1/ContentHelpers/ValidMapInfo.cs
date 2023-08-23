namespace Game1.ContentHelpers
{
    /// <summary>
    /// All the contained data is valid, but not all data may be present
    /// </summary>
    [Serializable]
    public readonly struct ValidMapInfo
    {
        public static ValidMapInfo CreateOrThrow(MapInfo mapInfo)
            => CreateOrThrow
            (
                notReadyToUse: mapInfo.NotReadyToUse,
                cosmicBodies: mapInfo.CosmicBodies.Select(ValidCosmicBodyInfo.CreateOrThrow).ToArray(),
                links: mapInfo.Links.Select(ValidLinkInfo.CreateOrThrow).ToArray(),
                startingInfo: ValidStartingInfo.CreateOrThrow(startingInfo: mapInfo.StartingInfo)
            );

        public static ValidMapInfo CreateOrThrow(bool notReadyToUse, ValidCosmicBodyInfo[] cosmicBodies, ValidLinkInfo[] links, ValidStartingInfo startingInfo)
        {
            HashSet<string> cosmicBodyNames = new();
            foreach (var cosmicBodyInfo in cosmicBodies)
                if (!cosmicBodyNames.Add(cosmicBodyInfo.Name))
                    throw new ContentException($"""Cosmic body names must be unique. "{cosmicBodyInfo.Name}" is used multiple times.""");
            foreach (var linkInfo in links)
            {
                if (!cosmicBodyNames.Contains(linkInfo.From))
                    throw new ContentException($"""Link {nameof(linkInfo.From)} must specify already existing cosmic body name. That's not the case for "{linkInfo.From}".""");
                if (!cosmicBodyNames.Contains(linkInfo.To))
                    throw new ContentException($"""Link {nameof(linkInfo.To)} must specify already existing cosmic body name. That's not the case for "{linkInfo.To}".""");
            }
            CheckIfCosmicBodyNameIsValid
            (
                cosmicBodyName: startingInfo.PowerPlantCosmicBody,
                cosmicBodyPurpose: nameof(ContentHelpers.StartingInfo.PowerPlantCosmicBody)
            );
            CheckIfCosmicBodyNameIsValid
            (
                cosmicBodyName: startingInfo.GearStorageCosmicBody,
                cosmicBodyPurpose: nameof(ContentHelpers.StartingInfo.GearStorageCosmicBody)
            );
            CheckIfCosmicBodyNameIsValid
            (
                cosmicBodyName: startingInfo.WireStorageCosmicBody,
                cosmicBodyPurpose: nameof(ContentHelpers.StartingInfo.WireStorageCosmicBody)
            );
            CheckIfCosmicBodyNameIsValid
            (
                cosmicBodyName: startingInfo.RoofTileStorageCosmicBody,
                cosmicBodyPurpose: nameof(ContentHelpers.StartingInfo.RoofTileStorageCosmicBody)
            );
            return new
            (
                notReadyToUse: notReadyToUse,
                cosmicBodies: cosmicBodies,
                links: links,
                startingInfo: startingInfo
            );

            void CheckIfCosmicBodyNameIsValid(string? cosmicBodyName, string cosmicBodyPurpose)
            {
                if (cosmicBodyName is not null && !cosmicBodyNames.Contains(cosmicBodyName))
                    throw new ContentException($"Starting {cosmicBodyPurpose} must specify already existing cosmic body name. That's not the case for \"{cosmicBodyName}\".");
            }
        }

        public bool NotReadyToUse { get; }
        public ValidCosmicBodyInfo[] CosmicBodies { get; }
        public ValidLinkInfo[] Links { get; }
        public ValidStartingInfo StartingInfo { get; }

        private ValidMapInfo(bool notReadyToUse, ValidCosmicBodyInfo[] cosmicBodies, ValidLinkInfo[] links, ValidStartingInfo startingInfo)
        {
            NotReadyToUse = notReadyToUse;
            CosmicBodies = cosmicBodies;
            Links = links;
            StartingInfo = startingInfo;
        }

        public MapInfo ToJsonable(bool readyToUse)
            => new()
            {
                NotReadyToUse = !readyToUse,
                CosmicBodies = CosmicBodies.Select(cosmicBody => cosmicBody.ToJsonable()).ToArray(),
                Links = Links.Select(link => link.ToJsonable()).ToArray(),
                StartingInfo = StartingInfo.ToJsonable()
            };
    }
}
