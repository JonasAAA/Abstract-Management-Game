//using System.Runtime.Serialization;
//using static Game1.WorldManager;

//namespace Game1
//{
//    [Serializable]
//    public class MyTexture : BaseTexture
//    {
//        public static MyTexture GetTexture(string textureName)
//        {
//            if (!CurWorldManager.myTextures.ContainsKey(textureName))
//                CurWorldManager.myTextures[textureName] = new(textureName: textureName);
//            return CurWorldManager.myTextures[textureName];
//        }

//        private MyTexture(string textureName)
//            : base(textureName: textureName, texture: C.LoadTexture(name: textureName))
//        { }

//        [OnDeserialized]
//        private void OnDeserialized(StreamingContext context)
//            => texture = C.LoadTexture(name: textureName);
//    }
//}
