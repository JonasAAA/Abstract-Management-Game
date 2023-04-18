using Game1.ContentHelpers;
using Game1.Delegates;
using Game1.GameStates;
using Game1.Shapes;
using Game1.UI;
using System.IO;
using System.Runtime;
using System.Text.Json;

namespace Game1
{
    public sealed class Game1 : Game
    {
        private readonly GraphicsDeviceManager graphics;
        private readonly PlayState playState;
        private MapCreationState? mapCreationState;
        private GameState gameState;

        public Game1()
        {
            graphics = new(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            Window.IsBorderless = true;

            // tries to enable antialiasing. will only work for Monogame 3.8.1 and later
            // this pr fixes the issue https://github.com/MonoGame/MonoGame/pull/7338
            graphics.PreparingDeviceSettings += (sender, e) =>
            {
                graphics.PreferMultiSampling = true;
                e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 8;
            };

            playState = PlayState.curPlayState;
            mapCreationState = null;
            // I know that gameState will be initialized in LoadContent and will not be used before then
            gameState = null!;
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

        protected override void Initialize()
        {
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferMultiSampling = true;
            GraphicsDevice.PresentationParameters.MultiSampleCount = 8;
            graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            //graphics.IsFullScreen = true;

            static void SetToPreserve(object? sender, PreparingDeviceSettingsEventArgs eventargs)
                => eventargs.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(SetToPreserve);

            graphics.ApplyChanges();

            base.Initialize();
        }

        [Serializable]
        private readonly record struct SetGameStateToPause(Game1 Game, GameState PauseMenu) : IAction
        {
            public void Invoke()
                => Game.SetGameState(newGameState: PauseMenu);
        }

        // TODO: make this work not only on my machine
        private static string GetMapsFolderPath()
            => @"C:\Users\Jonas\Desktop\Serious\Game Projects\Abstract Management Game\Game1\Content\Maps";

        private static JsonSerializerOptions JsonSerializerOptions
            => new()
            {
                AllowTrailingCommas = true,
                WriteIndented = true,
            };

        private static IEnumerable<string> GetMapFullPaths()
            => Directory.GetFiles(path: GetMapsFolderPath());

        // required means that the property must be in json, as said here https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/required-properties
        // TODO: could create a json schema for the file and use it to validate file while someone is writing it
        // TODO: could use https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonextensiondataattribute?view=net-7.0
        // to check for fields provided in json but deserialised
        // STARTING with .NET 8, could use https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/missing-members
        private static Result<ValidMapInfo, IEnumerable<string>> LoadMap(string fullMapPath)
        {
            try
            {
                return new
                (
                    ok: ValidMapInfo.CreateOrThrow
                    (
                        mapInfo: JsonSerializer.Deserialize<MapInfo>
                        (
                            json: File.ReadAllText(fullMapPath),
                            options: JsonSerializerOptions
                        )
                    )
                );
            }
            catch (Exception exception)
            {
                return new(errors: new string[] { exception.Message });
            }
        }

        private static Result<FullValidMapInfo, IEnumerable<string>> LoadFullMap(string fullMapPath)
            => LoadMap(fullMapPath: fullMapPath).FlatMap(func: FullValidMapInfo.Create);

        private static void SaveMap(string fullMapPath, ValidMapInfo mapInfo, bool readyToUse)
            => File.WriteAllText
            (
                path: fullMapPath,
                contents: JsonSerializer.Serialize<MapInfo>
                (
                    value: mapInfo.ToJsonable(readyToUse: readyToUse),
                    options: JsonSerializerOptions
                )
            );

        private static string GenerateMapName()
        {
            HashSet<string> usedNames = new
            (
                GetMapFullPaths().Select
                (
                    fullMapPath => Path.GetFileNameWithoutExtension(path: fullMapPath)
                )
            );
            for (uint i = 0; ; i++)
            {
                string newName = $"Map {i}";
                if (!usedNames.Contains(newName))
                    return newName;
            }
        }

        protected override void LoadContent()
        {
            C.Initialize
            (
                contentManager: Content,
                graphicsDevice: GraphicsDevice
            );

            // TODO: consider moving this to a constants class or similar
            UDouble buttonWidth = 200, buttonHeight = 30;
            ActionButton continueButton = CreateActionButton
            (
                text: "Continue",
                action: () =>
                {
                    playState.ContinueGame();
                    SetGameState(newGameState: playState);
                },
                tooltipText: "Continue from last save",
                enabled: playState.CanContinueGame()
            );

            playState.OnCreate += () =>
            {
                continueButton.PersonallyEnabled = playState.CanContinueGame();
                Debug.Assert(continueButton.PersonallyEnabled);
            };


            MenuState mapCreationStatePauseMenu = new();
            MenuState mapEditorMenu = new();
            MenuState chooseMapMenu = new();
            MenuState mainMenu = new();
            MenuState playStatePauseMenu = new();


            mapEditorMenu.Initialize
            (
                getActionButtons: () => new List<ActionButton>
                (
                    new[]
                    {
                        CreateActionButton
                        (
                            text: "Create new map",
                            action: () =>
                            {
                                mapCreationState = MapCreationState.CreateNewMap
                                (
                                    switchToPauseMenu: new SetGameStateToPause(Game: this, PauseMenu: mapCreationStatePauseMenu),
                                    mapName: GenerateMapName()
                                );
                                SetGameState(newGameState: mapCreationState);
                            },
                            tooltipText: "Create and edit new map"
                        )
                    }.Concat
                    (
                        GetMapFullPaths().Select
                        (
                            mapFullPath =>
                            {
                                string mapName = Path.GetFileNameWithoutExtension(path: mapFullPath);
                                return CreateActionButton
                                (
                                    text: mapName,
                                    action: () => LoadMap(fullMapPath: mapFullPath).SwitchStatement
                                    (
                                        ok: mapInfo =>
                                        {
                                            mapCreationState = MapCreationState.FromMap
                                            (
                                                switchToPauseMenu: new SetGameStateToPause(Game: this, PauseMenu: mapCreationStatePauseMenu),
                                                mapInfo: mapInfo,
                                                mapName: mapName
                                            );
                                            SetGameState(newGameState: mapCreationState);
                                        },
                                        error: errors => throw new NotImplementedException($"show exception message in a text box to explain why button can't be clicked.\nInner errors:\n{string.Join(";\n", errors)}")
                                    ),
                                    tooltipText: $"""Edit map named "{mapName}" """
                                );
                            }
                        )
                    ).Append
                    (
                        CreateActionButton
                        (
                            text: "Back",
                            action: () => SetGameState(newGameState: mainMenu),
                            tooltipText: "Back to main menu"
                        )
                    )
                )
            );

            
            chooseMapMenu.Initialize
            (
                getActionButtons: () => GetMapFullPaths().Select
                (
                    mapFullPath => CreateActionButton
                    (
                        text: Path.GetFileNameWithoutExtension(path: mapFullPath),
                        action: () => LoadFullMap(fullMapPath: mapFullPath).SwitchStatement
                        (
                            ok: mapInfo =>
                            {
                                playState.StartNewGame(mapInfo: mapInfo);
                                SetGameState(newGameState: playState);
                            },
#warning Implement what to do when map reading/validation failed
                            error: errors => throw new NotImplementedException($"show exception message in a text box to explain why button can't be clicked\nInner errors:\n{string.Join(";\n", errors)}")
                        ),
                        tooltipText: $"""Start game in map named "{Path.GetFileNameWithoutExtension(path: mapFullPath)}" """,
                        enabled: MapInfo.IsFileReady(mapFullPath: mapFullPath)
                    )
                ).Append
                (
                    CreateActionButton
                    (
                        text: "Back",
                        action: () => SetGameState(newGameState: mainMenu),
                        tooltipText: "Back to main menu"
                    )
                ).ToList()
            );

            
            mainMenu.Initialize
            (
                getActionButtons: () => new List<ActionButton>()
                {
                    continueButton,
                    CreateActionButton
                    (
                        text: "New game",
                        action: () => SetGameState(newGameState: chooseMapMenu),
                        tooltipText: "Start new game"
                    ),
                    CreateActionButton
                    (
                        text: "Edit map",
                        action: () => SetGameState(newGameState: mapEditorMenu),
                        tooltipText: "Edit map"
                    ),
                    CreateActionButton
                    (
                        action: Exit,
                        text: "Exit",
                        tooltipText: "Quit to desktop"
                    ),
                }
            );
            
            playStatePauseMenu.Initialize
            (
                getActionButtons: () => new List<ActionButton>()
                {
                    CreateActionButton
                    (
                        action: () => SetGameState(newGameState: playState),
                        text: "Continue",
                        tooltipText: "Continue from last save"
                    ),
                    CreateActionButton
                    (
                        action: playState.SaveGame,
                        text: "Save",
                        tooltipText: "Save the game. Will override the last save"
                    ),
                    CreateActionButton
                    (
                        action: () =>
                        {
                            playState.SaveGame();
                            SetGameState(newGameState: mainMenu);
                        },
                        text: "Save and exit",
                        tooltipText: "Save the game and exit. Will override the last save"
                    ),
                }
            );
            playState.Initialize
            (
                switchToPauseMenu: new SetGameStateToPause(Game: this, PauseMenu: playStatePauseMenu)
            );

            mapCreationStatePauseMenu.Initialize
            (
                getActionButtons: () => new List<ActionButton>()
                {
                    CreateActionButton
                    (
                        text: "Continue",
                        action: () =>
                        {
                            if (mapCreationState is null)
                                // Could show that there is nothing to continue on hover
                                // Could lead to a page where it's explained that there is nothing to continue
#warning Implement what to do when there is nothing to continue
                                throw new NotImplementedException();
                            SetGameState(newGameState: mapCreationState);
                        },
                        tooltipText: "Continue editing the map"
                    ),
                    CreateActionButton
                    (
                        text: "Save",
                        action: SaveCurrentMap,
                        tooltipText: "Save the map"
                    ),
                    CreateActionButton
                    (
                        action: () =>
                        {
                            SaveCurrentMap();
                            SetGameState(newGameState: mainMenu);
                        },
                        text: "Save and exit",
                        tooltipText: "Save the map and exit to main menu"
                    )
                }
            );

            SetGameState(newGameState: mainMenu);

            return;

            ActionButton CreateActionButton(string text, Action action, string tooltipText, bool enabled = true)
                => new
                (
                    shape: new MyRectangle
                    (
                        width: buttonWidth,
                        height: buttonHeight
                    ),
                    action: action,
                    tooltip: new ImmutableTextTooltip(text: tooltipText),
                    text: text
                )
                {
                    PersonallyEnabled = enabled
                };

            void SaveCurrentMap()
            {
                var mapInfo = mapCreationState!.CurrentMap();
                bool readyToUse = true;
                try
                {
                    FullValidMapInfo.Create(mapInfo: mapInfo);
                }
                catch (ContentException)
                {
                    readyToUse = false;
                }
                SaveMap
                (
                    fullMapPath: Path.Combine(GetMapsFolderPath(), mapCreationState!.mapName + ".json"),
                    mapInfo: mapInfo,
                    readyToUse: readyToUse
                );
            }
        }

        private void SetGameState(GameState newGameState)
        {
            gameState?.OnLeave();
            gameState = newGameState;
            gameState.OnEnter();
        }

        protected override void Update(GameTime gameTime)
        {
            TimeSpan elapsed = gameTime.ElapsedGameTime;

            gameState.Update(elapsed: elapsed);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            gameState.Draw();

            base.Draw(gameTime);
        }
    }
}
