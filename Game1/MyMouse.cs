using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Game1
{
    public static class MyMouse
    {
        public static Vector2 WorldPos => C.WorldCamera.WorldPos(screenPos: ScreenPos);
        public static Vector2 HUDPos => C.HUDCamera.HUDPos(screenPos: ScreenPos);
        private static Vector2 ScreenPos => Mouse.GetState().Position.ToVector2();
        public static bool LeftClick => C.Click(prev: prevLeft, cur: left);
        public static bool RightClick => C.Click(prev: prevRight, cur: right);
        public static bool LeftHold => left;
        public static bool RightHold => right;

        private static bool left, prevLeft, right, prevRight;

        static MyMouse()
        {
            left = false;
            prevLeft = false;
            right = false;
            prevRight = false;
        }

        public static void Update()
        {
            MouseState mouseState = Mouse.GetState();
            prevLeft = left;
            prevRight = right;
            left = mouseState.LeftButton == ButtonState.Pressed;
            right = mouseState.RightButton == ButtonState.Pressed;
        }
    }
}

