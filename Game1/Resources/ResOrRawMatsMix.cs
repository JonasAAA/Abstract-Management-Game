namespace Game1.Resources
{
    [Serializable]
    public readonly record struct ResOrRawMatsMix
    {
        public static readonly ResOrRawMatsMix RawMatsMix;

        static ResOrRawMatsMix()
            => RawMatsMix = new(resOrRawMatsMix: null);

        public static ResOrRawMatsMix FromRes(IResource res)
            => new(resOrRawMatsMix: res);

        /// <summary>
        /// If null, then it's raw mats mix
        /// </summary>
        private readonly IResource? resOrRawMatsMix;

        private ResOrRawMatsMix(IResource? resOrRawMatsMix)
            => this.resOrRawMatsMix = resOrRawMatsMix;

        public T SwitchExpression<T>(Func<IResource, T> res, Func<T> rawMatsMix)
            => resOrRawMatsMix switch
            {
                IResource resource => res(resource),
                null => rawMatsMix()
            };

        public void SwitchStatement(Action<IResource> res, Action rawMatsMix)
        {
            if (resOrRawMatsMix is IResource resource)
                res(resource);
            else
                rawMatsMix();
        }
    }
}
