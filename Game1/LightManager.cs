using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1
{
    public static class LightManager
    {
        public const int maxWidth = 2048, layer = 5;
        public static BasicEffect BasicEffect
            => Graph.Overlay switch
            {
                Overlay.Power => brightEffect,
                _ => dimEffect
            };

        private static readonly MyHashSet<ILightCatchingObject> lightCatchingObjects;
        private static readonly MyHashSet<ILightSource> lightSources;
        private static RenderTarget2D renderTarget;
        private static int actualScreenWidth, actualScreenHeight;
        private static BasicEffect brightEffect, dimEffect;

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
            
            brightEffect = GetBasicEffect(brightness: 100);
            dimEffect = GetBasicEffect(brightness: 1);

            static BasicEffect GetBasicEffect(double brightness)
            {
                if (brightness <= 0)
                    throw new ArgumentOutOfRangeException();
                Texture2D texture = new(C.GraphicsDevice, maxWidth, maxWidth);
                Color[] colorData = new Color[maxWidth * maxWidth];
                for (int i = 0; i < maxWidth; i++)
                    for (int j = 0; j < maxWidth; j++)
                    {
                        float distFromLight = Vector2.Distance
                        (
                            value1: new Vector2(maxWidth * .5f),
                            value2: new Vector2(i + .5f, j + .5f)
                        );
                        double a = C.standardStarRadius + brightness;
                        float factor = (float)Math.Min(1, a / (brightness + distFromLight));
                        colorData[i * maxWidth + j] = Color.White * factor;
                    }
                texture.SetData(colorData);

                return new(C.GraphicsDevice)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true,
                    Texture = texture
                };
            }
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
            // could return from this method if nothing changed since last call (including all positions)
            foreach (var lightSource in lightSources)
                lightSource.GiveWattsToObjects(lightCatchingObjects: lightCatchingObjects);
        }

        public static void Draw()
        {
            C.GraphicsDevice.SetRenderTarget(renderTarget);
            C.GraphicsDevice.Clear(Color.Transparent);

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
