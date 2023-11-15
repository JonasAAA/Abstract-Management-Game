using Game1.Collections;
using Game1.ContentHelpers;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using static Game1.GameConfig;

namespace Game1.GameStates
{
    [Serializable]
    public sealed class MapCreationState : GameState
    {
        [Serializable]
        private sealed record BuildingCosmicBodyTextBoxHUDPosUpdater(MapCreationState MapCreationState, StartingBuilding StartingBuilding) : IAction
        {
            void IAction.Invoke()
            {
                if (MapCreationState.CurMapInfo.StartingInfo.StartingBuildingToCosmicBodyId[StartingBuilding] is CosmicBodyId startingBuildingCosmicBodyId)
                    MapCreationState.startingBuildingToTextBox[StartingBuilding].Shape.Center = MapCreationState.CurCosmicBodyHUDPos(cosmicBodyId: startingBuildingCosmicBodyId);
            }
        }

        private interface IWorldUIElementId
        { }

        // This is not struct as it's often used as IWorldUIElementId, which would result in a lot of boxing and unboxing
        [Serializable]
        private sealed record CosmicBodyId : IWorldUIElementId, IComparable<CosmicBodyId>
        {
            private static uint nextId = 0;

            private readonly uint id;

            public CosmicBodyId()
            {
                id = nextId;
                nextId++;
            }

            public override string ToString()
                => id.ToString();
            
            int IComparable<CosmicBodyId>.CompareTo(CosmicBodyId? other)
                // Convention of what to do when comparing to null https://stackoverflow.com/a/23787253
                => other is null ? 1 : id.CompareTo(other.id);
        }

        [Serializable]
        private sealed record LinkId : IWorldUIElementId, IComparable<LinkId>
        {
            private static uint nextId = 0;

            private readonly uint id;

            public LinkId()
            {
                id = nextId;
                nextId++;
            }

            public override string ToString()
                => id.ToString();

            int IComparable<LinkId>.CompareTo(LinkId? other)
                => other is null ? 1 : id.CompareTo(other.id);
        }

        [Serializable]
        private readonly record struct CosmicBodyInfoInternal(WorldCamera WorldCamera, CosmicBodyId Id, string Name, MyVector2 Position, Length Radius)
        {
            [Serializable]
            private sealed record CosmicBodyShapeParams(CosmicBodyInfoInternal CosmicBody) : Disk.IParams
            {
                public MyVector2 Center
                    => CosmicBody.Position;

                public Length Radius
                    => CosmicBody.Radius;
            }

            public Shape GetShape()
                => new Disk
                (
                    parameters: new CosmicBodyShapeParams(CosmicBody: this),
                    worldCamera: WorldCamera
                );
        }

        [Serializable]
        private readonly record struct LinkInfoInternal(WorldCamera WorldCamera, LinkId Id, CosmicBodyId From, CosmicBodyId To)
        {
            [Serializable]
            private readonly record struct LinkShapeParams(MapInfoInternal CurMapInfo, LinkInfoInternal Link) : VectorShape.IParams
            {
                public MyVector2 StartPos
                    => CurMapInfo.CosmicBodies[Link.From].Position;

                public MyVector2 EndPos
                    => CurMapInfo.CosmicBodies[Link.To].Position;

                public Length Width
                    => Length.CreateFromM(CurGameConfig.linkPixelWidth);
            }

            public Shape GetShape(MapInfoInternal curMapInfo)
                => new LineSegment
                (
                    parameters: new LinkShapeParams(CurMapInfo: curMapInfo, Link: this),
                    worldCamera: WorldCamera
                );
        }

        [Serializable]
        private readonly record struct StartingInfoInternal(MyVector2 WorldCenter, Length CameraViewHeight, EnumDict<StartingBuilding, CosmicBodyId?> StartingBuildingToCosmicBodyId);

