//using System.Runtime.Serialization;

//namespace Game1.UI
//{
//    [DataContract]
//    public class WorldHUD : HUDElement
//    {
//        [DataMember] private readonly HUDPosSetter HUDPosSetter;

//        public WorldHUD()
//            : base(shape: new MyRectangle(width: (float)ActiveUIManager.ScreenWidth, height: (float)ActiveUIManager.ScreenHeight))
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
