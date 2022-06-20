namespace Game1
{
    /// <summary>
    /// Disallows duplicate elements.
    /// Allows Remove operation.
    /// All single-element operations are O(log N) where N is the number of elements in the collection at the time
    /// </summary>
    [Serializable]
    public class UniqueQueue<T>
        where T : notnull
    {
        public ulong Count
            => (ulong)elementToPriority.Count;

        private readonly SortedDictionary<ulong, T> priorityToElement;
        private readonly Dictionary<T, ulong> elementToPriority;
        private ulong nextPriority;

        public UniqueQueue()
        {
            priorityToElement = new();
            elementToPriority = new();
            nextPriority = 0;
        }

        public bool Contains(T element)
            => elementToPriority.ContainsKey(key: element);

        public void Enqueue(T element)
        {
            if (!elementToPriority.TryAdd(key: element, value: nextPriority))
                throw new ArgumentException();
            priorityToElement.Add(key: nextPriority, value: element);
            nextPriority++;
        }

        public T Dequeue()
        {
            if (Count is 0)
                throw new InvalidOperationException();
            var (priority, element) = priorityToElement.First();
            priorityToElement.Remove(key: priority);
            elementToPriority.Remove(key: element);
            return element;
        }

        public T Peek()
        {
            if (Count is 0)
                throw new InvalidOperationException();
            return priorityToElement.Values.First();
        }

        public void Remove(T element)
        {
            if (!TryRemove(element: element))
                throw new ArgumentException();
        }

        /// <returns>true if element was in the collection, false otherwise</returns>
        public bool TryRemove(T element)
        {
            if (elementToPriority.TryGetValue(key: element, value: out ulong priority))
            {
                priorityToElement.Remove(key: priority);
                elementToPriority.Remove(key: element);
                return true;
            }
            return false;
        }
    }
}
