namespace Game1.Resources
{
    [Serializable]
    public class ResConfig
    {
        public readonly ConstArray<Resource> resources;

        public ResConfig()
        {
            resources = new
            (
                selector: resInd => new
                (
                    resInd: resInd,
                    weight: (ulong)resInd switch
                    {
                        0 => 1,
                        1 => 2,
                        2 => 10,
                        _ => throw new ArgumentOutOfRangeException()
                    }
                )
            );
        }
    }
}
