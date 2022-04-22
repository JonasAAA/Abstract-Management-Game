using Game1.Delegates;

using static Game1.WorldManager;

namespace Game1.Lighting
{
    [Serializable]
    public class LightManager : IDeletedListener
    {
        private static readonly int actualScreenWidth, actualScreenHeight;

        static LightManager()
        {
            actualScreenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            actualScreenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }

        private readonly MySet<ILightCatchingObject> lightCatchingObjects;
        private readonly MySet<ILightSource> lightSources;

        private RenderTarget2D RenderTarget
            => renderTarget ?? throw new InvalidOperationException(mustInitializeMessage);
        private BasicEffect BrightEffect
            => brightEffect ?? throw new InvalidOperationException(mustInitializeMessage);
        private BasicEffect DimEffect
            => dimEffect ?? throw new InvalidOperationException(mustInitializeMessage);

        private const string mustInitializeMessage = $"must initialize {nameof(LightManager)} first by calling {nameof(Initialize)}";
        [NonSerialized] private RenderTarget2D? renderTarget;
        [NonSerialized] private BasicEffect? brightEffect, dimEffect;

        public LightManager()
        {
            lightCatchingObjects = new();
            lightSources = new();
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            renderTarget = new(graphicsDevice, actualScreenWidth, actualScreenHeight);
            brightEffect = GetBasicEffect(brightness: CurWorldConfig.brightStarTextureBrigthness);
            dimEffect = GetBasicEffect(brightness: CurWorldConfig.dimStarTextureBrightness);

            return;

            BasicEffect GetBasicEffect(UDouble brightness)
            {
                if (brightness.IsCloseTo(other: 0))
                    throw new ArgumentOutOfRangeException();
                Texture2D texture = new(graphicsDevice, CurWorldConfig.lightTextureWidth, CurWorldConfig.lightTextureWidth);
                Color[] colorData = new Color[CurWorldConfig.lightTextureWidth * CurWorldConfig.lightTextureWidth];
                for (int i = 0; i < CurWorldConfig.lightTextureWidth; i++)
                    for (int j = 0; j < CurWorldConfig.lightTextureWidth; j++)
                    {
                        UDouble distFromLight = MyVector2.Distance
                        (
                            value1: CurWorldConfig.lightTextureWidth * .5 * new MyVector2(xAndY: 1),
                            value2: new MyVector2(i + .5, j + .5)
                        );
                        UDouble a = CurWorldConfig.standardStarRadius + brightness;
                        UDouble factor = MyMathHelper.Min((UDouble)1, a / (brightness + distFromLight));
                        colorData[i * CurWorldConfig.lightTextureWidth + j] = Color.White * (float)factor;
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
            graphicsDevice.SetRenderTarget(RenderTarget);
            graphicsDevice.Clear(Color.Transparent);

            graphicsDevice.BlendState = BlendState.Additive;
            // to correctly draw clockwise and counterclocwise triangles
            graphicsDevice.RasterizerState = new()
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
                        IPowerOverlay => BrightEffect,
                        _ => DimEffect
                    },
                    actualScreenWidth: actualScreenWidth,
                    actualScreenHeight: actualScreenHeight
                );

            graphicsDevice.SetRenderTarget(null);

            C.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, null);
            C.SpriteBatch.Draw(RenderTarget, (Vector2)MyVector2.zero, Color.White);
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
