using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class UIConfig
    {
        [DataMember] public readonly int standardScreenHeight;
        [DataMember] public readonly float rectOutlineWidth;
        [DataMember] public readonly float letterHeight;
        [DataMember] public readonly Color mouseOnColor;

        public UIConfig()
        {
            standardScreenHeight = 1080;
            rectOutlineWidth = 0;
            letterHeight = 20;
            mouseOnColor = Color.Yellow;
        }
    }
}
