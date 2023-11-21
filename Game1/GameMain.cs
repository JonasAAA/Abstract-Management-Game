using Game1.ContentHelpers;
using Game1.ContentNames;
using Game1.Delegates;
using Game1.GameStates;
using Game1.Shapes;
using Game1.UI;
using System.Globalization;
using System.Runtime;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Game1.GameConfig;

namespace Game1
{
    [NonSerializable]
    public sealed class GameMain : Game
    {
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

        [NonSerializable]
        private sealed class SetGameStateToPause(GameMain game, GameState pauseMenu) : IAction
        {
            public void Invoke()
                => game.SetGameState(newGameState: pauseMenu);
        }

        private static FilePath GetGameSaveFilePath()
            => new(directoryPath: DirectoryPath.gameSavesPath, fileNameWithExtension: "save.bin");

        private static JsonSerializerOptions JsonSerializerOptions
            => new()
            {
                AllowTrailingCommas = true,
                WriteIndented = true,
                UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

        private static IEnumerable<FilePath> GetMapPaths(bool editable = true)
            => DirectoryPath.GetMapsPath(editable: editable).GetFilePaths();

        // required means that the property must be in json, as said here https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/required-properties
        // TODO: could create a json schema for the file and use it to validate file while someone is writing it
        private static Result<ValidMapInfo, TextErrors> LoadMap(FilePath mapPath)
        {
            try
            {
                return new
                (
                    ok: ValidMapInfo.CreateOrThrow
                    (
                        mapInfo: JsonSerializer.Deserialize<MapInfo>
                        (
                            json: mapPath.ReadAllText(),
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

        private static Result<FullValidMapInfo, TextErrors> LoadFullMap(FilePath fullMapPath)
            => LoadMap(mapPath: fullMapPath).SelectMany(func: FullValidMapInfo.Create);

        private static void SaveMap(FilePath mapPath, ValidMapInfo mapInfo, bool readyToUse)
            => mapPath.WriteAllText
            (
                contents: JsonSerializer.Serialize<MapInfo>
                (
                    value: mapInfo.ToJsonable(readyToUse: readyToUse),
                    options: JsonSerializerOptions
                )
            );

        private static FilePath GenerateMapPath()
            => new
            (
                directoryPath: DirectoryPath.editableMapsPath,
                fileNameWithExtension: Algorithms.GanerateNewName
                    (
                        prefix: "Map",
                        usedNames: GetMapPaths()
                            .Select(mapPath => mapPath.fileNameNoExtension)
                            .ToEfficientReadOnlyHashSet()
                    ) + ".json"
            );

        protected sealed override void LoadContent()
        {
            C.Initialize
            (
                contentManager: Content,
                graphicsDevice: GraphicsDevice
            );            

            if (!DirectoryPath.editableMapsPath.Exists())
            {
                DirectoryPath.editableMapsPath.CreateDirectory();
                foreach (var mapPath in GetMapPaths(editable: false))
                    mapPath.CopyFileToDirectory(directoryPath: DirectoryPath.editableMapsPath);
            }

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
                                switchToPauseMenu: new SetGameStateToPause(game: this, pauseMenu: mapCreationStatePauseMenu),
                                mapPath: GenerateMapPath()
                            );
                            SetGameState(newGameState: mapCreationState);
                        },
                        tooltipText: "Create and edit new map"
                    )
                }.Concat
                (
                    GetMapPaths().Select
                    (
                        mapPath => CreateActionButton
                        (
                            text: mapPath.fileNameNoExtension,
                            action: () => LoadMap(mapPath: mapPath).SwitchStatement
                            (
                                ok: mapInfo =>
                                {
                                    mapCreationState = MapCreationState.FromMap
                                    (
                                        switchToPauseMenu: new SetGameStateToPause(game: this, pauseMenu: mapCreationStatePauseMenu),
                                        mapInfo: mapInfo,
                                        mapPath: mapPath
                                    );
                                    SetGameState(newGameState: mapCreationState);
                                },
                                error: errors => SwitchToInvalidMapState
                                (
                                    mapFullPath: mapPath,
                                    errors: errors,
                                    goBackMenuState: mapEditorMenu
                                )
                            ),
                            tooltipText: $"""Edit map named "{mapPath.fileNameNoExtension}" """
                        )
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
                getHUDElements: () => GetMapPaths().Select
                (
                    mapPath => CreateActionButton
                    (
                        text: mapPath.fileNameNoExtension,
                        action: () => LoadFullMap(fullMapPath: mapPath).SwitchStatement
                        (
                            ok: mapInfo =>
                            {
                                playState = PlayState.StartGame
                                (
                                    switchToPauseMenu: new SetGameStateToPause(game: this, pauseMenu: playStatePauseMenu),
                                    mapInfo: mapInfo
                                );
                                SetGameState(newGameState: playState);
                            },
                            error: errors => SwitchToInvalidMapState
                            (
                                mapFullPath: mapPath,
                                errors: errors,
                                goBackMenuState: chooseMapMenu
                            )
                        ),
                        tooltipText: $"""Start game in map named "{mapPath.fileNameNoExtension}" """,
                        enabled: MapInfo.IsFileReady(mapFullPath: mapPath)
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
                                switchToPauseMenu: new SetGameStateToPause(game: this, pauseMenu: playStatePauseMenu),
                                saveFilePath: GetGameSaveFilePath()
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
                                            new TextBox
                                            (
                                                text: $"The save loading failed giving the following error:\n{errors}\nIf this save file is from an older game version, it can only be opened in that older game version.\nOtherwise contact the developer about this bug."
                                            ),
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
                        enabled: playState is not null || GetGameSaveFilePath().Exists()
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
                        action: () => playState!.SaveGame(saveFilePath: GetGameSaveFilePath()),
                        text: "Save",
                        tooltipText: "Save the game. Will override the last save"
                    ),
                    CreateActionButton
                    (
                        action: () =>
                        {
                            playState!.SaveGame(saveFilePath: GetGameSaveFilePath());
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
                    new TextBox(text: "Are you sure?"),
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

            void SwitchToInvalidMapState(FilePath mapFullPath, IEnumerable<string> errors, MenuState goBackMenuState)
            {
                MenuState invalidMapMenu = new();
                invalidMapMenu.Initialize
                (
                    getHUDElements: () => new List<IHUDElement>()
                    {
                        new TextBox
                        (
                            text: $"The map is invalid for the following reasons:\n{string.Join("\n", errors)}\n\nChoose another map or open file \"{mapFullPath}\" and fix these problems."
                        ),
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
                        width: CurGameConfig.wideUIElementWidth,
                        height: CurGameConfig.UILineHeight
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
                    mapPath: mapCreationState!.mapPath,
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

        // Implemented according to https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1001#example
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                graphics?.Dispose();
        }
    }
}
