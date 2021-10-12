using System.Collections.Generic;

namespace Game1
{
    public interface ILightSource : IDeletable
    {
        public void GiveWattsToObjects(IEnumerable<ILightCatchingObject> lightCatchingObjects);

        public void Draw();
    }
}
