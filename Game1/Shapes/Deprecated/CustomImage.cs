﻿using Microsoft.Xna.Framework;
using System;

namespace Game1.Shapes.Deprecated
{
    [Serializable]
    public class CustomImage : BaseImage
    {
        private readonly new CustomTexture texture;

        // Cut from Graph constructor
        //CustomImage customImage = new
        //(
        //    imageName: "triangle",
        //    width: 200,
        //    height: 500
        //)
        //{
        //    Center = new Vector2(-200, -300),
        //    rotation = 1.235f,
        //};
        //customImage.StartEdit();
        //customImage.DrawLineInImage
        //(
        //    worldPos1: customImage.Center,
        //    worldPos2: customImage.Center + new Vector2(20, 10),
        //    color: Color.Transparent
        //);
        //customImage.EndEdit();
        //AddChild
        //(
        //    child: new WorldUIElement
        //    (
        //        shape: customImage,
        //        activeColor: Color.White,
        //        inactiveColor: Color.Red,
        //        popupHorizPos: HorizPos.Left,
        //        popupVertPos: VertPos.Top
        //    ),
        //    layer: 20
        //);

        public CustomImage(string imageName, Vector2? origin = null, float? width = null, float? height = null)
            : base(texture: new CustomTexture(textureName: imageName), origin: origin, width: width, height: height)
        {
            texture = (CustomTexture)base.texture;
        }

        public void StartEdit()
            => texture.StartEdit();

        public void DrawLineInImage(Vector2 worldPos1, Vector2 worldPos2, Color color)
            => texture.DrawLineInTexture
            (
                pos1: TexturePos(position: worldPos1).ToPoint(),
                pos2: TexturePos(position: worldPos2).ToPoint(),
                color: color
            );

        public void EndEdit()
            => texture.EndEdit();
    }
}