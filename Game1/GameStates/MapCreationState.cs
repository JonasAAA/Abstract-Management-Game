using Game1.Collections;
using Game1.ContentHelpers;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using System.Collections.Immutable;
using System.Text.RegularExpressions;

namespace Game1.GameStates
{
    [Serializable]
    public sealed class MapCreationState : GameState
    {
        private enum StartingBuilding
        {
            PowerPlant,
            GearStorage,
            WireStorage,
            RoofTileStorage
        }

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

        [Serializable]
        private readonly record struct CosmicBodyId : IWorldUIElementId
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
        }

        [Serializable]
        private sealed class LinkId : IWorldUIElementId
        {
            private static uint nextId = 0;

            private readonly uint id;

            public LinkId()
            {
                id = nextId;
                nextId++;
            }

            public sealed override string ToString()
                => id.ToString();
        }

        [Serializable]
        private readonly record struct CosmicBodyInfoInternal(CosmicBodyId Id, string Name, MyVector2 Position, UDouble Radius)
        {
            [Serializable]
            private readonly record struct CosmicBodyShapeParams(CosmicBodyInfoInternal CosmicBody) : Disk.IParams
            {
                public MyVector2 Center
                    => CosmicBody.Position;

                public UDouble Radius
                    => CosmicBody.Radius;
            }

            public Shape GetShape()
                => new Disk
                (
                    parameters: new CosmicBodyShapeParams(CosmicBody: this)
                );
        }

        [Serializable]
        private readonly record struct LinkInfoInternal(LinkId Id, CosmicBodyId From, CosmicBodyId To)
        {
            [Serializable]
            private readonly record struct LinkShapeParams(MapInfoInternal CurMapInfo, LinkInfoInternal Link) : VectorShape.IParams
            {
                public MyVector2 StartPos
                    => CurMapInfo.CosmicBodies[Link.From].Position;

                public MyVector2 EndPos
                    => CurMapInfo.CosmicBodies[Link.To].Position;

                public UDouble Width
                    // TODO: put this into some config
                    => 20;
            }

            public Shape GetShape(MapInfoInternal curMapInfo)
                => new LineSegment
                (
                    parameters: new LinkShapeParams(CurMapInfo: curMapInfo, Link: this)
                );
        }

        [Serializable]
        private readonly record struct StartingInfoInternal(MyVector2 WorldCenter, UDouble CameraViewHeight, EnumDict<StartingBuilding, CosmicBodyId?> StartingBuildingToCosmicBodyId);

        [Serializable]
        private record struct MapInfoInternal(ImmutableDictionary<CosmicBodyId, CosmicBodyInfoInternal> CosmicBodies, ImmutableDictionary<LinkId, LinkInfoInternal> Links, StartingInfoInternal StartingInfo)
        {
            public static MapInfoInternal CreateEmpty()
                => new
                (
                    CosmicBodies: ImmutableDictionary<CosmicBodyId, CosmicBodyInfoInternal>.Empty,
                    Links: ImmutableDictionary<LinkId, LinkInfoInternal>.Empty,
                    StartingInfo: new
                    (
                        WorldCenter: MyVector2.zero,
                        CameraViewHeight: ActiveUIManager.curUIConfig.standardScreenHeight,
                        StartingBuildingToCosmicBodyId: new(startingBuilding => null)
                    )
                );

