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
        Fifthium,
        Sixthium
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

        /// <summary>
        /// Returns null for the last raw material
        /// </summary>
        public static RawMaterialID? Next(this RawMaterialID rawMatID)
            => rawMatIDToNext[rawMatID];

        public static ulong Ind(this RawMaterialID rawMatID)
            => (ulong)rawMatID;
    }
}
