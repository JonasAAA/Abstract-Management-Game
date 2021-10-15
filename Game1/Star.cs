using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Game1.WorldManager;

namespace Game1
{
    public class Star : WorldUIElement, ILightSource
    {
        /// <summary>
        /// CURRENTLY UNUSED
        /// </summary>
        public event Action Deleted;

        private readonly double prodWatts;
        private readonly LightPolygon polygon;
        private readonly TextBox popupTextBox;

        public Star(float radius, Vector2 center, double prodWatts, Color color)
            : base(shape: new Ellipse(width: 2 * radius, height: 2 * radius), activeColor: Color.AntiqueWhite, inactiveColor: Color.White, popupHorizPos: HorizPos.Right, popupVertPos: VertPos.Top)
        {
            shape.Center = center;
            this.prodWatts = prodWatts;
            polygon = new LightPolygon(strength: radius / CurWorldConfig.standardStarRadius, color: color);

            popupTextBox = new();
            popupTextBox.Shape.Color = Color.White;
            SetPopup
            (
                UIElement: popupTextBox,
                overlays: Enum.GetValues<Overlay>()
            );

            AddLightSource(lightSource: this);
        }

        public void Delete()
            => Deleted?.Invoke();

        // let N = lightCatchingObject.Count()
        // the complexity is O(N ^ 2) as each object has O(1) relevant angles
        // and each object checks intersection with all the rays
        // could maybe get the time down to O(N log N) by using modified interval tree
        void ILightSource.GiveWattsToObjects(IEnumerable<ILightCatchingObject> lightCatchingObjects)
        {
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

            // prepare angles
            for (int i = 0; i < 4; i++)
                angles.Add(i * MathHelper.TwoPi / 4);

            angles = new SortedSet<float>
            (
                from angle in angles
                select MathHelper.WrapAngle(angle)
            ).ToList();

            List<Vector2> vertices = new();
            List<ILightCatchingObject> rayCatchingObjects = new();
            const float maxDist = 2000;
            foreach (float angle in angles)
            {
                Vector2 rayDir = C.Direction(rotation: angle);
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
                vertices.Add(shape.Center + minDist * rayDir);
            }

            Debug.Assert(rayCatchingObjects.Count == angles.Count && vertices.Count == angles.Count);

            polygon.Update(center: shape.Center, vertices: vertices);

            Dictionary<ILightCatchingObject, double> powerPropsForObjects = lightCatchingObjects.ToDictionary
            (
                keySelector: lightCatchingObject => lightCatchingObject,
                elementSelector: lightCatchingObject => .0
            );
            double usedPowerProportion = 0;
            for (int i = 0; i < rayCatchingObjects.Count; i++)
                if (rayCatchingObjects[i] is not null && rayCatchingObjects[i] == rayCatchingObjects[(i + 1) % rayCatchingObjects.Count])
                {
                    double curPowerProportion = Math.Abs(MathHelper.WrapAngle(angles[i] - angles[(i + 1) % angles.Count])) / MathHelper.TwoPi;
                    powerPropsForObjects[rayCatchingObjects[i]] += curPowerProportion;
                    usedPowerProportion += curPowerProportion;
                }

            popupTextBox.Text = $"generates {prodWatts} power\n{usedPowerProportion * 100:0.}% of it is used";

            foreach (var lightCatchingObject in lightCatchingObjects)
                lightCatchingObject.SetWatts
                (
                    starPos: shape.Center,
                    watts: powerPropsForObjects[lightCatchingObject] * prodWatts,
                    powerPropor: powerPropsForObjects[lightCatchingObject]
                );
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