            public static MapInfoInternal Create(ValidMapInfo mapInfo)
            {
                var cosmicBodies = mapInfo.CosmicBodies.Select
                (
                    cosmicBodyInfo => new CosmicBodyInfoInternal
                    (
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
                return new
                (
                    CosmicBodies: cosmicBodies.ToImmutableDictionary(keySelector: cosmicBody => cosmicBody.Id),
                    Links: mapInfo.Links.Select
                    (
                        HUDLink => new LinkInfoInternal
                        (
                            Id: new(),
                            From: cosmicBodyNameToId[HUDLink.From],
                            To: cosmicBodyNameToId[HUDLink.To]
                        )
                    ).ToImmutableDictionary(keySelector: link => link.Id),
                    StartingInfo: new
                    (
                        WorldCenter: mapInfo.StartingInfo.WorldCenter,
                        CameraViewHeight: mapInfo.StartingInfo.CameraViewHeight,
                        StartingBuildingToCosmicBodyId: new
                        (
                            startingBuilding => startingBuilding switch
                            {
                                StartingBuilding.PowerPlant => GetCosmicBodyId(cosmicBodyName: mapInfo.StartingInfo.PowerPlantCosmicBody),
                                StartingBuilding.GearStorage => GetCosmicBodyId(cosmicBodyName: mapInfo.StartingInfo.GearStorageCosmicBody),
                                StartingBuilding.WireStorage => GetCosmicBodyId(cosmicBodyName: mapInfo.StartingInfo.WireStorageCosmicBody),
                                StartingBuilding.RoofTileStorage => GetCosmicBodyId(cosmicBodyName: mapInfo.StartingInfo.RoofTileStorageCosmicBody)
                            } 
                        )
                    )
                );

                CosmicBodyId? GetCosmicBodyId(string? cosmicBodyName)
                    => GetValueOrNullFromKeyClass(dict: cosmicBodyNameToId, key: cosmicBodyName);
            }

            public ValidMapInfo ToValidMapInfo()
            {
                // needed to make code compile
                var cosmicBodiesCopy = CosmicBodies;
                var startingInfoCopy = StartingInfo;
                return ValidMapInfo.CreateOrThrow
                (
                    notReadyToUse: true,
                    cosmicBodies: CosmicBodies.Values.Select
                    (
                        cosmicBody => ValidCosmicBodyInfo.CreateOrThrow
                        (
                            name: cosmicBody.Name,
                            position: cosmicBody.Position,
                            radius: cosmicBody.Radius
                        )
                    ).ToEfficientReadOnlyCollection(),
                    links: Links.Values.Select
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
                        powerPlantCosmicBody: GetCosmicBodyName(startingBuilding: StartingBuilding.PowerPlant),
                        gearStorageCosmicBody: GetCosmicBodyName(startingBuilding: StartingBuilding.GearStorage),
                        wireStorageCosmicBody: GetCosmicBodyName(startingBuilding: StartingBuilding.WireStorage),
                        roofTileStorageCosmicBody: GetCosmicBodyName(startingBuilding: StartingBuilding.RoofTileStorage)
                    )
                );

                string? GetCosmicBodyName(StartingBuilding startingBuilding)
                    => GetStructValueOrNullFromKeyStruct(dict: cosmicBodiesCopy, key: startingInfoCopy.StartingBuildingToCosmicBodyId[startingBuilding])?.Name;
            }

            private static TValue? GetValueOrNullFromKeyClass<TKey, TValue>(ImmutableDictionary<TKey, TValue> dict, TKey? key)
                where TKey : class
                where TValue : struct
                => key switch
                {
                    TKey notNullKey => dict[notNullKey],
                    null => null
                };

            private static TValue? GetStructValueOrNullFromKeyStruct<TKey, TValue>(ImmutableDictionary<TKey, TValue> dict, TKey? key)
                where TKey : struct
                where TValue : struct
                => key switch
                {
                    TKey notNullKey => dict[notNullKey],
                    null => null
                };
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

        public static MapCreationState CreateNewMap(IAction switchToPauseMenu, string mapName)
            => new(switchToPauseMenu: switchToPauseMenu, mapInfo: MapInfoInternal.CreateEmpty(), mapName: mapName);

        public static MapCreationState FromMap(IAction switchToPauseMenu, ValidMapInfo mapInfo, string mapName)
            => new(switchToPauseMenu: switchToPauseMenu, mapInfo: MapInfoInternal.Create(mapInfo: mapInfo), mapName: mapName);

        public readonly string mapName;

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

        private MapCreationState(IAction switchToPauseMenu, MapInfoInternal mapInfo, string mapName)
        {
            this.mapName = mapName;
            changeHistory = new(startingMapInfo: mapInfo);
            // TODO: move the constants to config file
            worldCamera = new
            (
                worldCenter: CurMapInfo.StartingInfo.WorldCenter,
                startingWorldScale: WorldCamera.GetWorldScaleFromCameraViewHeight(cameraViewHeight: CurMapInfo.StartingInfo.CameraViewHeight),
                scrollSpeed: 60,
                screenBoundWidthForMapMoving: 10
            );
            activeUIManager = new(worldCamera: worldCamera);

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
            startingBuildingToKeyAndExplanation = new
            (
                startingBuilding =>
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
                        input: startingBuildingToName[startingBuilding].ToLower().Replace('\n', ' '),
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

        private MyVector2 CurCosmicBodyHUDPos(CosmicBodyId cosmicBodyId)
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
            MyVector2 mouseWorldPos = worldCamera.ScreenPosToWorldPos
            (
                screenPos: new
                (
                    x: mouseState.Position.X,
                    y: mouseState.Position.Y
                )
            );

            IWorldUIElementId? hoverUIElement = null;
            foreach (var (id, shape, _) in GetCurWorldUIElements())
                if (shape.Contains(mouseWorldPos))
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
                            Id: newCosmicBodyId,
                            Name: GetNewCosmicBodyName(),
                            Position: mouseWorldPos,
                            Radius: 100
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
                    => new
                    (
                        startingBuilding =>
                        {
                            var buildingCosmicBodyId = CurMapInfo.StartingInfo.StartingBuildingToCosmicBodyId[startingBuilding];
                            return buildingCosmicBodyId == selectedCosmicBodyId ? null : buildingCosmicBodyId;
                        }
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
                                Radius = MyVector2.Distance
                                (
                                    value1: mouseWorldPos,
                                    value2: CurMapInfo.CosmicBodies[selectedCosmicBodyId].Position
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
                UDouble prevCameraViewHeight = CurMapInfo.StartingInfo.CameraViewHeight;
                action();
                if (prevWorldCenter != CurMapInfo.StartingInfo.WorldCenter || prevCameraViewHeight != CurMapInfo.StartingInfo.CameraViewHeight)
                    worldCamera.MoveTo
                    (
                        worldCenter: CurMapInfo.StartingInfo.WorldCenter,
                        worldScale: WorldCamera.GetWorldScaleFromCameraViewHeight
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
                width: WorldCamera.CameraViewWidthFromHeight(cameraViewHeight: CurMapInfo.StartingInfo.CameraViewHeight),
                height: CurMapInfo.StartingInfo.CameraViewHeight
            )
            {
                Center = CurMapInfo.StartingInfo.WorldCenter,
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
