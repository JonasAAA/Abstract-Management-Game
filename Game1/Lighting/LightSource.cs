//namespace Game1.Lighting
//{
//    public class LightSource : ILightSource
//    {
//        public Vector2 position;

//        public
//        Action Deleted;

//        protected float mainAngle;
//        private readonly float maxAngleDiff;
//        private readonly LightPolygon polygon;

//        public LightSource(Vector2 position, float strength, Color color)
//            : this(position: position, mainAngle: 0, maxAngleDiff: MathHelper.TwoPi, strength: strength, color: color)
//        { }

//        public LightSource(Vector2 position, float mainAngle, float maxAngleDiff, float strength, Color color)
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
//            List<float> angles = new();
//            foreach (var shadowCastingObject in shadowCastingObjects)
//                angles.AddRange(shadowCastingObject.RelAngles(lightPos: position));

//            const float small = 0.0001f;
//            int oldAngleCount = angles.Count;

//            for (int i = 0; i < oldAngleCount; i++)
//            {
//                angles.Add(angles[i] + small);
//                angles.Add(angles[i] - small);
//            }

//            PrepareAngles(angles: ref angles);

//            List<Vector2> vertices = new();
//            float maxDist = 2000;
//            for (int i = 0; i < angles.Count; i++)
//            {
//                float angle = angles[i];
//                Vector2 rayDir = C.Direction(rotation: angle);
//                List<float> dists = new();
//                foreach (var shadowCastingObject in shadowCastingObjects)
//                    dists.AddRange(shadowCastingObject.InterPoints(lightPos: position, lightDir: rayDir));
//                float minDist = dists.Where(dist => dist >= 0).DefaultIfEmpty(maxDist).Min();
//                vertices.Add(position + minDist * rayDir);
//            }

//            if (maxAngleDiff * 2 < MathHelper.TwoPi)
//                vertices.Add(position);

//            polygon.InternalUpdate(center: position, vertices: vertices);
//        }

//        private void PrepareAngles(ref List<float> angles)
//        {
//            for (int i = 0; i < 4; i++)
//                angles.Add(i * MathHelper.TwoPi / 4);

//            List<float> prepAngles = new();
//            foreach (float angle in angles)
//            {
//                float prepAngle = MathHelper.WrapAngle(angle - mainAngle);
//                if (Math.Abs(prepAngle) <= maxAngleDiff)
//                    prepAngles.Add(prepAngle + mainAngle);
//            }
//            prepAngles.Add(mainAngle + MathHelper.WrapAngle(maxAngleDiff));
//            prepAngles.Add(mainAngle - MathHelper.WrapAngle(maxAngleDiff));
//            prepAngles.Sort();

//            angles = new List<float>();
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
