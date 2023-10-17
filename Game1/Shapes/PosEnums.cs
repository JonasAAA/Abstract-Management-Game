namespace Game1.Shapes
{
    [Serializable]
    public readonly record struct PosEnums(HorizPosEnum HorizPos, VertPosEnum VertPos)
    {
        public Vector2Bare GetPosInRect(Vector2Bare center, UDouble width, UDouble height)
            => center + new Vector2Bare((int)HorizPos * width, (int)VertPos * height) * .5;

        public Vector2Bare GetRectCenter(Vector2Bare position, UDouble width, UDouble height)
            => position - new Vector2Bare((int)HorizPos * width, (int)VertPos * height) * .5;

        public MyVector2 GetPosInRect(MyVector2 center, Length width, Length height)
            => center + new MyVector2((int)HorizPos * (SignedLength)width, (int)VertPos * (SignedLength)height) * .5;
        
        public MyVector2 GetRectCenter(MyVector2 position, Length width, Length height)
            => position - new MyVector2((int)HorizPos * (SignedLength)width, (int)VertPos * (SignedLength)height) * .5;
    }
}
