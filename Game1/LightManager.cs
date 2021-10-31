using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public class LightManager
    {
        [DataMember]
        private readonly MyHashSet<ILightCatchingObject> lightCatchingObjects;
        [DataMember]
        private readonly MyHashSet<ILightSource> lightSources;
        [NonSerialized]
        private RenderTarget2D renderTarget;
        [NonSerialized]
        private int actualScreenWidth, actualScreenHeight;
        [NonSerialized]
        private BasicEffect brightEffect, dimEffect;

        public LightManager()
        {
            lightCatchingObjects = new();
            lightSources = new();
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            foreach (var lightCatchingObject in lightCatchingObjects)
                DealWithLightCatchingObjectDeletion(lightCatchingObject: lightCatchingObject);

            foreach (var lightSource in lightSources)
                DealWithLightSourceDeletion(lightSource: lightSource);

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

            DealWithLightCatchingObjectDeletion(lightCatchingObject: lightCatchingObject);
        }

        private void DealWithLightCatchingObjectDeletion(ILightCatchingObject lightCatchingObject)
            => lightCatchingObject.Deleted += () => lightCatchingObjects.Remove(lightCatchingObject);

        public void AddLightSource(ILightSource lightSource)
        {
            lightSources.Add(lightSource);

            DealWithLightSourceDeletion(lightSource: lightSource);
        }

        private void DealWithLightSourceDeletion(ILightSource lightSource)
            => lightSource.Deleted += () => lightSources.Remove(lightSource);

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
