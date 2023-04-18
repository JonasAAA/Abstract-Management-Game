namespace Game1
{
    [Serializable]
    public readonly struct Result<TOk, TErrors>
    {
        private readonly TOk okValue;
        private readonly TErrors errorValues;
        private readonly bool isOk, isInitialized;

        public Result(TOk ok)
        {
            okValue = ok;
            errorValues = default!;
            isOk = true;
            isInitialized = true;
        }

        public Result(TErrors errors)
        {
            okValue = default!;
            errorValues = errors;
            isOk = false;
            isInitialized = true;
        }

        public void SwitchStatement(Action<TOk> ok, Action<TErrors> error)
        {
            Debug.Assert(isInitialized);
            if (isOk)
                ok(okValue);
            else
                error(errorValues);
        }

        public T SwitchExpression<T>(Func<TOk, T> ok, Func<TErrors, T> error)
        {
            Debug.Assert(isInitialized);
            return isOk? ok(okValue) : error(errorValues);
        }

        public Result<T, TErrors> Map<T>(Func<TOk, T> func)
        {
            Debug.Assert(isInitialized);
            return isOk ? new(ok: func(okValue)) : new(errors: errorValues);
        }

        public Result<T, TErrors> FlatMap<T>(Func<TOk, Result<T, TErrors>> func)
        {
            Debug.Assert(isInitialized);
            return isOk ? func(okValue) : new(errors: errorValues);
        }
    }

    public static class Result
    {
        public static Result<IEnumerable<TOk>, TErrors> FlatMap<TItem, TOk, TErrors>(this IEnumerable<TItem> items, Func<TItem, Result<TOk, TErrors>> func)
        {
            List<TOk> results = new();
            foreach (var item in items)
            {
                (TErrors? errors, bool foundError) = func(item).SwitchExpression
                (
                    ok: result => { results.Add(result); return (errors: default(TErrors), foundError: false); },
                    error: errors => (errors: errors, foundError: true)
                );
                if (foundError)
                    return new(errors: errors!);
            }
            return new(ok: results);
        }

        public static Result<IEnumerable<TOk>, IEnumerable<TError>> FlatMap<TItem, TOk, TError>(this IEnumerable<TItem> items, Func<TItem, Result<TOk, IEnumerable<TError>>> func)
        {
            List<TOk> results = new();
            IEnumerable<TError> errors = Enumerable.Empty<TError>();
            bool foundErrors = false;
            foreach (var item in items)
                func(item).SwitchStatement
                (
                    ok: results.Add,
                    error: newErrors =>
                    {
                        errors = errors.Concat(newErrors);
                        foundErrors = true;
                    }
                );
            if (!foundErrors)
                return new(ok: results);
            else
                return new(errors: errors);
        }

        public static Result<TResult, IEnumerable<TError>> CallFunc<T1, T2, TResult, TError>(this Func<T1, T2, TResult> func, Result<T1, IEnumerable<TError>> arg1,
            Result<T2, IEnumerable<TError>> arg2)
        {
            IEnumerable<TError> errors = Enumerable.Empty<TError>();
            bool foundErrors = false;

            T1? arg1NoErr = Unpack(arg1);
            T2? arg2NoErr = Unpack(arg2);

            if (!foundErrors)
                return new(ok: func(arg1NoErr!, arg2NoErr!));
            else
                return new(errors: errors);

            T? Unpack<T>(Result<T, IEnumerable<TError>> arg)
                => arg.SwitchExpression
                (
                    ok: argTemp => argTemp,
                    error: newErrors =>
                    {
                        errors = errors.Concat(newErrors);
                        foundErrors = true;
                        return default(T);
                    }
                );
        }

        public static Result<TResult, IEnumerable<TError>> CallFunc<T1, T2, T3, TResult, TError>(this Func<T1, T2, T3, TResult> func, Result<T1, IEnumerable<TError>> arg1,
            Result<T2, IEnumerable<TError>> arg2, Result<T3, IEnumerable<TError>> arg3)
        {
            IEnumerable<TError> errors = Enumerable.Empty<TError>();
            bool foundErrors = false;

            T1? arg1NoErr = Unpack(arg1);
            T2? arg2NoErr = Unpack(arg2);
            T3? arg3NoErr = Unpack(arg3);

            if (!foundErrors)
                return new(ok: func(arg1NoErr!, arg2NoErr!, arg3NoErr!));
            else
                return new(errors: errors);

            T? Unpack<T>(Result<T, IEnumerable<TError>> arg)
                => arg.SwitchExpression
                (
                    ok: argTemp => argTemp,
                    error: newErrors =>
                    {
                        errors = errors.Concat(newErrors);
                        foundErrors = true;
                        return default(T);
                    }
                );
        }
    }
}
