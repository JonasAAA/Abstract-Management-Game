using Game1.Delegates;

namespace Game1
{
    [Serializable]
    public sealed class KeyButton
    {
        public bool Click { get; private set; }
        public bool Hold { get; private set; }
        private readonly Keys key;
        private readonly IAction? action;
        private bool prev;

        public KeyButton(Keys key, IAction? action = null)
        {
            this.key = key;
            this.action = action;
            prev = false;
            Click = false;
            Hold = false;
        }

        public void Update()
        {
            bool cur = Keyboard.GetState().IsKeyDown(key: key);
            if (C.Click(prev: prev, cur: cur))
            {
                action?.Invoke();
                Click = true;
            }
            else
                Click = false;

            Hold = cur && prev;
            prev = cur;
        }
    }

    //public class KeyButton<T1>
    //{
    //    private readonly Keys key;
    //    private readonly Func<T1> func;
    //    private bool prev;

    //    public KeyButton(Keys key, Func<T1> func)
    //    {
    //        this.key = key;
    //        this.func = func;
    //        prev = false;
    //    }

    //    /// <returns>if key is pressed, func(), else default</returns>
    //    public T1 Update()
    //    {
    //        bool cur = Keyboard.GetState().IsKeyDown(key: key);
    //        T1 answer = default;
    //        if (C.Click(prev: prev, cur: cur))
    //            answer = func();
    //        prev = cur;
    //        return answer;
    //    }
    //}
}
