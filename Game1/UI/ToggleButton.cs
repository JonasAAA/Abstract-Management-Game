using Microsoft.Xna.Framework;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class ToggleButton : OnOffButton
    {
        public ToggleButton(NearRectangle shape, string text, bool on, Color selectedColor, Color deselectedColor)
            : base(shape: shape, text: text, on: on, selectedColor: selectedColor, deselectedColor: deselectedColor)
        { }

        public override void OnClick()
            => On = !On;
    }
}
