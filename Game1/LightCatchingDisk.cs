using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class LightCatchingDisk : Ellipse, ILightCatchingObject
    {
        public IReadOnlyDictionary<Vector2, double> StarPosToPower
            => starPosToPower;

        public IReadOnlyDictionary<Vector2, double> StarPosToPowerProp
            => starPosToPowerProp;

        public double Power
            => starPosToPower.Values.DefaultIfEmpty().Sum();

        public readonly float radius;

        public event Action Deleted;

        private readonly Dictionary<Vector2, double> starPosToPower, starPosToPowerProp;

        public LightCatchingDisk(float radius)
            : base(width: 2 * radius, height: 2 * radius)
        {
            if (radius <= 0)
                throw new ArgumentOutOfRangeException();
            this.radius = radius;

            starPosToPower = new();
            starPosToPowerProp = new();

            LightManager.AddLightCatchingObject(lightCatchingObject: this);
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

        void ILightCatchingObject.SetPower(Vector2 starPos, double power, double powerPropor)
        {
            if (power < 0)
                throw new ArgumentOutOfRangeException();
            if (powerPropor < 0)
                throw new ArgumentOutOfRangeException();

            starPosToPower[starPos] = power;
            starPosToPowerProp[starPos] = powerPropor;
        }
    }
}
