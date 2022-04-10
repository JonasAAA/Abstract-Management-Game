using Game1.ChangingValues;
using Game1.Delegates;
using Game1.Shapes;

using static Game1.WorldManager;

namespace Game1.Lighting
{
    [Serializable]
    public class LightCatchingDisk : Disk, ILightCatchingObject
    {
        public IEvent<IDeletedListener> Deleted
            => deleted;
        public IReadOnlyDictionary<MyVector2, UDouble> StarPosToWatts
            => starPosToWatts;

        public IReadOnlyDictionary<MyVector2, Propor> StarPosToPowerPropor
            => starPosToPowerPropor;

        public UDouble Watts
            => starPosToWatts.Values.DefaultIfEmpty().Sum();

        private readonly Dictionary<MyVector2, UDouble> starPosToWatts;
        private readonly Dictionary<MyVector2, Propor> starPosToPowerPropor;
        private readonly Event<IDeletedListener> deleted;

        public LightCatchingDisk(IReadOnlyChangingUDouble radius)
            : base(radius: radius)
        {
            starPosToWatts = new();
            starPosToPowerPropor = new();
            deleted = new();

            CurWorldManager.AddLightCatchingObject(lightCatchingObject: this);
        }

        public void Delete()
            => deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));

        IEnumerable<double> ILightCatchingObject.RelAngles(MyVector2 lightPos)
        {
            UDouble dist = MyVector2.Distance(lightPos, Center);
            if (dist <= radius.Value)
                yield break;

            double a = radius.Value / MyVector2.Distance(lightPos, Center),
                  b = MyMathHelper.Sqrt((UDouble)(1 - a * a));
            MyVector2 center = Center * b * b + lightPos * a * a,
                    diff = Center - lightPos,
                    orth = new(diff.Y, -diff.X),
                    point1 = center + orth * a * b - lightPos,
                    point2 = center - orth * a * b - lightPos;
            yield return MyMathHelper.Atan2(point1.Y, point1.X);
            yield return MyMathHelper.Atan2(point2.Y, point2.X);
        }

        IEnumerable<double> ILightCatchingObject.InterPoints(MyVector2 lightPos, MyVector2 lightDir)
        {
            MyVector2 d = lightPos - Center;
            double e = MyVector2.Dot(lightDir, d), 
                f = MyVector2.Dot(d, d) - radius.Value * radius.Value,
                g = e * e - f;
            
            switch (UDouble.Create(value: g))
            {
                case null:
                    yield break;
                case UDouble nonnegG:
                    double h = MyMathHelper.Sqrt(nonnegG);
                    if (double.IsNaN(h))
                        yield break;

                    yield return -e + h + 1;
                    yield return -e - h + 1;
                    break;
            }
        }

        void ILightCatchingObject.SetWatts(MyVector2 starPos, UDouble watts, Propor powerPropor)
        {
            starPosToWatts[starPos] = watts;
            starPosToPowerPropor[starPos] = powerPropor;
        }
    }
}
