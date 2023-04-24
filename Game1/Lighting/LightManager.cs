using Game1.Delegates;
using static Game1.WorldManager;

namespace Game1.Lighting
{
    [Serializable]
    public sealed class LightManager : IDeletedListener
    {
        [Serializable]
        private class VacuumAsRadiantEnergyConsumer : IRadiantEnergyConsumer
        {
            private readonly EnergyPile<HeatEnergy> vacuumHeatEnergyPile;

            public VacuumAsRadiantEnergyConsumer(EnergyPile<HeatEnergy> vacuumHeatEnergyPile)
                => this.vacuumHeatEnergyPile = vacuumHeatEnergyPile;

            void IRadiantEnergyConsumer.TakeRadiantEnergyFrom(EnergyPile<RadiantEnergy> source, RadiantEnergy amount)
                => source.TransformTo(destin: vacuumHeatEnergyPile, amount: amount);

            void IRadiantEnergyConsumer.EnergyTakingComplete(IRadiantEnergyConsumer reflectedEnergyDestin)
            { }
        }

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
        private readonly VacuumAsRadiantEnergyConsumer vacuumAsRadiantEnergyConsumer;
        private const string mustInitializeMessage = $"must initialize {nameof(LightManager)} first by calling {nameof(Initialize)}";
        [NonSerialized] private RenderTarget2D? renderTarget;
        [NonSerialized] private BasicEffect? brightEffect, dimEffect;

        public LightManager(EnergyPile<HeatEnergy> vacuumHeatEnergyPile)
        {
            lightCatchingObjects = new();
            lightSources = new();
            vacuumAsRadiantEnergyConsumer = new(vacuumHeatEnergyPile: vacuumHeatEnergyPile);
        }

        public void Initialize()
        {
            renderTarget = new(C.GraphicsDevice, actualScreenWidth, actualScreenHeight);
            brightEffect = GetBasicEffect(brightness: CurWorldConfig.brightStarTextureBrigthness);
            dimEffect = GetBasicEffect(brightness: CurWorldConfig.dimStarTextureBrightness);

            return;

            static BasicEffect GetBasicEffect(UDouble brightness)
            {
                return new(C.GraphicsDevice)
                {
                    TextureEnabled = true,
                    VertexColorEnabled = true,
                    Texture = C.CreateTexture
                    (
                        width: CurWorldConfig.lightTextureWidthAndHeight,
                        height: CurWorldConfig.lightTextureWidthAndHeight,
                        colorFromRelToCenterPos: relToCenterPos => Color.White * (float)Brightness(distFromCenter: relToCenterPos.Length())
                    )
                };

                UDouble Brightness(UDouble distFromCenter)
                    => (UDouble)MyMathHelper.Pow(brightness * CurWorldConfig.standardStarRadius / (MyMathHelper.Max(1, distFromCenter - CurWorldConfig.standardStarRadius) + brightness * CurWorldConfig.standardStarRadius), 2);
            }
        }

        public void AddLightCatchingObject(ILightCatchingObject lightCatchingObject)
        {
            lightCatchingObjects.Add(lightCatchingObject);

            (lightCatchingObject as IDeletable)?.Deleted.Add(listener: this);
        }

        public void AddLightSource(ILightSource lightSource)
        {
            lightSources.Add(lightSource);

            (lightSource as IDeletable)?.Deleted.Add(listener: this);
        }

        public void Update()
        {
            var lightCatchingObjectList = lightCatchingObjects.ToList();
            foreach (var lightSource in lightSources)
                lightSource.ProduceAndDistributeRadiantEnergy
                (
                    lightCatchingObjects: lightCatchingObjectList,
                    vacuumAsRadiantEnergyConsumer
                );
            foreach (var lightCatchingObject in lightCatchingObjectList)
                lightCatchingObject.EnergyTakingComplete(reflectedEnergyDestin: vacuumAsRadiantEnergyConsumer);
        }

        public void Draw(Matrix worldToScreenTransform)
        {
            // RenderTarger stores things in reverse color and alpha values for ease of computation
            // i.e. to get the real image, need to apply x -> 1 - x tranform to all channels
            // TODO: give blending to custom shader, which should blend alphas like so:
            // a = 1 - (1 - a1) * (1 - a2),
            // and all other channels like so:
            // x = (a1 * (1 - a2) * x1 + a2 * (1 - a1) * x2) / (a1 * (1 - a2) + a2 * (1 - a1))
            // Currently all channels follow x = 1 - (1 - x1) * (1 - x2), which creates some unwanted artifacts on stars
            C.GraphicsDevice.SetRenderTarget(RenderTarget);
            C.GraphicsDevice.Clear(Color.White);

            C.GraphicsDevice.BlendState = new()
            {
                ColorSourceBlend = Blend.Zero,
                AlphaSourceBlend = Blend.Zero,
                ColorDestinationBlend = Blend.InverseSourceColor,
                AlphaDestinationBlend = Blend.InverseSourceColor,
                ColorBlendFunction = BlendFunction.Add,
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

            // invert the image, i.e.each color channel transforms x -> 1 - x, same with alpha channel
            C.SpriteBatch.Begin
            (
                blendState: new()
                {
                    ColorSourceBlend = Blend.InverseDestinationColor,
                    AlphaSourceBlend = Blend.InverseDestinationColor,
                    ColorDestinationBlend = Blend.Zero,
                    AlphaDestinationBlend = Blend.Zero,
                    ColorBlendFunction = BlendFunction.Add,
                    AlphaBlendFunction = BlendFunction.Add,
                }
            );
            C.SpriteBatch.Draw(C.PixelTexture, destinationRectangle: new Rectangle(0, 0, actualScreenWidth, actualScreenHeight), Color.White);
            C.SpriteBatch.End();

            // render the lighting to the screen
            C.GraphicsDevice.SetRenderTarget(null);
            C.SpriteBatch.Begin(blendState: BlendState.AlphaBlend);
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
