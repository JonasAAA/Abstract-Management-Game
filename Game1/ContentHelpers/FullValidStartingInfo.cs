using Game1.Collections;
using System.Collections.Immutable;

namespace Game1.ContentHelpers
{
    [Serializable]
    public readonly struct FullValidStartingInfo
    {
        public static Result<FullValidStartingInfo, TextErrors> Create(ValidStartingInfo startingInfo)
        {
            return Result.Lift<string, string, FullValidStartingInfo, string>
            (
                func: (arg1, arg2) => new FullValidStartingInfo
                (
                    houseCosmicBody: arg1,
                    powerPlantCosmicBody: arg2,
                    worldCenter: startingInfo.WorldCenter,
                    cameraViewHeight: startingInfo.CameraViewHeight
                ),
                arg1: startingInfo.HouseCosmicBody switch
                {
                    string houseCosmicBody => new(ok: houseCosmicBody),
                    null => new(errors: new(value: $"Starting {nameof(StartingInfo.HouseCosmicBody)} must be set"))
                },
                arg2: startingInfo.PowerPlantCosmicBody switch
                {
                    string powerPlantCosmicBody => new(ok: powerPlantCosmicBody),
                    null => new(errors: new(value: $"Starting {nameof(StartingInfo.PowerPlantCosmicBody)} must be set"))
                }
            );
        }

        public string HouseCosmicBody { get; }
        public string PowerPlantCosmicBody { get; }
        public MyVector2 WorldCenter { get; }
        public UDouble CameraViewHeight { get; }

        private FullValidStartingInfo(string houseCosmicBody, string powerPlantCosmicBody, MyVector2 worldCenter, UDouble cameraViewHeight)
        {
            HouseCosmicBody = houseCosmicBody;
            PowerPlantCosmicBody = powerPlantCosmicBody;
            WorldCenter = worldCenter;
            CameraViewHeight = cameraViewHeight;
        }
    }
}
