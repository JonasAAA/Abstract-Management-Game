using Game1.Collections;
using Game1.ContentHelpers;
using Game1.Delegates;
using Game1.GameStates;
using Game1.Shapes;
using Game1.UI;
using System.Globalization;
using System.IO;
using System.Runtime;
using System.Text.Json;

namespace Game1
{
    public sealed class GameMain : Game
    {
        public static EfficientReadOnlyHashSet<Type> NonSerializableTypes()
            => new
            (
                set: new()
                {
                    typeof(GameMain),
                    typeof(SetGameStateToPause)
                }
            );

        private readonly GraphicsDeviceManager graphics;
        private PlayState? playState;
        private MapCreationState? mapCreationState;
        private GameState gameState;

        public GameMain()
        {
            // Since all the spelling and such will use US conventions, want numbers, dates, and so on to use that as well.
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

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

            playState = null;
            mapCreationState = null;
            // I know that gameState will be initialized in LoadContent and will not be used before then
            gameState = null!;
            GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        }

        protected sealed override void Initialize()
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

        private readonly record struct SetGameStateToPause(GameMain Game, GameState PauseMenu) : IAction
        {
            public void Invoke()
                => Game.SetGameState(newGameState: PauseMenu);
        }

        // TODO: make this work not only on my machine
        private static string GetMapsFolderPath()
            => @"C:\Users\Jonas\Desktop\Serious\Game Projects\Abstract Management Game\Game1\Content\Maps";

