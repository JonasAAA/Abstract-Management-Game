using Game1.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.Lighting
{
    [DataContract]
    public class LightManager : IDeletedListener
    {
        [DataMember] private readonly Dictionary<ILightCatchingObject> lightCatchingObjects;
        [DataMember] private readonly Dictionary<ILightSource> lightSources;

        [NonSerialized] private int actualScreenWidth, actualScreenHeight;
        [NonSerialized] private RenderTarget2D renderTarget;
        [NonSerialized] private BasicEffect brightEffect, dimEffect;

        public LightManager()
        {
            lightCatchingObjects = new();
            lightSources = new();
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
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

            lightCatchingObject.Deleted.Add(listener: this);
        }

        public void AddLightSource(ILightSource lightSource)
        {
            lightSources.Add(lightSource);

            lightSource.Deleted.Add(listener: this);
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
                    basicEffect: CurWorldManager.Overlay switch
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

        void IDeletedListener.DeletedResponse(IDeletable deletable)
        {
            switch (deletable)
            {
                case ILightCatchingObject lightCatchingObject:
                    lightCatchingObjects.Remove(lightCatchingObject);
                    break;
                case ILightSource lightSource:
                    lightSources.Remove(lightSource);
                    break;
                default:
                    throw new ArgumentException();
            }
        }
    }
}
