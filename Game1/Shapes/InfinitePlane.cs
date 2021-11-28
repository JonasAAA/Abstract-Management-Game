using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.Shapes
{
    [Serializable]
    public class InfinitePlane : Shape
    {
        private static readonly Texture2D pixelTexture;

        static InfinitePlane()
            => pixelTexture = C.LoadTexture(name: "pixel");

        public InfinitePlane()
            => Color = Color.Transparent;

        public InfinitePlane(Color color)
            => Color = color;

        public override bool Contains(Vector2 position)
            => true;

        protected override void Draw(Color color)
        {
            if (!color.Transparent())
                C.Draw
                (
                    texture: pixelTexture,
                    position: Vector2.Zero,
                    color: color,
                    rotation: 0,
                    origin: Vector2.Zero,
                    scale: new Vector2((float)ActiveUIManager.ScreenWidth, (float)ActiveUIManager.ScreenHeight)
                );
        }
    }
}
