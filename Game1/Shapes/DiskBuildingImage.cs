using Game1.Industries;
using Game1.Lighting;
using Game1.UI;

namespace Game1.Shapes
{
    // This class is coded in a somewhat ugly way so that IncompleteBuildingImage doesn't allocate new object each time
    public sealed class DiskBuildingImage : IBuildingImage, IIncompleteBuildingImage
    {
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
            => DiskAlgos.Area(radius: CurRadius) - nodeShapeParams.Area.ToDouble();

        private UDouble FinishedBuildingRadius
            => nodeShapeParams.Radius + parameters.finishedBuildingHeight;
        private UDouble CurRadius
            => nodeShapeParams.Radius + buildingHeight;

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
