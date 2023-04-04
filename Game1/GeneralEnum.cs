namespace Game1
{
    [Serializable]
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

    public static class GeneralEnum
    {
        public static GeneralEnum<TResult, IEnumerable<TErr>> CallFunc<T, TResult, TErr>(this Func<T, TResult> func, GeneralEnum<T, IEnumerable<TErr>> arg)
            => arg.SwitchExpression<GeneralEnum<TResult, IEnumerable<TErr>>>
            (
                case1: argTemp => new(value1: func(argTemp)),
                case2: errors => new(value2: errors)
            );

        public static GeneralEnum<TResult, IEnumerable<TErr>> CallFunc<T1, T2, TResult, TErr>(Func<T1, T2, TResult> func, GeneralEnum<T1, IEnumerable<TErr>> arg1,
            GeneralEnum<T2, IEnumerable<TErr>> arg2)
        {
            IEnumerable<TErr> errors = Enumerable.Empty<TErr>();
            bool foundErrors = false;

            T1? arg1NoErr = Unpack(arg1);
            T2? arg2NoErr = Unpack(arg2);

            if (!foundErrors)
                return new(value1: func(arg1NoErr!, arg2NoErr!));
            else
                return new(value2: errors);

            T? Unpack<T>(GeneralEnum<T, IEnumerable<TErr>> arg)
                => arg.SwitchExpression
                (
                    case1: argTemp => argTemp,
                    case2: newErrors =>
                    {
                        errors = errors.Concat(newErrors);
                        foundErrors = true;
                        return default(T);
                    }
                );
        }

        public static GeneralEnum<TResult, IEnumerable<TErr>> CallFunc<T1, T2, T3, TResult, TErr>(this Func<T1, T2, T3, TResult> func, GeneralEnum<T1, IEnumerable<TErr>> arg1,
            GeneralEnum<T2, IEnumerable<TErr>> arg2, GeneralEnum<T3, IEnumerable<TErr>> arg3)
        {
            IEnumerable<TErr> errors = Enumerable.Empty<TErr>();
            bool foundErrors = false;

            T1? arg1NoErr = Unpack(arg1);
            T2? arg2NoErr = Unpack(arg2);
            T3? arg3NoErr = Unpack(arg3);

            if (!foundErrors)
                return new(value1: func(arg1NoErr!, arg2NoErr!, arg3NoErr!));
            else
                return new(value2: errors);

            T? Unpack<T>(GeneralEnum<T, IEnumerable<TErr>> arg)
                => arg.SwitchExpression
                (
                    case1: argTemp => argTemp,
                    case2: newErrors =>
                    {
                        errors = errors.Concat(newErrors);
                        foundErrors = true;
                        return default(T);
                    }
                );
        }
    }
}
