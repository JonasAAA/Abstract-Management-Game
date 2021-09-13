using Microsoft.Xna.Framework;

namespace Game1.UI
{
    public class UIRectangle : UIRectElement
    {
        private readonly Image pixelImage;

        public UIRectangle(float width, float height, Color color)
            : base(width, height)
        {
            pixelImage = new(imageName: "pixel", width: width, height: height)
            {
                Color = color
            };
        }

        public override void OnClick()
        {
            base.OnClick();
            pixelImage.Color = new
            (
                r: (float)C.Random(min: 0, max: 1),
                g: (float)C.Random(min: 0, max: 1),
                b: (float)C.Random(min: 0, max: 1)
            );
        }

        public override void Draw()
        {
            pixelImage.Draw(position: TopLeftCorner + new Vector2(Width, Height) * .5f);
            base.Draw();
        }
    }
}