        [Serializable]
        private record struct MapInfoInternal(ImmutableDictionary<CosmicBodyId, CosmicBodyInfoInternal> CosmicBodies, ImmutableDictionary<LinkId, LinkInfoInternal> Links, StartingInfoInternal StartingInfo)
        {
            public static (MapInfoInternal mapInfo, WorldCamera worldCamera) CreateEmpty()
            {
                var worldCenter = MyVector2.zero;
                var cameraViewHeight = Length.CreateFromM(ActiveUIManager.standardScreenHeight);
                return
                (
                    mapInfo: new
                    (
                        CosmicBodies: ImmutableDictionary<CosmicBodyId, CosmicBodyInfoInternal>.Empty,
                        Links: ImmutableDictionary<LinkId, LinkInfoInternal>.Empty,
                        StartingInfo: new
                        (
                            WorldCenter: worldCenter,
                            CameraViewHeight: cameraViewHeight,
                            StartingBuildingToCosmicBodyId: new(startingBuilding => null)
                        )
                    ),
                    worldCamera: CreateWorldCamera(worldCenter: worldCenter, cameraViewHeight: cameraViewHeight)
                );
            }

            public static (MapInfoInternal mapInfo, WorldCamera worldCamera) Create(ValidMapInfo mapInfo)
            {
                WorldCamera worldCamera = CreateWorldCamera
                (
                    worldCenter: mapInfo.StartingInfo.WorldCenter,
                    cameraViewHeight: mapInfo.StartingInfo.CameraViewHeight
                );
                var cosmicBodies = mapInfo.CosmicBodies.Select
                (
                    cosmicBodyInfo => new CosmicBodyInfoInternal
                    (
                        WorldCamera: worldCamera,
                        Id: new(),
                        Name: cosmicBodyInfo.Name,
                        Position: cosmicBodyInfo.Position,
                        Radius: cosmicBodyInfo.Radius
                    )
                ).ToList();
                var cosmicBodyNameToId = cosmicBodies.ToImmutableDictionary
                (
                    keySelector: cosmicBody => cosmicBody.Name,
                    elementSelector: cosmicBody => cosmicBody.Id
                );
                return
                (
                    mapInfo: new
                    (
                        CosmicBodies: cosmicBodies.ToImmutableDictionary(keySelector: cosmicBody => cosmicBody.Id),
                        Links: mapInfo.Links.Select
                        (
                            HUDLink => new LinkInfoInternal
                            (
                                WorldCamera: worldCamera,
                                Id: new(),
                                From: cosmicBodyNameToId[HUDLink.From],
                                To: cosmicBodyNameToId[HUDLink.To]
                            )
                        ).ToImmutableDictionary(keySelector: link => link.Id),
                        StartingInfo: new
                        (
                            WorldCenter: mapInfo.StartingInfo.WorldCenter,
                            CameraViewHeight: mapInfo.StartingInfo.CameraViewHeight,
                            StartingBuildingToCosmicBodyId: mapInfo.StartingInfo.StartingBuildingToCosmicBody.SelectValues<CosmicBodyId?>
                            (
                                cosmicBodyOrNull => cosmicBodyOrNull switch
                                {
                                    string cosmicBody => cosmicBodyNameToId[cosmicBody],
                                    null => null
                                }
                            )
                        )
                    ),
                    worldCamera: worldCamera
                );
            }

            private static WorldCamera CreateWorldCamera(MyVector2 worldCenter, Length cameraViewHeight)
                => new
                (
                    worldCenter: worldCenter,
                    worldMetersPerPixel: WorldCamera.GetWorldMetersPerPixelFromCameraViewHeight(cameraViewHeight: cameraViewHeight),
                    scrollSpeed: CurGameConfig.scrollSpeed,
                    screenBoundWidthForMapMoving: CurGameConfig.screenBoundWidthForMapMoving
                );

