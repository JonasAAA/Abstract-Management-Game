namespace Game1.Resources
{
    [Serializable]
    public class ResConfig
    {
        public readonly ResourceArray resources;

        public ResConfig()
        {
            resources = new();
            // TODO: delete
            //resources = new
            //(
            //    selector: resInd => new
            //    (
            //        resInd: resInd,
            //        mass: (ulong)resInd switch
            //        {
            //            0 => 1,
            //            1 => 2,
            //            2 => 10,
            //            _ => throw new ArgumentOutOfRangeException()
            //        },
            //        area: (ulong)resInd switch
            //        {
            //            0 => 10,
            //            1 => 2,
            //            2 => 1,
            //            _ => throw new ArgumentOutOfRangeException()
            //        }
            //    )
            //);
        }
    }
}
