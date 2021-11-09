using System.Runtime.Serialization;

namespace Game1.UI
{
    [DataContract]
    public class HUDElement : UIElement, IHUDElement
    {
        [DataMember] public NearRectangle Shape { get; private init; }

        public HUDElement(NearRectangle shape, string explanation = defaultExplanation)
            : base(shape: shape, explanation: explanation)
        {
            Shape = shape;
        }
    }
}
