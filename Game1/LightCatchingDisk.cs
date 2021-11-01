using Game1.Events;
using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public class LightCatchingDisk : Ellipse, ILightCatchingObject
    {
        public IReadOnlyDictionary<Vector2, double> StarPosToWatts
            => starPosToWatts;

        public IReadOnlyDictionary<Vector2, double> StarPosToPowerProp
            => starPosToPowerProp;

        public double Watts
            => starPosToWatts.Values.DefaultIfEmpty().Sum();

        [DataMember]
        public readonly float radius;
        [DataMember]
        public Event<IDeletedListener> Deleted { get; private init; }

        [DataMember]
        private readonly Dictionary<Vector2, double> starPosToWatts, starPosToPowerProp;

        public LightCatchingDisk(float radius)
            : base(width: 2 * radius, height: 2 * radius)
        {
            Deleted = new();
            if (radius <= 0)
                throw new ArgumentOutOfRangeException();
            this.radius = radius;

            starPosToWatts = new();
            starPosToPowerProp = new();

            AddLightCatchingObject(lightCatchingObject: this);
        }

        public void Delete()
            => Deleted.Raise(action: listener => listener.Deleted(deletable: this));

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

        void ILightCatchingObject.SetWatts(Vector2 starPos, double watts, double powerPropor)
        {
            if (watts < 0)
                throw new ArgumentOutOfRangeException();
            if (powerPropor < 0)
                throw new ArgumentOutOfRangeException();

            starPosToWatts[starPos] = watts;
            starPosToPowerProp[starPos] = powerPropor;
        }
    }
}
