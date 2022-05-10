using System.Collections;

namespace Game1
{
    public interface IMyArray<T> : IEnumerable<T>
    {
        public T this[ResInd resInd] { get; }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
            => (from resInd in ResInd.All
                select this[resInd]).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
