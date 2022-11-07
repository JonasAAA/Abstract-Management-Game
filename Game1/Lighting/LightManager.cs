using Game1.Delegates;

using static Game1.WorldManager;

namespace Game1.Lighting
{
    [Serializable]
    public sealed class LightManager : IDeletedListener
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

        public void Initialize()
        {
            renderTarget = new(C.GraphicsDevice, actualScreenWidth, actualScreenHeight);
            brightEffect = GetBasicEffect(brightness: CurWorldConfig.brightStarTextureBrigthness);
            dimEffect = GetBasicEffect(brightness: CurWorldConfig.dimStarTextureBrightness);

            return;

            static BasicEffect GetBasicEffect(UDouble brightness)
            {
                if (brightness.IsCloseTo(other: 0))
                    throw new ArgumentOutOfRangeException();

                return new(C.GraphicsDevice)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true,
                    Texture = C.CreateTexture
                    (
                        width: CurWorldConfig.lightTextureWidthAndHeight,
                        height: CurWorldConfig.lightTextureWidthAndHeight,
                        colorFromRelToCenterPos: relToCenterPos => new Color(1, 1, 1, (float)MyMathHelper.Min(1, MyMathHelper.Pow((CurWorldConfig.standardStarRadius + brightness) / (relToCenterPos.Length() + brightness), 2)))
                    )
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
            foreach (var lightCatchingObject in lightCatchingObjects)
                lightCatchingObject.BeginSetWatts();

            // could return from this method if nothing changed since last call (including all positions)
            foreach (var lightSource in lightSources)
                lightSource.GiveWattsToObjects(lightCatchingObjects: lightCatchingObjects.ToList());
        }

        public void Draw(Matrix worldToScreenTransform)
        {
            C.GraphicsDevice.SetRenderTarget(RenderTarget);
            C.GraphicsDevice.Clear(Color.Transparent);
            //BlendState.Additive
            
            C.GraphicsDevice.BlendState = new()
            {
                ColorSourceBlend = Blend.One,
                AlphaSourceBlend = Blend.One,
                ColorDestinationBlend = Blend.One,
                AlphaDestinationBlend = Blend.One,
                ColorBlendFunction = BlendFunction.Max,
                AlphaBlendFunction = BlendFunction.Add,
            };
            // to correctly draw clockwise and counterclocwise triangles
            C.GraphicsDevice.RasterizerState = new()
            {
                CullMode = CullMode.None
            };

            foreach (var lightSource in lightSources)
                lightSource.Draw
                (
                    worldToScreenTransform: worldToScreenTransform,
                    basicEffect: CurWorldManager.Overlay switch
                    {
                        IPowerOverlay => BrightEffect,
                        _ => DimEffect
                    },
                    actualScreenWidth: actualScreenWidth,
                    actualScreenHeight: actualScreenHeight
                );

            C.GraphicsDevice.SetRenderTarget(null);

            C.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, null);
            C.SpriteBatch.Draw(RenderTarget, Vector2.Zero, Color.White);
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
