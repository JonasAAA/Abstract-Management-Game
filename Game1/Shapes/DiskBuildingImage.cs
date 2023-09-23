using Game1.Industries;
using Game1.Lighting;
using Game1.UI;

namespace Game1.Shapes
{
    // This class is coded in a somewhat ugly way so that IncompleteBuildingImage doesn't allocate new object each time
    [Serializable]
    public sealed class DiskBuildingImage : IBuildingImage, IIncompleteBuildingImage
    {
        [Serializable]
        public readonly struct Params : IBuildingImageParams<DiskBuildingImage>
        {
            public readonly UDouble finishedBuildingHeight;
            public readonly Color color;
            
            public Params(UDouble finishedBuildingHeight, Color color)
            {
                this.color = color;
                this.finishedBuildingHeight = finishedBuildingHeight;
            }

            public DiskBuildingImage CreateImage(INodeShapeParams nodeShapeParams)
                => new(parameters: this, nodeShapeParams: nodeShapeParams);
        }

        // Note that this is building area, not disk area
        public AreaDouble Area
            => AreaImpl(planetArea: nodeShapeParams.Area);

        public AreaDouble HypotheticalArea(AreaInt hypotheticPlanetArea)
            => AreaImpl(planetArea: hypotheticPlanetArea);

        private AreaDouble AreaImpl(AreaInt planetArea)
            => DiskAlgos.Area(radius: RadiusImpl(planetArea: planetArea)) - planetArea.ToDouble();

        private UDouble FinishedBuildingRadius
            => nodeShapeParams.Radius + parameters.finishedBuildingHeight;
        private UDouble CurRadius
            => RadiusImpl(planetArea: nodeShapeParams.Area);
        private UDouble RadiusImpl(AreaInt planetArea)
        {
            // The algorithm to calculate hypothetical building area in case planet size changes relies on planet radius to be
            // exactly what you would get from the planet area assuming that the planet is disk.
            Debug.Assert(DiskAlgos.RadiusFromArea(area: nodeShapeParams.Area.ToDouble()).IsCloseTo(nodeShapeParams.Radius));
            return DiskAlgos.RadiusFromArea(area: planetArea.ToDouble()) + buildingHeight;
        }

        private readonly Params parameters;
        private readonly INodeShapeParams nodeShapeParams;
        private UDouble buildingHeight;
        private DiskBuildingImage? incompleteBuildingImage;

        private DiskBuildingImage(Params parameters, INodeShapeParams nodeShapeParams)
        {
            this.parameters = parameters;
            this.nodeShapeParams = nodeShapeParams;
            buildingHeight = parameters.finishedBuildingHeight;
            incompleteBuildingImage = null;
        }

        public AngleArc.Params BlockedAngleArcParams(MyVector2 lightPos)
            => DiskAlgos.BlockedAngleArcParams(center: nodeShapeParams.Position, radius: CurRadius, lightPos: lightPos);

        public MyVector2 GetPosition(PosEnums origin)
            => origin.GetPosInRect(center: nodeShapeParams.Position, width: 2 * FinishedBuildingRadius, height: 2 * FinishedBuildingRadius);

        public bool Contains(MyVector2 position)
            => DiskAlgos.Contains(center: nodeShapeParams.Position, radius: CurRadius, otherPos: position);

        public void Draw(Color otherColor, Propor otherColorPropor)
        {
            // The outline of the finished building
            DiskAlgos.Draw
            (
                center: nodeShapeParams.Position,
                radius: FinishedBuildingRadius,
                color: UIAlgorithms.MixColorsAndMakeTransparent
                (
                    transparency: (Propor).25,
                    baseColor: parameters.color,
                    otherColor: otherColor,
                    otherColorPropor: otherColorPropor
                )
            );

            // Building itself
            DiskAlgos.Draw
            (
                center: nodeShapeParams.Position,
                radius: CurRadius,
                color: parameters.color
            );
        }

        public IBuildingImage IncompleteBuildingImage(Propor donePropor)
        {
            incompleteBuildingImage ??= parameters.CreateImage(nodeShapeParams: nodeShapeParams);
            incompleteBuildingImage.buildingHeight = (UDouble)(DiskAlgos.RadiusFromArea(area: nodeShapeParams.Area.ToDouble() + donePropor * Area) - nodeShapeParams.Radius);
            return incompleteBuildingImage;
        }
    }
}
