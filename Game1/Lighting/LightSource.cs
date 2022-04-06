//namespace Game1.Lighting
//{
//    public class LightSource : ILightSource
//    {
//        public MyVector2 position;

//        public
//        Action Deleted;

//        protected double mainAngle;
//        private readonly double maxAngleDiff;
//        private readonly LightPolygon polygon;

//        public LightSource(MyVector2 position, double strength, Color color)
//            : this(position: position, mainAngle: 0, maxAngleDiff: MathHelper.TwoPi, strength: strength, color: color)
//        { }

//        public LightSource(MyVector2 position, double mainAngle, double maxAngleDiff, double strength, Color color)
//        {
//            this.position = position;
//            this.mainAngle = mainAngle;
//            this.maxAngleDiff = maxAngleDiff;
//            polygon = new LightPolygon(strength: strength, color: color);

//            LightManager.AddLightSource(lightSource: this);
//        }

//        // let N = shadowCastingObjects.Count()
//        // the complexity is O(N ^ 2) as each object has O(1) relevant angles
//        // and each object checks intersection with all the rays
//        // could maybe get the time down to O(N log N) by using modified interval tree
//        public void UpdateAndGetPower(IEnumerable<IShadowCastingObject> shadowCastingObjects)
//        {
//            //the current approach (with IEnumerable) may be less efficient
//            List<double> angles = new();
//            foreach (var shadowCastingObject in shadowCastingObjects)
//                angles.AddRange(shadowCastingObject.RelAngles(lightPos: position));

//            const double small = 0.0001f;
//            int oldAngleCount = angles.Count;

//            for (int i = 0; i < oldAngleCount; i++)
//            {
//                angles.Add(angles[i] + small);
//                angles.Add(angles[i] - small);
//            }

//            PrepareAngles(angles: ref angles);

//            List<MyVector2> vertices = new();
//            double maxDist = 2000;
//            for (int i = 0; i < angles.Count; i++)
//            {
//                double angle = angles[i];
//                MyVector2 rayDir = C.Direction(rotation: angle);
//                List<double> dists = new();
//                foreach (var shadowCastingObject in shadowCastingObjects)
//                    dists.AddRange(shadowCastingObject.InterPoints(lightPos: position, lightDir: rayDir));
//                double minDist = dists.Where(dist => dist >= 0).DefaultIfEmpty(maxDist).Min();
//                vertices.Add(position + minDist * rayDir);
//            }

//            if (maxAngleDiff * 2 < MathHelper.TwoPi)
//                vertices.Add(position);

//            polygon.InternalUpdate(center: position, vertices: vertices);
//        }

//        private void PrepareAngles(ref List<double> angles)
//        {
//            for (int i = 0; i < 4; i++)
//                angles.Add(i * MathHelper.TwoPi / 4);

//            List<double> prepAngles = new();
//            foreach (double angle in angles)
//            {
//                double prepAngle = MathHelper.WrapAngle(angle - mainAngle);
//                if (MathHelper.Abs(prepAngle) <= maxAngleDiff)
//                    prepAngles.Add(prepAngle + mainAngle);
//            }
//            prepAngles.Add(mainAngle + MathHelper.WrapAngle(maxAngleDiff));
//            prepAngles.Add(mainAngle - MathHelper.WrapAngle(maxAngleDiff));
//            prepAngles.Sort();

//            angles = new List<double>();
//            for (int i = 0; i < prepAngles.Count; i++)
//            {
//                if (i == 0 || prepAngles[i - 1] != prepAngles[i])
//                    angles.Add(prepAngles[i]);
//            }
//        }

//        public void Delete()
//            => Deleted?.Invoke();

//        public void Draw()
//            => polygon.Draw();
//    }
//}
