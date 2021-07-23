using Microsoft.Xna.Framework.Input;
using System;

namespace Game1
{
    public class KeyButton
    {
        private readonly Keys key;
        private readonly Action action;
        private bool prev;

        public KeyButton(Keys key, Action action)
        {
            this.key = key;
            this.action = action;
            prev = false;
        }

        public void Update()
        {
            bool cur = Keyboard.GetState().IsKeyDown(key: key);
            if (C.Click(prev: prev, cur: cur))
                action();
            prev = cur;
        }
    }
}