            public ValidMapInfo ToValidMapInfo()
            {
                // needed to make code compile
                var cosmicBodiesCopy = CosmicBodies;
                var startingInfoCopy = StartingInfo;
                return ValidMapInfo.CreateOrThrow
                (
                    notReadyToUse: true,
                    // The "to sorted dictionary" part is imporant - that ensures the same ordering of cosmic bodies and links in output json as in input json
                    cosmicBodies: CosmicBodies.ToImmutableSortedDictionary().Values.Select
                    (
                        cosmicBody => ValidCosmicBodyInfo.CreateOrThrow
                        (
                            name: cosmicBody.Name,
                            position: cosmicBody.Position,
                            radius: cosmicBody.Radius
                        )
                    ).ToEfficientReadOnlyCollection(),
                    links: Links.ToImmutableSortedDictionary().Values.Select
                    (
                        link => ValidLinkInfo.CreateOrThrow
                        (
                            from: cosmicBodiesCopy[link.From].Name,
                            to: cosmicBodiesCopy[link.To].Name
                        )
                    ).ToEfficientReadOnlyCollection(),
                    startingInfo: ValidStartingInfo.CreateOrThrow
                    (
                        worldCenter: StartingInfo.WorldCenter,
                        cameraViewHeight: StartingInfo.CameraViewHeight,
                        startingBuildingToCosmicBody: startingInfoCopy.StartingBuildingToCosmicBodyId.SelectValues
                        (
                            cosmicBodyIdOrNull => cosmicBodyIdOrNull switch
                            {
                                CosmicBodyId cosmicBodyId => cosmicBodiesCopy[cosmicBodyId].Name,
                                null => null
                            }
                        )
                    )
                );
            }
        }

        [Serializable]
        private sealed class ChangeHistory
        {
            public MapInfoInternal CurMapInfo
                => changeHistory[historyCurInd].mapInfo;
            public string CurInfoForUser
                => changeHistory[historyCurInd].infoForUser;
            private readonly List<(MapInfoInternal mapInfo, string infoForUser)> changeHistory;
            private int historyCurInd;

            public ChangeHistory(MapInfoInternal startingMapInfo)
            {
                changeHistory = new();
                historyCurInd = -1;
                LogNewChange(newMapInfo: startingMapInfo);
            }

            public void LogNewChange(MapInfoInternal newMapInfo)
            {
                ValidMapInfo? validNewMapInfo = null;
                string contentValidationMessage = "All Ok";
                try
                {
                    validNewMapInfo = newMapInfo.ToValidMapInfo();
                }
                catch (ContentException contentException)
                {
                    contentValidationMessage = contentException.Message;
                }
                string contentMissingInfoMessage = validNewMapInfo switch
                {
                    null => "N/A (fix validation errors first)",
                    not null => FullValidMapInfo.Create(mapInfo: validNewMapInfo.Value).SwitchExpression
                    (
                        ok: fullValidMapInfo => "Nothing",
                        error: errors => contentMissingInfoMessage = string.Join(";\n", errors)
                    )
                };
                changeHistory.RemoveRange(index: historyCurInd + 1, count: changeHistory.Count - historyCurInd - 1);
                changeHistory.Add
                (
                    (
                        mapInfo: newMapInfo,
                        infoForUser: $"Map validation message:\n{contentValidationMessage}\n\nMap missing info:\n{contentMissingInfoMessage}"
                    )
                );
                historyCurInd++;
            }
            
            public void Undo()
                => historyCurInd = MyMathHelper.Max(0, historyCurInd - 1);

            public void Redo()
                => historyCurInd = MyMathHelper.Min(changeHistory.Count - 1, historyCurInd + 1);
        }

        public static MapCreationState CreateNewMap(IAction switchToPauseMenu, FilePath mapPath)
            => new(switchToPauseMenu: switchToPauseMenu, mapWithCamera: MapInfoInternal.CreateEmpty(), mapPath: mapPath);

        public static MapCreationState FromMap(IAction switchToPauseMenu, ValidMapInfo mapInfo, FilePath mapPath)
            => new(switchToPauseMenu: switchToPauseMenu, mapWithCamera: MapInfoInternal.Create(mapInfo: mapInfo), mapPath: mapPath);

