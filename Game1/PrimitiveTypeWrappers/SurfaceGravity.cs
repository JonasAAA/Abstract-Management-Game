namespace Game1.PrimitiveTypeWrappers
{
    [Serializable]
    public readonly struct SurfaceGravity : IComparable<SurfaceGravity>, IScalar<SurfaceGravity>
    {
        public static readonly SurfaceGravity zero = new(valueInMetPerSeqSq: 0);

        /// <summary>
        /// From value in m/s^2
        /// </summary>
        public static SurfaceGravity CreateFromMetPerSecSq(UDouble valueInMetPerSeqSq)
            => new(valueInMetPerSeqSq: valueInMetPerSeqSq);

        public readonly UDouble valueInMetPerSeqSq;

        private SurfaceGravity(UDouble valueInMetPerSeqSq)
            => this.valueInMetPerSeqSq = valueInMetPerSeqSq;

        public override string ToString()
            => $"{valueInMetPerSeqSq} m/s^2";

        int IComparable<SurfaceGravity>.CompareTo(SurfaceGravity other)
            => valueInMetPerSeqSq.CompareTo(other.valueInMetPerSeqSq);

        public static Propor Normalize(SurfaceGravity value, SurfaceGravity start, SurfaceGravity stop)
            => UDouble.Normalize(value: value.valueInMetPerSeqSq, start: start.valueInMetPerSeqSq, stop: stop.valueInMetPerSeqSq);

        public static SurfaceGravity Interpolate(Propor normalized, SurfaceGravity start, SurfaceGravity stop)
            => new(valueInMetPerSeqSq: UDouble.Interpolate(normalized: normalized, start: start.valueInMetPerSeqSq, stop: stop.valueInMetPerSeqSq));
    }
}
