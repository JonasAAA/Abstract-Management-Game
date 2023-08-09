namespace Game1.Shapes
{
    [Serializable]
    public readonly record struct PosEnums(HorizPosEnum HorizPos, VertPosEnum VertPos)
    {
        public MyVector2 GetPosInRect(MyVector2 center, UDouble width, UDouble height)
            => center + new MyVector2((int)HorizPos * width, (int)VertPos * height) * .5;
        
        public MyVector2 GetRectCenter(MyVector2 position, UDouble width, UDouble height)
            => position - new MyVector2((int)HorizPos * width, (int)VertPos * height) * .5;
    }
}
