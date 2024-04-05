// This namespace contains stuff that both MapCreationState and PlayState can use
using Game1.Collections;

namespace Game1.GlobalTypes
{
    [Serializable]
    public enum RawMaterialID
    {
        Firstium,
        Secondium,
        Thirdium,
        Fourthium,
        //Fifthium,
        //Sixthium
    }

    public static class RawMaterialIDUtil
    {
        public static readonly RawMaterialID lastRawMatID = Enum.GetValues<RawMaterialID>().Last();
        private static readonly EnumDict<RawMaterialID, RawMaterialID?> rawMatIDToNext = new
        (
            selector: rawMatID =>
            {
                var nextRawMatID = (RawMaterialID)(rawMatID.Ind() + 1);
                if (rawMatID == lastRawMatID)
                {
                    Debug.Assert(!Enum.IsDefined(nextRawMatID));
                    return null;
                }
                Debug.Assert(Enum.IsDefined(nextRawMatID));
                return nextRawMatID;
            }
        );
        private static readonly EfficientReadOnlyDictionary<int, RawMaterialID> representativeNumToRawMatID = Enum.GetValues<RawMaterialID>().ToEfficientReadOnlyDict
        (
            keySelector: rawMatID => (int)rawMatID.RepresentativeNumber()
        );

        /// <summary>
        /// Returns null for the last raw material
        /// </summary>
        public static RawMaterialID? Next(this RawMaterialID rawMatID)
            => rawMatIDToNext[rawMatID];

        public static ulong Ind(this RawMaterialID rawMatID)
            => (ulong)rawMatID;

        public static string Name(this RawMaterialID rawMatID)
            => rawMatID switch
            {
                RawMaterialID.Firstium => "firstium",
                RawMaterialID.Secondium => "secondium",
                RawMaterialID.Thirdium => "thirdium",
                RawMaterialID.Fourthium => "fourthium",
                //RawMaterialID.Fifthium => "fifthium",
                //RawMaterialID.Sixthium => "sixthium"
            };

        /// <summary>
        /// This is to be used in places like file names, map dumps, etc. so that the player could
        /// easily associate the number with the raw material (e.g. 2 with secondium)
        /// </summary>
        public static ulong RepresentativeNumber(this RawMaterialID rawMatID)
            => rawMatID.Ind() + 1;

        public static RawMaterialID? FromRepresentativeNumber(int representativeNum)
            => representativeNumToRawMatID.TryGetValue(key: representativeNum, value: out var value) ? value : null;

        public static Propor Normalized(this RawMaterialID rawMatID)
            => Algorithms.Normalize(value: rawMatID.Ind(), start: 0, stop: lastRawMatID.Ind());
    }
}
