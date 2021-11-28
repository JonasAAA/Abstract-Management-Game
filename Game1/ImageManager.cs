using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Game1
{
    public class ImageManager
    {
        private readonly ContentManager contentManager;
        // to not store texture data for no longer needed textures
        // colorDatas[texture][x, y] gives color of texture's (x, y) point
        private readonly Dictionary<string, Texture2D> textures;
        private readonly Dictionary<string, Color[,]> colorDatas;

        public ImageManager(ContentManager contentManager)
        {
            this.contentManager = contentManager;
            colorDatas = new();
        }

        //public void LoadImage(string imageName)
        //{
        //    Texture2D texture = contentManager.Load<Texture2D>(imageName);
        //    colorDatas.GetValue(texture, GetColorData);

        //    static Color[,] GetColorData(Texture2D texture)
        //    {
        //        Color[,] colorData = new Color[texture.Width, texture.Height];
        //        Color[] colorData1D = new Color[texture.Width * texture.Height];
        //        texture.GetData(colorData1D);
        //        for (int x = 0; x < texture.Width; x++)
        //            for (int y = 0; y < texture.Height; y++)
        //                colorData[x, y] = colorData1D[x + y * texture.Width];
        //        return colorData;
        //    }
        //}
    }
}
