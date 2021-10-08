using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public static class LightManager
    {
        public const int maxWidth = 2048;
        public static BasicEffect BasicEffect { get; private set; }

        private const float b = 1;
        private static readonly MyHashSet<ILightCatchingObject> lightCatchingObjects;
        private static readonly MyHashSet<ILightSource> lightSources;
        private static RenderTarget2D renderTarget;
        private static int actualScreenWidth, actualScreenHeight;

        static LightManager()
        {
            lightCatchingObjects = new();
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
                    colorData[i * maxWidth + j] = CalcColor
                    (
                        distFromLight: Vector2.Distance
                        (
                            value1: new Vector2(maxWidth * .5f),
                            value2: new Vector2(i + .5f, j + .5f)
                        )
                    );
            texture.SetData(colorData);

            BasicEffect = new(C.GraphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = true,
                Texture = texture
            };
            //BasicEffect.Texture = texture;
        }

        private static Color CalcColor(float distFromLight)
        {
            //float factor = (float)Math.Exp(-distFromLight * distFromLight / 50000);
            float a = C.standardStarRadius + b;
            float factor = (float)Math.Min(1, a / (b + distFromLight));
            //float factor = distFromLight switch
            //{
            //    <= C.standardStarRadius => 1,
            //    > C.standardStarRadius => 50 / distFromLight,
            //    _ => throw new ArgumentException()
            //};
            return new Color(factor, factor, factor, 1);
            //return new Color(factor, 0, 0, 1);
            //return Color.White * factor;
            //return new Color(0.2f, 0.2f, 0.2f, 1) * factor;
            //float factor = (float)Math.Min(1, 10 / (1 + distFromLight * .1));
            //return new Color(1f, 1f, 1f, factor);
        }

        public static Vector3 Transform(Vector2 pos)
        {
            Vector2 transPos = Vector2.Transform(pos, C.WorldCamera.GetToScreenTransform());
            return new Vector3(2 * transPos.X / actualScreenWidth - 1, 1 - 2 * transPos.Y / actualScreenHeight, 0);
        }

        public static void AddLightCatchingObject(ILightCatchingObject lightCatchingObject)
        {
            lightCatchingObjects.Add(lightCatchingObject);

            lightCatchingObject.Deleted += () => lightCatchingObjects.Remove(lightCatchingObject);
        }

        public static void AddLightSource(ILightSource lightSource)
        {
            lightSources.Add(lightSource);

            lightSource.Deleted += () => lightSources.Remove(lightSource);
        }

        public static void Update()
        {
            Dictionary<ILightCatchingObject, float> powersForObjects = lightCatchingObjects.ToDictionary
            (
                keySelector: lightCatchingObject => lightCatchingObject,
                elementSelector: lightCatchingObject => 0f
            ); 
            foreach (var lightSource in lightSources)
            {
                var newPowersForObjects = lightSource.UpdateAndGetPower(lightCatchingObjects: lightCatchingObjects);
                foreach (var lightCatchingObject in newPowersForObjects.Keys)
                    powersForObjects[lightCatchingObject] += newPowersForObjects[lightCatchingObject];
            }

            foreach (var lightCatchingObject in lightCatchingObjects)
                lightCatchingObject.UsePower(power: powersForObjects[lightCatchingObject]);
        }

        public static void Draw()
        {
            C.GraphicsDevice.SetRenderTarget(renderTarget);
            C.GraphicsDevice.Clear(Color.Transparent);

            //C.GraphicsDevice.BlendState = new BlendState()
            //{
            //    //AlphaBlendFunction = BlendState.AlphaBlend.AlphaBlendFunction,
            //    AlphaBlendFunction = BlendFunction.Add,
            //    //AlphaSourceBlend = BlendState.AlphaBlend.ColorSourceBlend,
            //    AlphaSourceBlend = Blend.One,
            //    AlphaDestinationBlend = Blend.One,
            //    //AlphaDestinationBlend = BlendState.AlphaBlend.AlphaDestinationBlend,
            //    BlendFactor = Color.White,
            //    //ColorBlendFunction = BlendFunction.Add,
            //    ColorBlendFunction = BlendFunction.Add,
            //    //ColorBlendFunction = BlendFunction.Min,
            //    //ColorBlendFunction = BlendFunction.ReverseSubtract,
            //    //ColorBlendFunction = BlendFunction.Subtract,
            //    ColorSourceBlend = Blend.One,
            //    //ColorSourceBlend = Blend.SourceAlpha,
            //    ColorDestinationBlend = Blend.One
            //    //ColorDestinationBlend = Blend.DestinationAlpha
            //    //ColorSourceBlend = BlendState.AlphaBlend.ColorSourceBlend,
            //    //ColorDestinationBlend = Blend.SourceAlpha, //BlendState.AlphaBlend.ColorDestinationBlend,
            //};

            C.GraphicsDevice.BlendState = BlendState.Additive;

            C.GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None
            };

            foreach (var lightSource in lightSources)
                lightSource.Draw();

            C.GraphicsDevice.SetRenderTarget(null);

            C.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, null);

            C.SpriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);

            C.SpriteBatch.End();
        }
    }
}
