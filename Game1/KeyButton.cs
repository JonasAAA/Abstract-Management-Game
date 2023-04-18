using Game1.Delegates;
using Game1.UI;

namespace Game1
{
    [Serializable]
    public sealed class KeyButton
    {
        public bool HalfClicked
            => abstractButton.HalfClicked;
        private readonly Keys key;
        private readonly IAction? action;
        private readonly AbstractButton abstractButton;

        public KeyButton(Keys key, IAction? action = null)
        {
            this.key = key;
            this.action = action;
            abstractButton = new();
        }

        public void Update()
        {
            abstractButton.Update(down: Keyboard.GetState().IsKeyDown(key: key));
            if (abstractButton.Clicked)
                action?.Invoke();
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
    //        if (C.HalfClicked(prev: prev, cur: cur))
    //            answer = func();
    //        prev = cur;
    //        return answer;
    //    }
    //}
}
