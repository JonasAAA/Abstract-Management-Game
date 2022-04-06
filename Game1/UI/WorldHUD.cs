//

//namespace Game1.UI
//{
//    [Serializable]
//    public class WorldHUD : HUDElement
//    {
//        private readonly HUDPosSetter HUDPosSetter;

//        public WorldHUD()
//            : base(shape: new MyRectangle(width: (double)ActiveUIManager.ScreenWidth, height: (double)ActiveUIManager.ScreenHeight))
//        {
//            HUDPosSetter = new();
//        }

//        public void AddHUDElement(IHUDElement HUDElement, HorizPos horizPos, VertPos vertPos)
//        {
//            if (HUDElement is null)
//                return;

//            HUDPosSetter.AddHUDElement(HUDElement: HUDElement, horizPos: horizPos, vertPos: vertPos);
//            AddChild(child: HUDElement);
//        }

//        public void RemoveHUDElement(IHUDElement HUDElement)
//        {
//            if (HUDElement is null)
//                return;

//            HUDPosSetter.RemoveHUDElement(HUDElement: HUDElement);
//            RemoveChild(child: HUDElement);
//        }
//    }
//}