        public readonly FilePath mapPath;

        private MapInfoInternal CurMapInfo
            => changeHistory.CurMapInfo;
        private readonly ChangeHistory changeHistory;
        private readonly WorldCamera worldCamera;
        private readonly ActiveUIManager activeUIManager;
        private readonly AbstractButton mouseLeftButton;
        private readonly KeyButton zKey, toggleInfoKey, setCameraKey;
        private bool expandControlDescr;
        private IWorldUIElementId? selectedUIElement;
        private readonly KeyButton switchToPauseMenuButton;
        private readonly TextBox globalTextBox;
        private readonly EnumDict<StartingBuilding, (Keys key, string explanation)> startingBuildingToKeyAndExplanation;
        private readonly EnumDict<StartingBuilding, string> startingBuildingToName;
        private readonly EnumDict<StartingBuilding, TextBox> startingBuildingToTextBox;
        private readonly string controlDescrContr, controlDescrExp;

        private MapCreationState(IAction switchToPauseMenu, (MapInfoInternal mapInfo, WorldCamera worldCamera) mapWithCamera, FilePath mapPath)
        {
            this.mapPath = mapPath;
            changeHistory = new(startingMapInfo: mapWithCamera.mapInfo);
            worldCamera = mapWithCamera.worldCamera;
            activeUIManager = new();

            mouseLeftButton = new();
            zKey = new(key: Keys.Z);
            toggleInfoKey = new(key: Keys.T);
            setCameraKey = new(key: Keys.C);
            expandControlDescr = false;
            selectedUIElement = null;
            switchToPauseMenuButton = new
            (
                key: Keys.Escape,
                action: switchToPauseMenu
            );
            globalTextBox = new(backgroundColor: ActiveUIManager.colorConfig.UIBackgroundColor);
            activeUIManager.AddHUDElement(HUDElement: globalTextBox, position: new(HorizPosEnum.Left, VertPosEnum.Top));
            startingBuildingToName = new
            (
                startingBuilding => startingBuilding switch
                {
                    StartingBuilding.PowerPlant => "Power\nplant",
                    StartingBuilding.GearStorage => "Gear\nstorage",
                    StartingBuilding.WireStorage => "Wire\nstorage",
                    StartingBuilding.RoofTileStorage => "Roof tile\nstorage"
                }
            );
            startingBuildingToKeyAndExplanation = startingBuildingToName.SelectValues
            (
                (startingBuilding, buildingName) =>
                {
                    char keyLetter = startingBuilding switch
                    {
                        StartingBuilding.PowerPlant => 'P',
                        StartingBuilding.GearStorage => 'G',
                        StartingBuilding.WireStorage => 'W',
                        StartingBuilding.RoofTileStorage => 'F'
                    };
                    var keyLetterString = keyLetter.ToString();
                    Regex emphasizeRegex = new(keyLetterString, RegexOptions.IgnoreCase);
                    var emphasizedName = emphasizeRegex.Replace
                    (
                        input: buildingName.ToLower().Replace('\n', ' '),
                        replacement: $"[{char.ToLower(keyLetter)}]",
                        count: 1
                    );
                    return
                    (
                        key: Enum.Parse<Keys>(value: keyLetterString, ignoreCase: true),
                        explanation: $"select body + {keyLetter} - Select starting {emphasizedName} location"
                    );
                }
            );
            startingBuildingToTextBox = new
            (
                startingBuilding =>
                {
                    TextBox textBox = new();
                    activeUIManager.AddWorldHUDElement
                    (
                        worldHUDElement: textBox,
                        updateHUDPos: new BuildingCosmicBodyTextBoxHUDPosUpdater(MapCreationState: this, StartingBuilding: startingBuilding)
                    );
                    return textBox;
                }
            );

            controlDescrContr = """
                Esc - Go to pause menu
                T - [T]oggle control info
                """;
            controlDescrExp = $"""
                Esc - Go to pause menu
                T - [T]oggle control info
                left click on body/link - Select cosmic body/link
                N + left click - [N]ew cosmic body
                select body + D - [D]elete cosmic body
                {string.Join('\n', startingBuildingToKeyAndExplanation.Values.Select(keyAndExplanation => keyAndExplanation.explanation))}
                select body + R + left click - Change cosmic body [r]adius
                select body + M + left click - [M]ove cosmic body
                select body + L + left click other body - Add [l]ink bewteen the two cosmic bodies
                select link + D - [D]elete link
                C - Set starting [c]amera to be current camera
                ctrl + Z - Undo
                ctrl + shift + Z - Redo
                I - Zoom [i]n
                O - Zoom [o]ut
                bump mouse to the screen edge - Move camera
                """;
        }