        // TODO: make this work not only on my machine
        private static string GetGameSaveFilePath
            => @"C:\Users\Jonas\Desktop\Serious\Game Projects\Abstract Management Game\save.bin";

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
        private static Result<ValidMapInfo, TextErrors> LoadMap(string fullMapPath)
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
                return new(errors: new(value: exception.Message));
            }
        }

        private static Result<FullValidMapInfo, TextErrors> LoadFullMap(string fullMapPath)
            => LoadMap(fullMapPath: fullMapPath).SelectMany(func: FullValidMapInfo.Create);

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
            => Algorithms.GanerateNewName
            (
                prefix: "Map",
                usedNames: GetMapFullPaths().Select
                (
                    fullMapPath => Path.GetFileNameWithoutExtension(path: fullMapPath)
                ).ToEfficientReadOnlyHashSet()
            );

        protected sealed override void LoadContent()
        {
            C.Initialize
            (
                contentManager: Content,
                graphicsDevice: GraphicsDevice
            );

            // TODO: consider moving this to a constants class or similar
            UDouble buttonWidth = 200, buttonHeight = 30;

            MenuState mapCreationStatePauseMenu = new();
            MenuState mapCreationStateDoubleCheckIfExitWithoutSaving = new();
            MenuState mapEditorMenu = new();
            MenuState chooseMapMenu = new();
            MenuState mainMenu = new();
            MenuState playStatePauseMenu = new();

            mapEditorMenu.Initialize
            (
                getHUDElements: () => new ActionButton[]
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
                                    error: errors => SwitchToInvalidMapState
                                    (
                                        mapFullPath: mapFullPath,
                                        errors: errors,
                                        goBackMenuState: mapEditorMenu
                                    )
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
            );

            
            chooseMapMenu.Initialize
            (
                getHUDElements: () => GetMapFullPaths().Select
                (
                    mapFullPath => CreateActionButton
                    (
                        text: Path.GetFileNameWithoutExtension(path: mapFullPath),
                        action: () => LoadFullMap(fullMapPath: mapFullPath).SwitchStatement
                        (
                            ok: mapInfo =>
                            {
                                playState = PlayState.StartGame
                                (
                                    switchToPauseMenu: new SetGameStateToPause(Game: this, PauseMenu: playStatePauseMenu),
                                    mapInfo: mapInfo
                                );
                                SetGameState(newGameState: playState);
                            },
                            error: errors => SwitchToInvalidMapState
                            (
                                mapFullPath: mapFullPath,
                                errors: errors,
                                goBackMenuState: chooseMapMenu
                            )
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
                )
            );

            
            mainMenu.Initialize
            (
                getHUDElements: () => new List<ActionButton>()
                {
                    CreateActionButton
                    (
                        text: "Continue",
                        action: () => SetGameState
                        (
                            newGameState: playState ?? PlayState.ContinueFromSave
                            (
                                switchToPauseMenu: new SetGameStateToPause(Game: this, PauseMenu: playStatePauseMenu),
                                saveFilePath: GetGameSaveFilePath
                            ).SwitchExpression
                            (
                                ok: newPlayState =>
                                {
                                    playState = newPlayState;
                                    return playState as GameState;
                                },
                                error: errors =>
                                {
                                    MenuState invalidSaveStateMenu = new();
                                    invalidSaveStateMenu.Initialize
                                    (
                                        getHUDElements: () => new List<IHUDElement>()
                                        {
                                            new TextBox()
                                            {
                                                Text = $"The save loading failed giving the following error:\n{errors}\nIf this save file is from an older game version, it can only be opened in that older game version.\nOtherwise contact the developer about this bug."
                                            },
                                            CreateActionButton
                                            (
                                                text: "Back to menu",
                                                tooltipText: "Back to menu",
                                                action: () => SetGameState(newGameState: mainMenu)
                                            )
                                        }
                                    );
                                    return invalidSaveStateMenu as GameState;
                                }
                            )
                        ),
                        tooltipText: "Continue from last save",
                        enabled: playState is not null || File.Exists(GetGameSaveFilePath)
                    ),
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
                getHUDElements: () => new List<ActionButton>()
                {
                    CreateActionButton
                    (
                        action: () => SetGameState(newGameState: playState!),
                        text: "Continue",
                        tooltipText: "Continue from last save"
                    ),
                    CreateActionButton
                    (
                        action: () => playState!.SaveGame(saveFilePath: GetGameSaveFilePath),
                        text: "Save",
                        tooltipText: "Save the game. Will override the last save"
                    ),
                    CreateActionButton
                    (
                        action: () =>
                        {
                            playState!.SaveGame(saveFilePath: GetGameSaveFilePath);
                            SetGameState(newGameState: mainMenu);
                        },
                        text: "Save and exit",
                        tooltipText: "Save the game and exit. Will override the last save"
                    ),
                }
            );

            mapCreationStatePauseMenu.Initialize
            (
                getHUDElements: () => new List<ActionButton>()
                {
                    CreateActionButton
                    (
                        text: "Continue",
                        action: () => SetGameState(newGameState: mapCreationState!),
                        tooltipText: "Continue editing the map",
                        enabled: mapCreationState is not null
                    ),
                    CreateActionButton
                    (
                        text: "Save",
                        action: SaveCurrentMap,
                        tooltipText: "Save the map"
                    ),
                    CreateActionButton
                    (
                        text: "Save and exit",
                        tooltipText: "Save the map and exit to main menu",
                        action: () =>
                        {
                            SaveCurrentMap();
                            SetGameState(newGameState: mainMenu);
                        }
                    ),
                    CreateActionButton
                    (
                        text: "Exit without saving",
                        tooltipText: "Exit without saving.\nALL CHANGES WILL BE LOST",
                        action: () => SetGameState(newGameState: mapCreationStateDoubleCheckIfExitWithoutSaving)
                    )
                }
            );

            mapCreationStateDoubleCheckIfExitWithoutSaving.Initialize
            (
                getHUDElements: () => new List<IHUDElement>
                {
                    new TextBox()
                    {
                        Text = "Are you sure?"
                    },
                    CreateActionButton
                    (
                        text: "Exit without saving",
                        tooltipText: "ALL CHANGES WILL BE LOST",
                        action: () =>
                        {
                            mapCreationState = null;
                            SetGameState(newGameState: mainMenu);
                        }
                    ),
                    CreateActionButton
                    (
                        text: "Cancel",
                        tooltipText: "Return to map pause menu",
                        action: () => SetGameState(newGameState: mapCreationStatePauseMenu)
                    )
                }
            );

            SetGameState(newGameState: mainMenu);

            return;

            void SwitchToInvalidMapState(string mapFullPath, IEnumerable<string> errors, MenuState goBackMenuState)
            {
                MenuState invalidMapMenu = new();
                invalidMapMenu.Initialize
                (
                    getHUDElements: () => new List<IHUDElement>()
                    {
                        new TextBox()
                        {
                            Text = $"The map is invalid for the following reasons:\n{string.Join("\n", errors)}\n\nChoose another map or open file \"{mapFullPath}\" and fix these problems."
                        },
                        CreateActionButton
                        (
                            text: "Back to menu",
                            tooltipText: "Back to menu",
                            action: () => SetGameState(newGameState: goBackMenuState)
                        )
                    }
                );
                SetGameState(newGameState: invalidMapMenu);
            }

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

        protected sealed override void Update(GameTime gameTime)
        {
            TimeSpan elapsed = gameTime.ElapsedGameTime;

            gameState.Update(elapsed: elapsed);

            base.Update(gameTime);
        }

        protected sealed override void Draw(GameTime gameTime)
        {
            gameState.Draw();

            base.Draw(gameTime);
        }
    }
}
