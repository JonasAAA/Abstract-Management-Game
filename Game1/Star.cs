using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class Star : WorldUIElement, ILightSource
    {
        // temporary
        public Vector2 Center
        {
            get => shape.Center;
            set => shape.Center = value;
        }

        /// <summary>
        /// CURRENTLY UNUSED
        /// </summary>
        public event Action Deleted;

        //protected float mainAngle;
        //private readonly float maxAngleDiff;
        private readonly double power;
        private readonly LightPolygon polygon;
        private readonly TextBox textBox;

        public Star(float radius, double power, Color color)
            : base(shape: new Ellipse(width: 2 * radius, height: 2 * radius), active: false, activeColor: Color.AntiqueWhite, inactiveColor: Color.White, popupHorizPos: HorizPos.Right, popupVertPos: VertPos.Top)
        {
            this.power = power;
            polygon = new LightPolygon(strength: radius / C.standardStarRadius, color: color);

            textBox = new();
            textBox.Shape.Center = shape.Center;
            shape.SizeOrPosChanged += () => textBox.Shape.Center = shape.Center;
            AddChild(child: textBox);

            Graph.World.AddUIElement(UIElement: this, layer: 10);

            LightManager.AddLightSource(lightSource: this);
        }

        public void Delete()
            => Deleted?.Invoke();

        // let N = lightCatchingObject.Count()
        // the complexity is O(N ^ 2) as each object has O(1) relevant angles
        // and each object checks intersection with all the rays
        // could maybe get the time down to O(N log N) by using modified interval tree
        Dictionary<ILightCatchingObject, float> ILightSource.UpdateAndGetPower(IEnumerable<ILightCatchingObject> lightCatchingObjects)
        {
            //the current approach (with IEnumerable) may be less efficient
            List<float> angles = new();
            foreach (var lightCatchingObject in lightCatchingObjects)
                angles.AddRange(lightCatchingObject.RelAngles(lightPos: shape.Center));

            const float small = 0.0001f;
            int oldAngleCount = angles.Count;

            for (int i = 0; i < oldAngleCount; i++)
            {
                angles.Add(angles[i] + small);
                angles.Add(angles[i] - small);
            }

            PrepareAngles(angles: ref angles);

            List<Vector2> vertices = new();
            List<ILightCatchingObject> rayCatchingObjects = new();
            const float maxDist = 2000;
            for (int i = 0; i < angles.Count; i++)
            {
                Vector2 rayDir = C.Direction(rotation: angles[i]);
                float minDist = maxDist;
                ILightCatchingObject rayCatchingObject = null;
                foreach (var lightCatchingObject in lightCatchingObjects)
                {
                    var dists = lightCatchingObject.InterPoints(lightPos: shape.Center, lightDir: rayDir);
                    foreach (var dist in dists)
                        if (0 <= dist && dist < minDist)
                        {
                            minDist = dist;
                            rayCatchingObject = lightCatchingObject;
                        }
                }
                rayCatchingObjects.Add(rayCatchingObject);
                //List<float> dists = new();
                //foreach (var lightCatchingObject in lightCatchingObjects)
                //    dists.AddRange(lightCatchingObject.InterPoints(lightPos: shape.Center, lightDir: rayDir));
                //float minDist = dists.Where(dist => dist >= 0).DefaultIfEmpty(maxDist).Min();
                vertices.Add(shape.Center + minDist * rayDir);
            }

            polygon.Update(center: shape.Center, vertices: vertices);

            Dictionary<ILightCatchingObject, float> powersForObjects = lightCatchingObjects.ToDictionary
            (
                keySelector: lightCatchingObject => lightCatchingObject,
                elementSelector: lightCatchingObject => 0f
            );
            double usedPowerProportion = 0;
            for (int i = 0; i < rayCatchingObjects.Count; i++)
                if (rayCatchingObjects[i] is not null && rayCatchingObjects[i] == rayCatchingObjects[(i + 1) % rayCatchingObjects.Count])
                {
                    double curPowerProportion = Math.Abs(MathHelper.WrapAngle(angles[i] - angles[(i + 1) % angles.Count])) / MathHelper.TwoPi;
                    powersForObjects[rayCatchingObjects[i]] += (float)(power * curPowerProportion);
                    usedPowerProportion += curPowerProportion;
                }

            textBox.Text = $"used power\n proportion {usedPowerProportion * 100:0.}%";

            return powersForObjects;
        }

        private static void PrepareAngles(ref List<float> angles)
        {
            for (int i = 0; i < 4; i++)
                angles.Add(i * MathHelper.TwoPi / 4);

            List<float> prepAngles = new();
            foreach (float angle in angles)
            {
                prepAngles.Add(MathHelper.WrapAngle(angle));
                //float prepAngle = MathHelper.WrapAngle(angle - mainAngle);
                //if (Math.Abs(prepAngle) <= maxAngleDiff)
                //    prepAngles.Add(prepAngle + mainAngle);
            }
            //prepAngles.Add(mainAngle + MathHelper.WrapAngle(maxAngleDiff));
            //prepAngles.Add(mainAngle - MathHelper.WrapAngle(maxAngleDiff));
            prepAngles.Sort();

            angles = new List<float>();
            for (int i = 0; i < prepAngles.Count; i++)
            {
                if (i == 0 || prepAngles[i - 1] != prepAngles[i])
                    angles.Add(prepAngles[i]);
            }
        }

        void ILightSource.Draw()
            => polygon.Draw();
    }
}
