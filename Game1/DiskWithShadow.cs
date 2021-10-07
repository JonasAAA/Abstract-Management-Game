using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class DiskWithShadow : Ellipse, IShadowCastingObject
    {
        public readonly float radius;

        public event Action Deleted;

        public DiskWithShadow(float radius)
            : base(width: 2 * radius, height: 2 * radius)
        {
            this.radius = radius;

            LightManager.AddShadowCastingObject(shadowCastingObject: this);
        }

        public void Delete()
            => Deleted?.Invoke();

        public IEnumerable<float> RelAngles(Vector2 lightPos)
        {
            float dist = Vector2.Distance(lightPos, Center);
            if (dist <= radius)
                yield break;

            float a = radius / Vector2.Distance(lightPos, Center),
                  b = (float)Math.Sqrt(1 - a * a);
            Vector2 center = Center * b * b + lightPos * a * a,
                    diff = Center - lightPos,
                    orth = new(diff.Y, -diff.X),
                    point1 = center + orth * a * b - lightPos,
                    point2 = center - orth * a * b - lightPos;
            yield return (float)Math.Atan2(point1.Y, point1.X);
            yield return (float)Math.Atan2(point2.Y, point2.X);
        }

        public IEnumerable<float> InterPoints(Vector2 lightPos, Vector2 lightDir)
        {
            //float dist = Vector2.Distance(lightPos, position);
            //if (dist <= radius)
            //    return;

            Vector2 d = lightPos - Center;
            float e = Vector2.Dot(lightDir, d), f = Vector2.Dot(d, d) - radius * radius, g = e * e - f;
            if (g < 0)
                yield break;

            float h = (float)Math.Sqrt(g);

            if (float.IsNaN(h))
                yield break;

            yield return -e + h + 1f;
            yield return -e - h + 1f;
        }
    }
}
