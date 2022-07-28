﻿namespace Game1.Lighting
{
    public interface ILightSource : IDeletable
    {
        public void GiveWattsToObjects(List<ILightCatchingObject> lightCatchingObjects);

        public void Draw(Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight);
    }
}
