using System.Collections.Generic;

namespace Game1
{
    public interface ILightSource : IDeletable
    {
        public void Update(IEnumerable<IShadowCastingObject> shadowCastingObjects);

        public void Draw();
    }
}
