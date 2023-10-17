namespace Game1.Shapes
{
    public static class SegmentAlgos
    {
        public static void Draw(Vector2Bare startPos, Vector2Bare endPos, UDouble width, Color color)
        {
            var texture = C.PixelTexture;
            C.Draw
            (
                texture: texture,
                position: (startPos + endPos) / 2,
                color: color,
                rotation: MyMathHelper.Rotation(vector: endPos - startPos),
                origin: new Vector2Bare(texture.Width, texture.Height) * .5,
                scaleX: Vector2Bare.Distance(startPos, endPos) / (UDouble)texture.Width,
                scaleY: width / (UDouble)texture.Height
            );
        }

        public static void Draw(MyVector2 startPos, MyVector2 endPos, Length width, Color color)
        {
            var texture = C.PixelTexture;
            C.Draw
            (
                texture: texture,
                position: (startPos + endPos) / 2,
                color: color,
                rotation: MyMathHelper.Rotation(vector: endPos - startPos),
                origin: new Vector2Bare(texture.Width, texture.Height) * .5,
                scaleX: MyVector2.Distance(startPos, endPos) / (UDouble)texture.Width,
                scaleY: width / (UDouble)texture.Height
            );
        }
    }
}
