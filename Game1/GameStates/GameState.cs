using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.GameStates
{
    public abstract class GameState
    {
        public virtual void OnEnter()
        { }

        public virtual void OnLeave()
        { }

        public abstract void Update(TimeSpan elapsed);

        public abstract void Draw(GraphicsDevice graphicsDevice);
    }
}
