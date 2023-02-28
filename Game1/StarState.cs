using Game1.Lighting;

namespace Game1
{
    [Serializable]
    public sealed class StarState
    {
        // TODO: make prodWatts and color the consequence of Mass/Radius/material
        public readonly StarID starID;
        public readonly MyVector2 position;
        public UDouble Radius
            => MyMathHelper.Sqrt(value: consistsOfResPile.Amount.Area() / MyMathHelper.pi);
        public readonly LocationCounters locationCounters;
        public readonly SimpleHistoricProporSplitter<IRadiantEnergyConsumer> radiantEnergySplitter;
        public readonly ResPile consistsOfResPile;

        public StarState(MyVector2 position, BasicResInd consistsOfResInd, ulong mainResAmount, ResPile resSource)
        {
            starID = StarID.Create();
            this.position = position;
            radiantEnergySplitter = new();
            locationCounters = LocationCounters.CreateEmpty();
            consistsOfResPile = ResPile.CreateEmpty
            (
                thermalBody: ThermalBody.CreateEmpty(locationCounters: locationCounters)
            );
            consistsOfResPile.TransferFrom
            (
                source: resSource,
                amount: new
                (
                    resInd: consistsOfResInd,
                    amount: mainResAmount
                )
            );
            // Transfer appropriate amount of resources to here
            throw new NotImplementedException();
        }
    }
}