        // Can't use $"Cosmic body {Id}" straight up, as when loading initial map from the file, that mapName may be already taken
        private string GetNewCosmicBodyName()
            => Algorithms.GanerateNewName
            (
                prefix: "Cosmic body",
                usedNames: CurMapInfo.CosmicBodies.Values.Select(cosmicBody => cosmicBody.Name).ToEfficientReadOnlyHashSet()
            );

        public sealed override void Update(TimeSpan elapsed)
        {
            worldCamera.Update(elapsed: elapsed, canScroll: true);
            activeUIManager.Update(elapsed: elapsed);

            var newMapInfo = HandleUserInput();
            if (newMapInfo is not null)
                changeHistory.LogNewChange(newMapInfo: newMapInfo.Value);

            globalTextBox.Text = expandControlDescr ? controlDescrExp : controlDescrContr + "\n\n" + changeHistory.CurInfoForUser;
            foreach (var startingBuilding in Enum.GetValues<StartingBuilding>())
                startingBuildingToTextBox[startingBuilding].Text = CurMapInfo.StartingInfo.StartingBuildingToCosmicBodyId[startingBuilding] is null ? null : startingBuildingToName[startingBuilding];
        }

        private Vector2Bare CurCosmicBodyHUDPos(CosmicBodyId cosmicBodyId)
            => ActiveUIManager.ScreenPosToHUDPos
            (
                screenPos: worldCamera.WorldPosToScreenPos
                (
                    worldPos: CurMapInfo.CosmicBodies[cosmicBodyId].Position
                )
            );

