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
        public readonly struct Params(Length finishedBuildingHeight, Color color) : IBuildingImageParams<DiskBuildingImage>
        {
            public readonly Length finishedBuildingHeight = finishedBuildingHeight;
            public readonly Color color = color;

            public DiskBuildingImage CreateImage(INodeShapeParams nodeShapeParams)
                => new(parameters: this, nodeShapeParams: nodeShapeParams);
        }

        public static AreaDouble ComputeBuildingArea(AreaInt planetArea, Length buildingHeight)
            => DiskAlgos.Area(radius: ComputeRadius(planetArea: planetArea, buildingHeight: buildingHeight)) - planetArea.ToDouble();

        private static Length ComputeRadius(AreaInt planetArea, Length buildingHeight)
            => DiskAlgos.RadiusFromArea(area: planetArea.ToDouble()) + buildingHeight;

        // Note that this is building area, not disk area
        public AreaDouble Area
            => ComputeBuildingArea(planetArea: nodeShapeParams.Area, buildingHeight: buildingHeight);

        public AreaDouble HypotheticalArea(AreaInt hypotheticPlanetArea)
            => ComputeBuildingArea(planetArea: hypotheticPlanetArea, buildingHeight: buildingHeight);

        
        private Length FinishedBuildingRadius
            => nodeShapeParams.Radius + parameters.finishedBuildingHeight;
        private Length CurRadius
        {
            get
            {
                // The algorithm to calculate hypothetical building area in case planet size changes relies on planet radius to be
                // exactly what you would get from the planet area assuming that the planet is disk.
                Debug.Assert(DiskAlgos.RadiusFromArea(area: nodeShapeParams.Area.ToDouble()).IsCloseTo(nodeShapeParams.Radius));
                return ComputeRadius(planetArea: nodeShapeParams.Area, buildingHeight: buildingHeight);
            }
        }


        private readonly Params parameters;
        private readonly INodeShapeParams nodeShapeParams;
        private Length buildingHeight;
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
                    transparency: (Propor).5,
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
                color: UIAlgorithms.MixColors
                (
                    baseColor: parameters.color,
                    otherColor: otherColor,
                    otherColorPropor: otherColorPropor
                )
            );
        }

        public IBuildingImage IncompleteBuildingImage(Propor donePropor)
        {
            incompleteBuildingImage ??= parameters.CreateImage(nodeShapeParams: nodeShapeParams);
            incompleteBuildingImage.buildingHeight = (Length)((SignedLength)DiskAlgos.RadiusFromArea(area: nodeShapeParams.Area.ToDouble() + donePropor * Area) - nodeShapeParams.Radius);
            return incompleteBuildingImage;
        }
    }
}
