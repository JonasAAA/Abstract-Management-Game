namespace Game1
{
    [Serializable]
    public abstract class BoolWithExplanationIfFalse
    {
        [Serializable]
        public class True : BoolWithExplanationIfFalse
        {
            public static True Create()
                => trueInstance;

            private static readonly True trueInstance;

            static True()
                => trueInstance = new();

            private True()
            { }
        }

        [Serializable]
        public class False : BoolWithExplanationIfFalse
        {
            public static False Create(string explanation)
                => new(explanation: explanation);

            public readonly string explanation;

            private False(string explanation)
                => this.explanation = explanation;
        }

        public static BoolWithExplanationIfFalse Create(bool value, string explanationIfFalse)
            => value switch
            {
                true => True.Create(),
                false => False.Create(explanation: explanationIfFalse)
            };

        public void SwitchStatement(Action trueCase, Action<string> falseCase)
        {
            switch (this)
            {
                case True:
                    trueCase();
                    break;
                case False @false:
                    falseCase(@false.explanation);
                    break;
                default:
                    throw new Exception();
            }
        }

        public void SwitchStatement(Action trueCase, Action falseCase)
        {
            switch (this)
            {
                case True:
                    trueCase();
                    break;
                case False:
                    falseCase();
                    break;
                default:
                    throw new Exception();
            }
        }

        public T SwitchExpression<T>(Func<T> trueCase, Func<string, T> falseCase)
            => this switch
            {
                True => trueCase(),
                False @false => falseCase(@false.explanation),
                _ => throw new Exception()
            };

        public T SwitchExpression<T>(Func<T> trueCase, Func<T> falseCase)
            => this switch
            {
                True => trueCase(),
                False => falseCase(),
                _ => throw new Exception()
            };

        public static BoolWithExplanationIfFalse operator &(BoolWithExplanationIfFalse bool1, BoolWithExplanationIfFalse bool2)
            => (bool1, bool2) switch
            {
                (True, True) => True.Create(),
                (True, False @false) => @false,
                (False @false, True) => @false,
                (False false1, False false2) => False.Create(explanation: false1.explanation + " and " + false2.explanation),
                _ => throw new Exception()
            };

        public static explicit operator bool(BoolWithExplanationIfFalse boolWithExplanationIfFalse)
            => boolWithExplanationIfFalse is True;

        // TODO: delete or improve
        //public static BoolWithExplanationIfFalse operator |(BoolWithExplanationIfFalse bool1, BoolWithExplanationIfFalse bool2)
        //    => (bool1, bool2) switch
        //    {
        //        (True, True) or (True, False) or (False, True) => True.Create(),
        //        (False false1, False false2) => False.Create(explanation: false1.explanation + " and " + false2.explanation),
        //        _ => throw new Exception()
        //    };
    }
}
