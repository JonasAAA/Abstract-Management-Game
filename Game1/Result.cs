using Game1.Collections;

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

        public Result<T, TErrors> Select<T>(Func<TOk, T> func)
        {
            Debug.Assert(isInitialized);
            return isOk ? new(ok: func(okValue)) : new(errors: errorValues);
        }

        // This method makes it possible to write things like
        // from a in aOrErr
        // from b in bOrErr
        // select (a, b)
        // The problem is that it doesn't accumulate errors from such syntax, because the way it's translated into c#, it's assumed that b depends on a.
        //public Result<TOutOk, TErrors> SelectMany<TInOk, TOutOk>(Func<TOk, Result<TInOk, TErrors>> resultSelector, Func<TOk, TInOk, TOutOk> transform)
        //{
        //    Debug.Assert(isInitialized);
        //    // this copy is needed so the compiler doesn't complain
        //    TOk okValueCopy = okValue;
        //    return isOk switch
        //    {
        //        true => resultSelector(okValue).Select(innerOk => transform(okValueCopy, innerOk)),
        //        false => new(errors: errorValues)
        //    };
        //}

        public Result<T, TErrors> SelectMany<T>(Func<TOk, Result<T, TErrors>> func)
        {
            Debug.Assert(isInitialized);
            return isOk ? func(okValue) : new(errors: errorValues);
        }
    }

    public static class Result
    {
        //public static Result<IEnumerable<TOk>, TErrors> FlatMap<TItem, TOk, TErrors>(this IEnumerable<TItem> items, Func<TItem, Result<TOk, TErrors>> func)
        //{
        //    List<TOk> results = new();
        //    foreach (var item in items)
        //    {
        //        (TErrors? errors, bool foundError) = func(item).SwitchExpression
        //        (
        //            ok: result => { results.Add(result); return (errors: default(TErrors), foundError: false); },
        //            error: errors => (errors: errors, foundError: true)
        //        );
        //        if (foundError)
        //            return new(errors: errors!);
        //    }
        //    return new(ok: results);
        //}

        /// <summary>
        /// FlatMap in Scala
        /// </summary>
        public static Result<IEnumerable<TResultItem>, EfficientReadOnlyHashSet<TError>> SelectMany<TItem, TOk, TError, TResultItem>(this IEnumerable<TItem> items, Func<TItem, Result<TOk, EfficientReadOnlyHashSet<TError>>> collectionSelector, Func<TItem, TOk, TResultItem> resultSelector)
        {
            List<TResultItem> results = new();
            HashSet<TError> errors = new();
            foreach (var item in items)
                collectionSelector(item).SwitchStatement
                (
                    ok: ok => results.Add(resultSelector(item, ok)),
                    error: newErrors => errors.UnionWith(newErrors)
                );
            if (errors.Count is 0)
                return new(ok: results);
            else
                return new(errors: new(errors));
        }

        public static Result<IEnumerable<TOk>, EfficientReadOnlyHashSet<TError>> SelectMany<TItem, TOk, TError>(this IEnumerable<TItem> items, Func<TItem, Result<TOk, EfficientReadOnlyHashSet<TError>>> func)
        {
            List<TOk> results = new();
            HashSet<TError> errors = new();
            foreach (var item in items)
                func(item).SwitchStatement
                (
                    ok: results.Add,
                    error: newErrors => errors.UnionWith(newErrors)
                );
            if (errors.Count is 0)
                return new(ok: results);
            else
                return new(errors: new(errors));
        }

        // naming inspired by Haskell's lift https://stackoverflow.com/questions/2395697/what-is-lifting-in-haskell
        public static Result<TResult, EfficientReadOnlyHashSet<TError>> Lift<T1, T2, TResult, TError>(this Func<T1, T2, TResult> func, Result<T1, EfficientReadOnlyHashSet<TError>> arg1,
            Result<T2, EfficientReadOnlyHashSet<TError>> arg2)
        {
            HashSet<TError> errors = new();

            T1? arg1NoErr = Unpack(arg1);
            T2? arg2NoErr = Unpack(arg2);

            if (errors.Count is 0)
                return new(ok: func(arg1NoErr!, arg2NoErr!));
            else
                return new(errors: new(errors));

            T? Unpack<T>(Result<T, EfficientReadOnlyHashSet<TError>> arg)
                => arg.SwitchExpression
                (
                    ok: argTemp => argTemp,
                    error: newErrors =>
                    {
                        errors.UnionWith(newErrors);
                        return default(T);
                    }
                );
        }

        public static Result<TResult, EfficientReadOnlyHashSet<TError>> Lift<T1, T2, T3, TResult, TError>(this Func<T1, T2, T3, TResult> func, Result<T1, EfficientReadOnlyHashSet<TError>> arg1,
            Result<T2, EfficientReadOnlyHashSet<TError>> arg2, Result<T3, EfficientReadOnlyHashSet<TError>> arg3)
        {
            HashSet<TError> errors = new();

            T1? arg1NoErr = Unpack(arg1);
            T2? arg2NoErr = Unpack(arg2);
            T3? arg3NoErr = Unpack(arg3);

            if (errors.Count is 0)
                return new(ok: func(arg1NoErr!, arg2NoErr!, arg3NoErr!));
            else
                return new(errors: new(errors));

            T? Unpack<T>(Result<T, EfficientReadOnlyHashSet<TError>> arg)
                => arg.SwitchExpression
                (
                    ok: argTemp => argTemp,
                    error: newErrors =>
                    {
                        errors.UnionWith(newErrors);
                        return default(T);
                    }
                );
        }

        public static Result<TResult, EfficientReadOnlyHashSet<TError>> Lift<T1, T2, T3, T4, TResult, TError>(this Func<T1, T2, T3, T4, TResult> func, Result<T1, EfficientReadOnlyHashSet<TError>> arg1,
            Result<T2, EfficientReadOnlyHashSet<TError>> arg2, Result<T3, EfficientReadOnlyHashSet<TError>> arg3, Result<T4, EfficientReadOnlyHashSet<TError>> arg4)
        {
            HashSet<TError> errors = new();

            T1? arg1NoErr = Unpack(arg1);
            T2? arg2NoErr = Unpack(arg2);
            T3? arg3NoErr = Unpack(arg3);
            T4? arg4NoErr = Unpack(arg4);

            if (errors.Count is 0)
                return new(ok: func(arg1NoErr!, arg2NoErr!, arg3NoErr!, arg4NoErr!));
            else
                return new(errors: new(errors));

            //T? UnpackLocal<T>(Result<T, IEnumerable<TError>> arg)
            //{
            //    var unpacked = Unpack<T, TError>(arg: arg);
            //    foundErrors &= unpacked.foundErrors;
            //    errors = errors.Concat(unpacked.errors);
            //    return unpacked.argVal;
            //}
            T? Unpack<T>(Result<T, EfficientReadOnlyHashSet<TError>> arg)
                => arg.SwitchExpression
                (
                    ok: argTemp => argTemp,
                    error: newErrors =>
                    {
                        errors.UnionWith(newErrors);
                        return default(T);
                    }
                );
        }
    }
}
