using System.Collections.Generic;

namespace Game1
{
    public interface ILightSource : IDeletable
    {
        public Dictionary<ILightCatchingObject, float> UpdateAndGetPower(IEnumerable<ILightCatchingObject> lightCatchingObjects);

        public void Draw();
    }
}
