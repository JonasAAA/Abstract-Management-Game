using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static Game1.WorldManager;

namespace Game1
{
    public class LightManager
    {
        private readonly MyHashSet<ILightCatchingObject> lightCatchingObjects;
        private readonly MyHashSet<ILightSource> lightSources;
        private readonly RenderTarget2D renderTarget;
        private readonly int actualScreenWidth, actualScreenHeight;
        private readonly BasicEffect brightEffect, dimEffect;

        public LightManager(GraphicsDevice graphicsDevice)
        {
            lightCatchingObjects = new();
            lightSources = new();
            actualScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            actualScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            renderTarget = new(graphicsDevice, actualScreenWidth, actualScreenHeight);
            brightEffect = GetBasicEffect(brightness: CurWorldConfig.brightStarTextureBrigthness);
            dimEffect = GetBasicEffect(brightness: CurWorldConfig.dimStarTextureBrightness);

            BasicEffect GetBasicEffect(double brightness)
            {
                if (brightness <= 0)
                    throw new ArgumentOutOfRangeException();
                Texture2D texture = new(graphicsDevice, CurWorldConfig.lightTextureWidth, CurWorldConfig.lightTextureWidth);
                Color[] colorData = new Color[CurWorldConfig.lightTextureWidth * CurWorldConfig.lightTextureWidth];
                for (int i = 0; i < CurWorldConfig.lightTextureWidth; i++)
                    for (int j = 0; j < CurWorldConfig.lightTextureWidth; j++)
                    {
                        float distFromLight = Vector2.Distance
                        (
                            value1: new Vector2(CurWorldConfig.lightTextureWidth * .5f),
                            value2: new Vector2(i + .5f, j + .5f)
                        );
                        double a = CurWorldConfig.standardStarRadius + brightness;
                        float factor = (float)Math.Min(1, a / (brightness + distFromLight));
                        colorData[i * CurWorldConfig.lightTextureWidth + j] = Color.White * factor;
                    }
                texture.SetData(colorData);

                return new(graphicsDevice)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true,
                    Texture = texture
                };
            }
        }

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

        public void Draw(GraphicsDevice graphicsDevice, Matrix worldToScreenTransform)
        {
            graphicsDevice.SetRenderTarget(renderTarget);
            graphicsDevice.Clear(Color.Transparent);

            graphicsDevice.BlendState = BlendState.Additive;
            graphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None
            };

            foreach (var lightSource in lightSources)
                lightSource.Draw
                (
                    graphicsDevice: graphicsDevice,
                    worldToScreenTransform: worldToScreenTransform,
                    basicEffect: CurOverlay switch
                    {
                        Overlay.Power => brightEffect,
                        _ => dimEffect
                    },
                    actualScreenWidth: actualScreenWidth,
                    actualScreenHeight: actualScreenHeight
                );

            graphicsDevice.SetRenderTarget(null);

            C.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, null);
            C.SpriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            C.SpriteBatch.End();
        }
    }
}
