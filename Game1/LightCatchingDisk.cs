using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1
{
    public class LightCatchingDisk : Ellipse, ILightCatchingObject
    {
        public readonly float radius;

        public event Action Deleted;

        private Action<float> usePower;

        public LightCatchingDisk(float radius)
            : base(width: 2 * radius, height: 2 * radius)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException();
            this.radius = radius;

            LightManager.AddLightCatchingObject(lightCatchingObject: this);
        }

        public void Init(Action<float> usePower)
        {
            if (usePower is null)
                throw new ArgumentNullException();
            this.usePower = usePower;
        }

        public void Delete()
            => Deleted?.Invoke();

        IEnumerable<float> ILightCatchingObject.RelAngles(Vector2 lightPos)
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

        IEnumerable<float> ILightCatchingObject.InterPoints(Vector2 lightPos, Vector2 lightDir)
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

        public void UsePower(float power)
            => usePower(power);
    }
}
