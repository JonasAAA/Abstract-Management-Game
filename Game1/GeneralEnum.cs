namespace Game1
{
    public readonly struct GeneralEnum<T1, T2>
    {
        private readonly T1 value1;
        private readonly T2 value2;
        private readonly bool isCase1;

        public GeneralEnum(T1 value1)
        {
            this.value1 = value1;
            value2 = default!;
            isCase1 = true;
        }

        public GeneralEnum(T2 value2)
        {
            value1 = default!;
            this.value2 = value2;
            isCase1 = false;
        }

        public void SwitchStatement(Action<T1> case1, Action<T2> case2)
        {
            if (isCase1)
                case1(value1);
            else
                case2(value2);
        }

        public TResult SwitchExpression<TResult>(Func<T1, TResult> case1, Func<T2, TResult> case2)
            => isCase1 ? case1(value1) : case2(value2);
    }
}
