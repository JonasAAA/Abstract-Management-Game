﻿//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;

//namespace Game1
//{
//    public class Image
//    {
//        public float Width => texture.Width * scale.X;
//        public float Height => texture.Height * scale.Y;
//        public Color Color { private get; set; }
//        private readonly Texture2D texture;
//        private readonly Vector2 origin, scale;

//        public Image(string imageName, float? width = null, float? height = null)
//        {
//            texture = C.LoadTexture(name: imageName);
//            origin = new(texture.Width * .5f, texture.Height * .5f);
//            scale = new(1);
//            if (width.HasValue)
//            {
//                scale.X = width.Value / texture.Width;
//                if (!height.HasValue)
//                    scale.Y = scale.X;
//            }
//            if (height.HasValue)
//            {
//                scale.Y = height.Value / texture.Height;
//                if (!width.HasValue)
//                    scale.X = scale.Y;
//            }
//            Color = Color.White;
//        }

//        public void Draw(Vector2 position, float rotation = 0)
//            => C.Draw
//            (
//                texture: texture,
//                position: position,
//                color: Color,
//                rotation: rotation,
//                origin: origin,
//                scale: scale
//            );
//    }
//}