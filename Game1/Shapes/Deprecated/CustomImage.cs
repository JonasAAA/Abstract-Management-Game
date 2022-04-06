using Game1.PrimitiveTypeWrappers;

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
        //    Center = new MyVector2(-200, -300),
        //    rotation = 1.235,
        //};
        //customImage.StartEdit();
        //customImage.DrawLineInImage
        //(
        //    worldPos1: customImage.Center,
        //    worldPos2: customImage.Center + new MyVector2(20, 10),
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

        public CustomImage(string imageName, MyVector2? origin = null, UDouble? width = null, UDouble? height = null)
            : base(texture: new CustomTexture(textureName: imageName), origin: origin, width: width, height: height)
        {
            texture = (CustomTexture)base.texture;
        }

        public void StartEdit()
            => texture.StartEdit();

        public void DrawLineInImage(MyVector2 worldPos1, MyVector2 worldPos2, Color color)
            => texture.DrawLineInTexture
            (
                pos1: (Point)TexturePos(position: worldPos1),
                pos2: (Point)TexturePos(position: worldPos2),
                color: color
            );

        public void EndEdit()
            => texture.EndEdit();
    }
}