        private MapInfoInternal? HandleUserInput()
        {
            switchToPauseMenuButton.Update();
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            mouseLeftButton.Update(down: mouseState.LeftButton == ButtonState.Pressed);
            Vector2Bare mouseScreenPos = (Vector2Bare)mouseState.Position;
            MyVector2 mouseWorldPos = worldCamera.ScreenPosToWorldPos(screenPos: mouseScreenPos);

            IWorldUIElementId? hoverUIElement = null;
            foreach (var (id, shape, _) in GetCurWorldUIElements())
                if (shape.Contains(mouseScreenPos))
                {
                    hoverUIElement = id;
                    break;
                }
            toggleInfoKey.Update();
            if (toggleInfoKey.HalfClicked)
                expandControlDescr = !expandControlDescr;
            // New cosmic body
            if (mouseLeftButton.Clicked && keyboardState.IsKeyDown(Keys.N))
            {
                CosmicBodyId newCosmicBodyId = new();
                selectedUIElement = newCosmicBodyId;
                return CurMapInfo with
                {
                    CosmicBodies = CurMapInfo.CosmicBodies.Add
                    (
                        key: newCosmicBodyId,
                        value: new CosmicBodyInfoInternal
                        (
                            WorldCamera: worldCamera,
                            Id: newCosmicBodyId,
                            Name: GetNewCosmicBodyName(),
                            Position: mouseWorldPos,
                            Radius: Length.CreateFromM(valueInM: 100)
                        )
                    )
                };
            }
            if (selectedUIElement is CosmicBodyId selectedCosmicBodyId)
            {
                // Delete selected cosmic body
                if (keyboardState.IsKeyDown(Keys.D))
                {
                    selectedUIElement = null;
                    return new()
                    {
                        CosmicBodies = CurMapInfo.CosmicBodies.Remove(key: selectedCosmicBodyId),
                        Links = CurMapInfo.Links.Where(link => link.Value.From != selectedCosmicBodyId && link.Value.To != selectedCosmicBodyId).ToImmutableDictionary
                        (
                            keySelector: keyValue => keyValue.Key,
                            elementSelector: keyValue => keyValue.Value
                        ),
                        StartingInfo = CurMapInfo.StartingInfo with
                        {
                            StartingBuildingToCosmicBodyId = RemoveBuildingFromSelectedCosmicBody()
                        }
                    };
                }
                // Starting building location selection
                foreach (var (startingBuilding, (key, _)) in startingBuildingToKeyAndExplanation)
                    if (keyboardState.IsKeyDown(key))
                        return CurMapInfo with
                        {
                            StartingInfo = CurMapInfo.StartingInfo with
                            {
                                StartingBuildingToCosmicBodyId = RemoveBuildingFromSelectedCosmicBody().Update
                                (
                                    key: startingBuilding,
                                    newValue: selectedCosmicBodyId
                                )
                            }
                        };

                EnumDict<StartingBuilding, CosmicBodyId?> RemoveBuildingFromSelectedCosmicBody()
                    => CurMapInfo.StartingInfo.StartingBuildingToCosmicBodyId.SelectValues
                    (
                        buildingCosmicBodyId => buildingCosmicBodyId == selectedCosmicBodyId ? null : buildingCosmicBodyId
                    );
                // radius change
                if (mouseLeftButton.Clicked && keyboardState.IsKeyDown(Keys.R))
                    return CurMapInfo with
                    {
                        CosmicBodies = CurMapInfo.CosmicBodies.SetItem
                        (
                            key: selectedCosmicBodyId,
                            value: CurMapInfo.CosmicBodies[selectedCosmicBodyId] with
                            {
                                Radius = MyMathHelper.Max
                                (
                                    Length.CreateFromM(valueInM: CurGameConfig.minPlanetPixelRadius),
                                    MyVector2.Distance
                                    (
                                        value1: mouseWorldPos,
                                        value2: CurMapInfo.CosmicBodies[selectedCosmicBodyId].Position
                                    )
                                )
                            }
                        )
                    };
                // Move
                if (mouseLeftButton.Clicked && keyboardState.IsKeyDown(Keys.M))
                    return CurMapInfo with
                    {
                        CosmicBodies = CurMapInfo.CosmicBodies.SetItem
                        (
                            key: selectedCosmicBodyId,
                            value: CurMapInfo.CosmicBodies[selectedCosmicBodyId] with
                            {
                                Position = mouseWorldPos
                            }
                        )
                    };
                // Link add
                if (mouseLeftButton.Clicked && keyboardState.IsKeyDown(Keys.L) && hoverUIElement is CosmicBodyId hoverCosmicBodyId && selectedCosmicBodyId != hoverCosmicBodyId)
                {
                    LinkInfoInternal newLink = new
                    (
                        WorldCamera: worldCamera,
                        Id: new(),
                        From: selectedCosmicBodyId,
                        To: hoverCosmicBodyId
                    );
                    // TODO: these checks should probably be performed by the mapInfo immediate validator, and a message explaining why the link can't be added should be shown
                    if (CurMapInfo.Links.Values.All(link => (newLink.From, newLink.To) != (link.From, link.To) && (newLink.To, newLink.From) != (link.From, link.To)))
                        return CurMapInfo with
                        {
                            Links = CurMapInfo.Links.Add(key: newLink.Id, value: newLink)
                        };
                }
            }
            // Delete selected link
            if (keyboardState.IsKeyDown(Keys.D) && selectedUIElement is LinkId linkId)
            {
                selectedUIElement = null;
                return CurMapInfo with
                {
                    Links = CurMapInfo.Links.Remove(key: linkId)
                };
            }
            // Select/deselect planet/link
            if (mouseLeftButton.Clicked)
            {
                selectedUIElement = hoverUIElement;
                return null;
            }
            setCameraKey.Update();
            // Set the starting camera to be current camera
            if (setCameraKey.HalfClicked)
                return CurMapInfo with
                {
                    StartingInfo = CurMapInfo.StartingInfo with
                    {
                        WorldCenter = worldCamera.WorldCenter,
                        CameraViewHeight = worldCamera.CameraViewHeight
                    }
                };
            zKey.Update();
            bool controlDown = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl),
                shiftDown = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            // Redo
            if (controlDown && shiftDown && zKey.HalfClicked)
            {
                CenterCameraAfterActionIfNeeded(action: changeHistory.Redo);
                return null;
            }
            // Undo
            if (controlDown && zKey.HalfClicked)
            {
                CenterCameraAfterActionIfNeeded(action: changeHistory.Undo);
                return null;
            }
            return null;

