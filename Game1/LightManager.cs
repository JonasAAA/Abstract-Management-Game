using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1
{
    public static class LightManager
    {
        public const int maxWidth = 1024;
        public static BasicEffect BasicEffect { get; private set; }

        private static readonly MyHashSet<IShadowCastingObject> shadowCastingObjects;
        private static readonly MyHashSet<ILightSource> lightSources;
        private static RenderTarget2D renderTarget;
        private static int actualScreenWidth, actualScreenHeight;

        static LightManager()
        {
            shadowCastingObjects = new();
            lightSources = new();
        }

        public static void Initialize()
        {
            actualScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            actualScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            renderTarget = new(C.GraphicsDevice, actualScreenWidth, actualScreenHeight);
            Texture2D texture = new(C.GraphicsDevice, maxWidth, maxWidth);
            Color[] colorData = new Color[maxWidth * maxWidth];
            for (int i = 0; i < maxWidth; i++)
                for (int j = 0; j < maxWidth; j++)
                    colorData[i * maxWidth + j] = CalcColor(Vector2.Distance(new Vector2(maxWidth / 2, maxWidth / 2), new Vector2(i, j)));
            texture.SetData(colorData);

            BasicEffect = new(C.GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
            };
            BasicEffect.Texture = texture;
        }

        private static Color CalcColor(float distFromLight)
        {
            //colorData[i * maxWidth + j] = Color.White * (float)Math.Exp(-relDist / 200);
            //colorData[i * maxWidth + j] = Color.White * (float)Math.Exp(-relDist * relDist / 50000);
            //colorData[i * maxWidth + j] = new Color(0f, 0f, 0f, (float)Math.Exp(-1 / relDist / relDist * 5000));

            float factor = (float)Math.Exp(-distFromLight * distFromLight / 50000);
            return new Color(0.2f, 0.2f, 0.2f, 1) * factor;
        }

        public static Vector3 Transform(Vector2 pos)
        {
            Vector2 transPos = Vector2.Transform(pos, C.WorldCamera.GetToScreenTransform());
            return new Vector3(2 * transPos.X / actualScreenWidth - 1, 1 - 2 * transPos.Y / actualScreenHeight, 0);
        }

        public static void AddShadowCastingObject(IShadowCastingObject shadowCastingObject)
        {
            shadowCastingObjects.Add(shadowCastingObject);

            shadowCastingObject.Deleted += () => shadowCastingObjects.Remove(shadowCastingObject);
        }

        public static void AddLightSource(ILightSource lightSource)
        {
            lightSources.Add(lightSource);

            lightSource.Deleted += () => lightSources.Remove(lightSource);
        }

        public static void Update()
        {
            foreach (var lightSource in lightSources)
                lightSource.Update(shadowCastingObjects: shadowCastingObjects);
        }

        public static void Draw()
        {
            // begin draw
            C.GraphicsDevice.SetRenderTarget(renderTarget);
            C.GraphicsDevice.Clear(Color.Transparent);

            foreach (var lightSource in lightSources)
                lightSource.Draw();

            // end draw
            C.GraphicsDevice.SetRenderTarget(null);

            BlendState blendState = new()
            {
                AlphaBlendFunction = BlendState.AlphaBlend.AlphaBlendFunction,
                AlphaSourceBlend = BlendState.AlphaBlend.ColorSourceBlend,
                AlphaDestinationBlend = BlendState.AlphaBlend.AlphaDestinationBlend,
                BlendFactor = Color.White,
                ColorBlendFunction = BlendFunction.Add,//BlendFunction.Max,
                ColorSourceBlend = BlendState.AlphaBlend.ColorSourceBlend,
                ColorDestinationBlend = Blend.SourceAlpha, //BlendState.AlphaBlend.ColorDestinationBlend,
            };

            C.GraphicsDevice.BlendState = blendState;

            C.SpriteBatch.Begin(SpriteSortMode.Deferred, blendState, null, null, null, null, null);

            C.SpriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);

            C.SpriteBatch.End();
        }
    }
}
