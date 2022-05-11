//namespace Game1.Shapes
//{
//    // about texture.GetData and texture.SetData https://gamedev.stackexchange.com/questions/89567/texture2d-setdata-method-overload
//    [Serializable]
//    public abstract class BaseImage : Shape
//    {
//        public double rotation;

//        protected readonly BaseTexture texture;

//        private readonly MyVector2 origin;
//        private readonly UDouble scaleX, scaleY;

//        protected BaseImage(BaseTexture texture, MyVector2? origin = null, UDouble? width = null, UDouble? height = null)
//        {
//            this.texture = texture;

//            this.origin = origin ?? new(this.texture.Width * .5, this.texture.Height * .5);
//            scaleX = 1;
//            scaleY = 1;
//            if (width is UDouble widthValue)
//            {
//                scaleX = widthValue / (UDouble)this.texture.Width;
//                if (height is null)
//                    scaleY = scaleX;
//            }
//            if (height is UDouble heightValue)
//            {
//                scaleY = heightValue / (UDouble)this.texture.Height;
//                if (width is null)
//                    scaleX = scaleY;
//            }
//            Color = Color.White;
//        }

//        public override bool Contains(MyVector2 position)
//        {
//            Point texturePos = (Point)TexturePos(position: position);

//            if (!texture.Contains(pos: texturePos))
//                return false;
//            return !texture.Color(pos: texturePos).Transparent();
//        }

//        protected override void Draw(Color color)
//            => texture.Draw
//            (
//                position: Center,
//                color: color,
//                rotation: rotation,
//                origin: origin,
//                scaleX: scaleX,
//                scaleY: scaleY
//            );

//        protected MyVector2 TexturePos(MyVector2 position)
//        {
//            Matrix transform = Matrix.CreateTranslation((float)-Center.X, (float)-Center.Y, 0) *
//                Matrix.CreateRotationZ((float)-rotation) *
//                Matrix.CreateScale((float)(1 / scaleX), (float)(1 / scaleY), 1) *
//                Matrix.CreateTranslation((float)origin.X, (float)origin.Y, 0);

//            return MyVector2.Transform(position, transform);
//        }
//    }
//}
