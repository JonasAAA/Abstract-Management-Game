using LibTessDotNet;

namespace Game1.OldGeometry
{
    public static class ExtensionMethods
    {
        public static Vec3 ToVec3(this Vector3 vector3)
            => new(vector3.X, vector3.Y, vector3.Z);

        public static Vector3 ToVector3(this Vec3 vec3)
            => new(vec3.X, vec3.Y, vec3.Z);
    }
}
