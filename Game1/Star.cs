using Game1.Lighting;
using Game1.PrimitiveTypeWrappers;
using Game1.Shapes;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class Star : WorldUIElement, ILightSource
    {
        private readonly UDouble prodWatts;
        private readonly LightPolygon polygon;

        private readonly TextBox popupTextBox;

        public Star(UDouble radius, MyVector2 center, UDouble prodWatts, Color color)
            : base(shape: new Ellipse(width: 2 * radius, height: 2 * radius), activeColor: Color.AntiqueWhite, inactiveColor: Color.White, popupHorizPos: HorizPos.Right, popupVertPos: VertPos.Top)
        {
            shape.Center = center;
            this.prodWatts = prodWatts;
            polygon = new LightPolygon(strength: radius / CurWorldConfig.standardStarRadius, color: color);

            popupTextBox = new();
            popupTextBox.Shape.Color = Color.White;
            SetPopup
            (
                HUDElement: popupTextBox,
                overlays: IOverlay.all
            );

            CurWorldManager.AddLightSource(lightSource: this);
        }

        // let N = lightCatchingObject.Count()
        // the complexity is O(N ^ 2) as each object has O(1) relevant angles
        // and each object checks intersection with all the rays
        // could maybe get the time down to O(N log N) by using modified interval tree
        void ILightSource.GiveWattsToObjects(IEnumerable<ILightCatchingObject> lightCatchingObjects)
        {
            List<double> angles = new();
            foreach (var lightCatchingObject in lightCatchingObjects)
                angles.AddRange(lightCatchingObject.RelAngles(lightPos: shape.Center));

            const double small = .0001;
            int oldAngleCount = angles.Count;

            for (int i = 0; i < oldAngleCount; i++)
            {
                angles.Add(angles[i] + small);
                angles.Add(angles[i] - small);
            }

            // prepare angles
            for (int i = 0; i < 4; i++)
                angles.Add(i * 2 * (double)MyMathHelper.pi / 4);

            angles = new SortedSet<double>
            (
                from angle in angles
                select MyMathHelper.WrapAngle(angle)
            ).ToList();

            List<MyVector2> vertices = new();
            List<ILightCatchingObject> rayCatchingObjects = new();
            // TODO: consider moving this to constants class
            UDouble maxDist = 2000;
            foreach (double angle in angles)
            {
                MyVector2 rayDir = MyMathHelper.Direction(rotation: angle);
                UDouble minDist = maxDist;
                ILightCatchingObject rayCatchingObject = null;
                foreach (var lightCatchingObject in lightCatchingObjects)
                {
                    var dists = lightCatchingObject.InterPoints(lightPos: shape.Center, lightDir: rayDir);
                    foreach (var dist in dists)
                        if (UDouble.Create(value: dist) is UDouble nonnegDist && nonnegDist < minDist)
                        {
                            minDist = nonnegDist;
                            rayCatchingObject = lightCatchingObject;
                        }
                }
                rayCatchingObjects.Add(rayCatchingObject);
                vertices.Add(shape.Center + (double)minDist * rayDir);
            }

            Debug.Assert(rayCatchingObjects.Count == angles.Count && vertices.Count == angles.Count);

            polygon.Update(center: shape.Center, vertices: vertices);

            Dictionary<ILightCatchingObject, UDouble> arcsForObjects = lightCatchingObjects.ToDictionary
            (
                keySelector: lightCatchingObject => lightCatchingObject,
                elementSelector: lightCatchingObject => (UDouble)0
            );
            UDouble usedArc = 0;
            for (int i = 0; i < rayCatchingObjects.Count; i++)
                if (rayCatchingObjects[i] is not null && rayCatchingObjects[i] == rayCatchingObjects[(i + 1) % rayCatchingObjects.Count])
                {
                    // TODO: delete comments
                    UDouble curArc = MyMathHelper.Abs(MyMathHelper.WrapAngle(angles[i] - angles[(i + 1) % angles.Count]));// / (2 * MyMathHelper.pi);
                    arcsForObjects[rayCatchingObjects[i]] += curArc;
                    usedArc += curArc;
                }

            popupTextBox.Text = $"generates {prodWatts} power\n{usedArc / (2 * MyMathHelper.pi) * 100:0.}% of it is used";

            foreach (var lightCatchingObject in lightCatchingObjects)
            {
                Propor powerPropor = Propor.Create(part: arcsForObjects[lightCatchingObject], whole: 2 * MyMathHelper.pi).Value;
                lightCatchingObject.SetWatts
                (
                    starPos: shape.Center,
                    watts: powerPropor * prodWatts,
                    powerPropor: powerPropor
                );
            }
        }

        void ILightSource.Draw(GraphicsDevice graphicsDevice, Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight)
            => polygon.Draw
            (
                graphicsDevice: graphicsDevice,
                worldToScreenTransform: worldToScreenTransform,
                basicEffect: basicEffect,
                actualScreenWidth: actualScreenWidth,
                actualScreenHeight: actualScreenHeight
            );
    }
}
