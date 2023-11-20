namespace Game1
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class NonSerializableAttribute : Attribute
    { }
}