            void CenterCameraAfterActionIfNeeded(Action action)
            {
                MyVector2 prevWorldCenter = CurMapInfo.StartingInfo.WorldCenter;
                Length prevCameraViewHeight = CurMapInfo.StartingInfo.CameraViewHeight;
                action();
                if (prevWorldCenter != CurMapInfo.StartingInfo.WorldCenter || prevCameraViewHeight != CurMapInfo.StartingInfo.CameraViewHeight)
                    worldCamera.MoveTo
                    (
                        worldCenter: CurMapInfo.StartingInfo.WorldCenter,
                        worldMetersPerPixel: WorldCamera.GetWorldMetersPerPixelFromCameraViewHeight
                        (
                            cameraViewHeight: CurMapInfo.StartingInfo.CameraViewHeight
                        )
                    );
            }
        }

        private IEnumerable<(IWorldUIElementId id, Shape shape, Color color)> GetCurWorldUIElements()
        {
            foreach (var (id, cosmicBody) in CurMapInfo.CosmicBodies)
                yield return
                (
                    id: id,
                    shape: cosmicBody.GetShape(),
                    color: (selectedUIElement is CosmicBodyId selectedCosmicBody && selectedCosmicBody == id)
                        ? ActiveUIManager.colorConfig.mapCreationSelectedWorldUIElementColor
                        : ActiveUIManager.colorConfig.Res0Color
                );
            foreach (var (id, link) in CurMapInfo.Links)
                yield return
                (
                    id: id,
                    shape: link.GetShape(curMapInfo: CurMapInfo),
                    color: (selectedUIElement is LinkId selectedLink && selectedLink == id)
                        ? ActiveUIManager.colorConfig.mapCreationSelectedWorldUIElementColor
                        : ActiveUIManager.colorConfig.costlyLinkColor
                );
        }

        public sealed override void Draw()
        {
            C.GraphicsDevice.Clear(C.ColorFromRGB(rgb: 0x00035b));

            worldCamera.BeginDraw();

            // Draw starting camera shape
            new MyRectangle
            (
                width: WorldCamera.CameraViewWidthFromHeight(cameraViewHeight: CurMapInfo.StartingInfo.CameraViewHeight).valueInM,
                height: CurMapInfo.StartingInfo.CameraViewHeight.valueInM
            )
            {
                Center = new Vector2Bare(x: CurMapInfo.StartingInfo.WorldCenter.X.valueInM, y: CurMapInfo.StartingInfo.WorldCenter.Y.valueInM),
            }.Draw(color: ActiveUIManager.colorConfig.cosmosBackgroundColor);
            
            foreach (var (_, shape, color) in GetCurWorldUIElements().Reverse())
                shape.Draw(color: color);
            worldCamera.EndDraw();

            activeUIManager.DrawHUD();
        }

        public ValidMapInfo CurrentMap()
            => CurMapInfo.ToValidMapInfo();
    }
}
