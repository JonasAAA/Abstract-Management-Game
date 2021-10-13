using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Game1.WorldManager;

namespace Game1
{
    public class LightManager
    {
        public const int maxWidth = 2048, layer = 5;
        //public BasicEffect BasicEffect
        //    => CurOverlay switch
        //    {
        //        Overlay.Power => brightEffect,
        //        _ => dimEffect
        //    };

        private readonly MyHashSet<ILightCatchingObject> lightCatchingObjects;
        private readonly MyHashSet<ILightSource> lightSources;
        private readonly RenderTarget2D renderTarget;
        private readonly int actualScreenWidth, actualScreenHeight;
        private BasicEffect brightEffect, dimEffect;

        public LightManager()
        {
            lightCatchingObjects = new();
            lightSources = new();
            actualScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            actualScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            renderTarget = new(C.GraphicsDevice, actualScreenWidth, actualScreenHeight);
        }

        public void Initialize()
        {
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
                        double a = CurWorldConfig.standardStarRadius + brightness;
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

        //public void Initialize()
        //{
        //    actualScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        //    actualScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        //    renderTarget = new(C.GraphicsDevice, actualScreenWidth, actualScreenHeight);

        //    brightEffect = GetBasicEffect(brightness: 100);
        //    dimEffect = GetBasicEffect(brightness: 1);

        //    static BasicEffect GetBasicEffect(double brightness)
        //    {
        //        if (brightness <= 0)
        //            throw new ArgumentOutOfRangeException();
        //        Texture2D texture = new(C.GraphicsDevice, maxWidth, maxWidth);
        //        Color[] colorData = new Color[maxWidth * maxWidth];
        //        for (int i = 0; i < maxWidth; i++)
        //            for (int j = 0; j < maxWidth; j++)
        //            {
        //                float distFromLight = Vector2.Distance
        //                (
        //                    value1: new Vector2(maxWidth * .5f),
        //                    value2: new Vector2(i + .5f, j + .5f)
        //                );
        //                double a = C.standardStarRadius + brightness;
        //                float factor = (float)Math.Min(1, a / (brightness + distFromLight));
        //                colorData[i * maxWidth + j] = Color.White * factor;
        //            }
        //        texture.SetData(colorData);

        //        return new(C.GraphicsDevice)
        //        {
        //            TextureEnabled = true,
        //            VertexColorEnabled = true,
        //            Texture = texture
        //        };
        //    }
        //}

        //public Vector3 Transform(Vector2 pos)
        //{
        //    Vector2 transPos = Vector2.Transform(pos, C.WorldCamera.GetToScreenTransform());
        //    return new Vector3(2 * transPos.X / actualScreenWidth - 1, 1 - 2 * transPos.Y / actualScreenHeight, 0);
        //}

        public void AddLightCatchingObject(ILightCatchingObject lightCatchingObject)
        {
            lightCatchingObjects.Add(lightCatchingObject);

            lightCatchingObject.Deleted += () => lightCatchingObjects.Remove(lightCatchingObject);
        }

        public void AddLightSource(ILightSource lightSource)
        {
            lightSources.Add(lightSource);

            lightSource.Deleted += () => lightSources.Remove(lightSource);
        }

        public void Update()
        {
            // could return from this method if nothing changed since last call (including all positions)
            foreach (var lightSource in lightSources)
                lightSource.GiveWattsToObjects(lightCatchingObjects: lightCatchingObjects);
        }

        public void Draw(Matrix worldToScreenTransform)
        {
            C.GraphicsDevice.SetRenderTarget(renderTarget);
            C.GraphicsDevice.Clear(Color.Transparent);

            C.GraphicsDevice.BlendState = BlendState.Additive;
            C.GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None
            };

            foreach (var lightSource in lightSources)
                lightSource.Draw
                (
                    worldToScreenTransform: worldToScreenTransform,
                    basicEffect: CurOverlay switch
                    {
                        Overlay.Power => brightEffect,
                        _ => dimEffect
                    },
                    actualScreenWidth: actualScreenWidth,
                    actualScreenHeight: actualScreenHeight
                );

            C.GraphicsDevice.SetRenderTarget(null);

            C.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, null);
            C.SpriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            C.SpriteBatch.End();
        }
    }
}
