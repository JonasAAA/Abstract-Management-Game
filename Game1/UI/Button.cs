using Microsoft.Xna.Framework;
using System;

namespace Game1.UI
{
    public class Button : UIRectElement
    {
        private readonly Action action;
        private readonly Color activeColor, passiveColor;
        private readonly Image pixelImage;   

        public Button(float width, float height, Action action, Color activeColor, Color passiveColor)
            : base(width, height)
        {
            this.action = action;
            this.activeColor = activeColor;
            this.passiveColor = passiveColor;
            pixelImage = new(imageName: "pixel", width: width, height: height)
            {
                Color = passiveColor
            };
        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            pixelImage.Color = activeColor;
        }

        public override void OnClick()
        {
            base.OnClick();
            action();
        }

        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            pixelImage.Color = passiveColor;
        }

        public override void Draw()
        {
            pixelImage.Draw(position: TopLeftCorner + new Vector2(Width, Height) * .5f);
            base.Draw();
        }
    }
}
