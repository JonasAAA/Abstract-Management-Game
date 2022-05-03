namespace Game1
{
    // This class is an ugly hack to make inheritors from this class see current Param instance
    // The straight-forward way doesn't work as you can't use "this" during the call to the base constructor 
    // The class will throw an exception if not properly initialized before use
    public abstract record LateInitializer<TParam>
    {
        private const string mustInitializeMessage = $"Must initialize {nameof(LateInitializer<TParam>)} first by calling {nameof(InitializeLast)}";
        private static LateInitializer<TParam>? lastUninitialized;

        public static void InitializeLast(TParam param)
        {
            if (lastUninitialized is null)
                throw new InvalidOperationException("There is nothing to initialize");
            lastUninitialized.param = param;
            lastUninitialized = null;
        }

        protected TParam Param
            => param ?? throw new InvalidOperationException(mustInitializeMessage);

        private TParam? param;

        public LateInitializer()
        {
            if (lastUninitialized is not null)
                throw new InvalidOperationException(mustInitializeMessage);
            lastUninitialized = this;
        }
    }
}
