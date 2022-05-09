using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;

using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class Star : WorldUIElement, ILightSource
    {
        [Serializable]
        private readonly record struct ShapeParams(StarState State) : Disk.IParams
        {
            public MyVector2 Center
                => State.position;

            public UDouble Radius
                => State.radius;
        }

        private readonly StarState state;
        private readonly LightPolygon polygon;
        private readonly TextBox popupTextBox;

        public Star(StarState state, Color color)
            : base
            (
                shape: new Disk(parameters: new ShapeParams(State: state), color: Color.White),
                activeColor: Color.AntiqueWhite,
                inactiveColor: Color.White,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            this.state = state;
            polygon = new LightPolygon(strength: state.radius / CurWorldConfig.standardStarRadius, color: color);

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
                angles.AddRange(lightCatchingObject.RelAngles(lightPos: state.position));

            const double small = .0001;
            int oldAngleCount = angles.Count;

            for (int i = 0; i < oldAngleCount; i++)
            {
                angles.Add(angles[i] + small);
                angles.Add(angles[i] - small);
            }

            // prepare angles
            for (int i = 0; i < 4; i++)
                angles.Add(i * 2 * MyMathHelper.pi / 4);

            angles = new SortedSet<double>
            (
                from angle in angles
                select MyMathHelper.WrapAngle(angle)
            ).ToList();

            List<MyVector2> vertices = new();
            List<ILightCatchingObject?> rayCatchingObjects = new();
            // TODO: consider moving this to constants class
            UDouble maxDist = 2000;
            foreach (double angle in angles)
            {
                MyVector2 rayDir = MyMathHelper.Direction(rotation: angle);
                UDouble minDist = maxDist;
                ILightCatchingObject? rayCatchingObject = null;
                foreach (var lightCatchingObject in lightCatchingObjects)
                {
                    var dists = lightCatchingObject.InterPoints(lightPos: state.position, lightDir: rayDir);
                    foreach (var dist in dists)
                        if (UDouble.Create(value: dist) is UDouble nonnegDist && nonnegDist < minDist)
                        {
                            minDist = nonnegDist;
                            rayCatchingObject = lightCatchingObject;
                        }
                }
                rayCatchingObjects.Add(rayCatchingObject);
                vertices.Add(state.position + minDist * rayDir);
            }

            Debug.Assert(rayCatchingObjects.Count == angles.Count && vertices.Count == angles.Count);

            polygon.Update(center: state.position, vertices: vertices);

            Dictionary<ILightCatchingObject, UDouble> arcsForObjects = lightCatchingObjects.ToDictionary
            (
                keySelector: lightCatchingObject => lightCatchingObject,
                elementSelector: lightCatchingObject => (UDouble)0
            );
            UDouble usedArc = 0;
            for (int i = 0; i < rayCatchingObjects.Count; i++)
            {
                UDouble curArc = MyMathHelper.Abs(MyMathHelper.WrapAngle(angles[i] - angles[(i + 1) % angles.Count]));
                UseArc(rayCatchingObject: rayCatchingObjects[i]);
                UseArc(rayCatchingObject: rayCatchingObjects[(i + 1) % rayCatchingObjects.Count]);
                void UseArc(ILightCatchingObject? rayCatchingObject)
                {
                    if (rayCatchingObject is not null)
                    {
                        arcsForObjects[rayCatchingObject] += curArc / 2;
                        usedArc += curArc / 2;
                    }
                }
            }

            popupTextBox.Text = $"generates {state.prodWatts} power\n{usedArc / (2 * MyMathHelper.pi) * 100:0.}% of it is used";

            foreach (var lightCatchingObject in lightCatchingObjects)
            {
                Propor powerPropor = Propor.Create(part: arcsForObjects[lightCatchingObject], whole: 2 * MyMathHelper.pi)!.Value;
                lightCatchingObject.SetWatts
                (
                    starPos: state.starID,
                    watts: powerPropor * state.prodWatts,
                    powerPropor: powerPropor
                );
            }
        }

        void ILightSource.Draw(Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight)
            => polygon.Draw
            (
                worldToScreenTransform: worldToScreenTransform,
                basicEffect: basicEffect,
                actualScreenWidth: actualScreenWidth,
                actualScreenHeight: actualScreenHeight
            );
    }
}
