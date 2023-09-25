using Game1.Shapes;

namespace Game1.Lighting
{
    [Serializable]
    public sealed class LightBlockingDisk : Disk, ILightBlockingObject
    {
        public LightBlockingDisk(IParams parameters)
            : base(parameters: parameters)
        { }

        AngleArc.Params ILightBlockingObject.BlockedAngleArcParams(MyVector2 lightPos)
            => DiskAlgos.BlockedAngleArcParams(center: Center, radius: Radius, lightPos: lightPos);
    }
}
