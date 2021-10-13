using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Game1
{
    public interface ILightSource : IDeletable
    {
        public void GiveWattsToObjects(IEnumerable<ILightCatchingObject> lightCatchingObjects);

        public void Draw(Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight);
    }
}
