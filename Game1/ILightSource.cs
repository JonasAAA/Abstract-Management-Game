using System.Collections.Generic;

namespace Game1
{
    public interface ILightSource : IDeletable
    {
        public void GivePowerToObjects(IEnumerable<ILightCatchingObject> lightCatchingObjects);

        public void Draw();
    }
}
