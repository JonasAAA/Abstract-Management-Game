using System.Collections;
using static Game1.WorldManager;

namespace Game1.Collections
{
    [Serializable]
    public readonly struct SortedResSet<TRes> : IEnumerable<TRes>
        where TRes : class, IResource
    {
        public static readonly SortedResSet<TRes> empty = new(resSet: new());

        /// <summary>
        /// In production doesn't check that the list is actually sorted
        /// </summary>
        public static SortedResSet<TRes> FromSortedUniqueResListUnsafe(EfficientReadOnlyCollection<TRes> sortedUniqueResList)
            => new(resSet: sortedUniqueResList);

        public readonly int count;

        private readonly EfficientReadOnlyCollection<TRes> resSet;

        public SortedResSet(TRes res)
            : this(resSet: new List<TRes>() { res }.ToEfficientReadOnlyCollection())
        { }

        private SortedResSet(EfficientReadOnlyCollection<TRes> resSet)
        {
            this.resSet = resSet;
            count = resSet.Count;
            Validate();
        }

        private void Validate()
        {
#if DEBUG
            for (int i = 1; i < resSet.Count; i++)
                Debug.Assert(CurResConfig.CompareRes(left: resSet[i - 1], right: resSet[i]) < 0);
#endif
        }

        public SortedResSet<IResource> ToAll()
        {
            List<IResource> newResSet = new(count);
            for (int ind = 0; ind < count; ind++)
                newResSet.Add(resSet[ind]);
            return new(newResSet.ToEfficientReadOnlyCollection());
        }

        private static TRes? GetRes(SortedResSet<TRes> resSet, int ind)
            => ind < resSet.count ? resSet.resSet[ind] : null;

        public SortedResSet<TRes> UnionWith(SortedResSet<TRes> otherResSet)
        {
            List<TRes> unionResSet = new(capacity: count + otherResSet.count);
            int thisInd = 0, otherInd = 0;
            while (true)
            {
                TRes? thisRes = GetRes(resSet: this, ind: thisInd);
                TRes? otherRes = GetRes(resSet: otherResSet, ind: otherInd);
                int compare = CurResConfig.CompareNullableRes(thisRes, otherRes);
                TRes? newRes = null;
                if (compare <= 0)
                {
                    newRes = thisRes;
                    thisInd++;
                }
                if (compare >= 0)
                {
                    newRes = otherRes;
                    otherInd++;
                }
                if (newRes is null)
                    break;
                unionResSet.Add(newRes);
            }
            return new(unionResSet.ToEfficientReadOnlyCollection());
        }

        public IEnumerator<TRes> GetEnumerator()
            => resSet.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator()
            => resSet.GetEnumerator();
    }
}
