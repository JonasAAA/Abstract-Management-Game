using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Game1
{
    public interface IShadowCastingObject : IDeletable
    {
        public IEnumerable<float> RelAngles(Vector2 lightPos);

        public IEnumerable<float> InterPoints(Vector2 lightPos, Vector2 lightDir);
    }
}
