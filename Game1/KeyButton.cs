using Microsoft.Xna.Framework.Input;
using System;

namespace Game1
{
    public class KeyButton
    {
        public bool Click { get; private set; }
        private readonly Keys key;
        private readonly Action action;
        private bool prev;

        public KeyButton(Keys key, Action action = null)
        {
            this.key = key;
            this.action = action;
            prev = false;
            Click = false;
        }

        public void Update()
        {
            bool cur = Keyboard.GetState().IsKeyDown(key: key);
            if (C.Click(prev: prev, cur: cur))
            {
                if (action is not null)
                    action();
                Click = true;
            }
            else
                Click = false;
            prev = cur;
        }
    }

    //public class KeyButton<T>
    //{
    //    private readonly Keys key;
    //    private readonly Func<T> func;
    //    private bool prev;

    //    public KeyButton(Keys key, Func<T> func)
    //    {
    //        this.key = key;
    //        this.func = func;
    //        prev = false;
    //    }

    //    /// <returns>if key is pressed, func(), else default</returns>
    //    public T Update()
    //    {
    //        bool cur = Keyboard.GetState().IsKeyDown(key: key);
    //        T answer = default;
    //        if (C.Click(prev: prev, cur: cur))
    //            answer = func();
    //        prev = cur;
    //        return answer;
    //    }
    //}
}
