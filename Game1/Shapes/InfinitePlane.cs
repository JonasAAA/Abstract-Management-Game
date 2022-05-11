﻿using Game1.UI;

namespace Game1.Shapes
{
    [Serializable]
    public sealed class InfinitePlane : Shape
    {
        private static readonly Texture2D pixelTexture;

        static InfinitePlane()
            => pixelTexture = C.LoadTexture(name: "pixel");

        public override bool Contains(MyVector2 position)
            => true;

        public override void Draw(Color color)
        {
            if (!color.Transparent())
                C.Draw
                (
                    texture: pixelTexture,
                    position: MyVector2.zero,
                    color: color,
                    rotation: 0,
                    origin: MyVector2.zero,
                    scaleX: ActiveUIManager.screenWidth,
                    scaleY: ActiveUIManager.screenHeight
                );
        }
    }
}
