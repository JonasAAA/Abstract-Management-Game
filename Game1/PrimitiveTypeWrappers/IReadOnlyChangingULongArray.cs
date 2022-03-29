namespace Game1.PrimitiveTypeWrappers
{
    // TODO: delete
    //struct A
    //{
    //    private const int size = 10;
    //    private unsafe fixed ulong buffer[size];
        
    //    public ulong this[int index]
    //    {
    //        get
    //        {
    //            if (IndexInRange(index))
    //                return buffer[index];
    //            else
    //                throw new IndexOutOfRangeException();
    //        }
    //        set
    //        {
    //            if (IndexInRange(index))
    //                buffer[index] = value;
    //            else
    //                throw new IndexOutOfRangeException();
    //        }
    //    }

    //    private static bool IndexInRange(int index)
    //        => 0 <= index && index < size;
    //}

    public interface IReadOnlyChangingULongArray : IReadOnlyChangingValue<ConstULongArray>
    {
        // TODO: implement this and related classes/structs
    }
}
