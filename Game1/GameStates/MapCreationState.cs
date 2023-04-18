using Game1.ContentHelpers;
using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;
using System.Collections.Immutable;

namespace Game1.GameStates
{
    [Serializable]
    public sealed class MapCreationState : GameState
    {
        private interface IWorldUIElementId
        { }

        [Serializable]
        private readonly record struct CosmicBodyId : IWorldUIElementId
        {
            static uint nextId = 0;

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
        private class LinkId : IWorldUIElementId
        {
            static uint nextId = 0;

            private readonly uint id;

            public LinkId()
            {
                id = nextId;
                nextId++;
            }

            public override string ToString()
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
        private readonly record struct StartingInfoInternal(CosmicBodyId? HouseCosmicBodyId, CosmicBodyId? PowerPlantCosmicBodyId, MyVector2 WorldCenter, UDouble CameraViewHeight);

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
                        HouseCosmicBodyId: null,
                        PowerPlantCosmicBodyId: null,
                        WorldCenter: MyVector2.zero,
                        CameraViewHeight: 1080
                    )
                );

            public static MapInfoInternal Create(ValidMapInfo mapInfo)
            {
                List<CosmicBodyInfoInternal> cosmicBodies = mapInfo.CosmicBodies.Select
                (
                    cosmicBodyInfo => new CosmicBodyInfoInternal
                    (
                        Id: new(),
                        Name: cosmicBodyInfo.Name,
                        Position: cosmicBodyInfo.Position,
                        Radius: cosmicBodyInfo.Radius
                    )
                ).ToList();
                ImmutableDictionary<string, CosmicBodyId> cosmicBodyNameToId = cosmicBodies.ToImmutableDictionary
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
                        HouseCosmicBodyId: mapInfo.StartingInfo.HouseCosmicBody is null ? null : cosmicBodyNameToId[mapInfo.StartingInfo.HouseCosmicBody],
                        PowerPlantCosmicBodyId: mapInfo.StartingInfo.PowerPlantCosmicBody is null ? null : cosmicBodyNameToId[mapInfo.StartingInfo.PowerPlantCosmicBody],
                        WorldCenter: mapInfo.StartingInfo.WorldCenter,
                        CameraViewHeight: mapInfo.StartingInfo.CameraViewHeight
                    )
                );
            }

            public ValidMapInfo ToValidMapInfo()
            {
                // needed to make code compile
                var cosmicBodiesCopy = CosmicBodies;
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
                    ).ToArray(),
                    links: Links.Values.Select
                    (
                        link => ValidLinkInfo.CreateOrThrow
                        (
                            from: cosmicBodiesCopy[link.From].Name,
                            to: cosmicBodiesCopy[link.To].Name
                        )
                    ).ToArray(),
                    startingInfo: ValidStartingInfo.CreateOrThrow
                    (
                        houseCosmicBodyName: StartingInfo.HouseCosmicBodyId is null ? null : cosmicBodiesCopy[StartingInfo.HouseCosmicBodyId.Value].Name,
                        powerPlantCosmicBodyName: StartingInfo.PowerPlantCosmicBodyId is null ? null : cosmicBodiesCopy[StartingInfo.PowerPlantCosmicBodyId.Value].Name,
                        worldCenter: StartingInfo.WorldCenter,
                        cameraViewHeight: StartingInfo.CameraViewHeight
                    )
                );
            }
        }

        [Serializable]
        private class ChangeHistory
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
        private bool prevLeftDown, prevZDown;
        private IWorldUIElementId? selectedUIElement;
        private readonly KeyButton switchToPauseMenuButton;
        private readonly TextBox globalTextBox, houseTextBox, powerPlantTextBox;
        private readonly string controlDescription;

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

            prevLeftDown = false;
            prevZDown = false;
            selectedUIElement = null;
            switchToPauseMenuButton = new
            (
                key: Keys.Escape,
                action: switchToPauseMenu
            );
            globalTextBox = new(backgroundColor: ActiveUIManager.colorConfig.UIBackgroundColor);
            activeUIManager.AddHUDElement(HUDElement: globalTextBox, horizPos: HorizPos.Left, vertPos: VertPos.Top);
            houseTextBox = new();
            activeUIManager.AddWorldHUDElement(worldHUDElement: houseTextBox);
            powerPlantTextBox = new();
            activeUIManager.AddWorldHUDElement(worldHUDElement: powerPlantTextBox);

            controlDescription = """
                Esc - go to pause menu
                left click on body/link - select cosmic body/link
                N + left click - [N]ew cosmic body
                select body + D - [D]elete cosmic body
                select body + H - Select starting [h]ouse location
                select body + P - Select starting [p]ower plant location
                select body + R + left click - Change cosmic body [r]adius
                select body + M + left click - [M]ove cosmic body
                select body + L + left click other body - Add [l]ink bewteen the two cosmic bodies
                select link + D - [D]elete link
                ctrl + Z - Undo
                ctrl + shift + Z - Redo
                I - zoom [i]n
                O - zoom [o]ut
                bump mouse to the screen edge - move camera
                """;
        }

        // Can't use $"Cosmic body {Id}" straight up, as when loading initial map from the file, that mapName may be already taken
        private string GetNewCosmicBodyName()
        {
            HashSet<string> curCosmicBodyNames = new(CurMapInfo.CosmicBodies.Values.Select(cosmicBody => cosmicBody.Name));
            for (int i = 0; ; i++)
            {
                string newName = $"Cosmic body {i}";
                if (!curCosmicBodyNames.Contains(newName))
                    return newName;
            }
        }

        public override void Update(TimeSpan elapsed)
        {
            worldCamera.Update(elapsed: elapsed, canScroll: true);
            
            var newMapInfo = HandleUserInput();
            if (newMapInfo is not null)
                changeHistory.LogNewChange(newMapInfo: newMapInfo.Value);
            
            globalTextBox.Text = controlDescription + "\n\n" + changeHistory.CurInfoForUser;
            if (CurMapInfo.StartingInfo.HouseCosmicBodyId is CosmicBodyId houseCosmicBodyId)
            {
                houseTextBox.Text = "House";
                houseTextBox.Shape.Center = CurCosmicBodyHUDPos(cosmicBodyId: houseCosmicBodyId);
            }
            else
                houseTextBox.Text = null;

            if (CurMapInfo.StartingInfo.PowerPlantCosmicBodyId is CosmicBodyId powerPlantCosmicBodyId)
            {
                powerPlantTextBox.Text = "Power\nplant";
                powerPlantTextBox.Shape.Center = CurCosmicBodyHUDPos(cosmicBodyId: powerPlantCosmicBodyId);
            }
            else
                powerPlantTextBox.Text = null;

            MyVector2 CurCosmicBodyHUDPos(CosmicBodyId cosmicBodyId)
                => ActiveUIManager.ScreenPosToHUDPos
                (
                    screenPos: worldCamera.WorldPosToScreenPos
                    (
                        worldPos: CurMapInfo.CosmicBodies[cosmicBodyId].Position
                    )
                );
        }

        private MapInfoInternal? HandleUserInput()
        {
            switchToPauseMenuButton.Update();
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            bool leftDown = mouseState.LeftButton == ButtonState.Pressed;
            MyVector2 mouseWorldPos = worldCamera.ScreenPosToWorldPos
            (
                screenPos: new
                (
                    x: mouseState.Position.X,
                    y: mouseState.Position.Y
                )
            );
            bool leftClicked = leftDown && !prevLeftDown;
            prevLeftDown = leftDown;

            IWorldUIElementId? hoverUIElement = null;
            foreach (var (id, shape, _) in GetCurWorldUIElements())
                if (shape.Contains(mouseWorldPos))
                {
                    hoverUIElement = id;
                    break;
                }

            // New cosmic body
            if (leftClicked && keyboardState.IsKeyDown(Keys.N))
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
                            HouseCosmicBodyId = CurMapInfo.StartingInfo.HouseCosmicBodyId == selectedCosmicBodyId ? null : CurMapInfo.StartingInfo.HouseCosmicBodyId,
                            PowerPlantCosmicBodyId = CurMapInfo.StartingInfo.PowerPlantCosmicBodyId == selectedCosmicBodyId ? null : CurMapInfo.StartingInfo.PowerPlantCosmicBodyId
                        }
                    };
                }
                // Starting house location select
                if (keyboardState.IsKeyDown(Keys.H))
                    return CurMapInfo with
                    {
                        StartingInfo = CurMapInfo.StartingInfo with
                        {
                            HouseCosmicBodyId = selectedCosmicBodyId,
                            PowerPlantCosmicBodyId = CurMapInfo.StartingInfo.PowerPlantCosmicBodyId == selectedCosmicBodyId ? null : CurMapInfo.StartingInfo.PowerPlantCosmicBodyId
                        }
                    };
                // Starting power plant location selection
                if (keyboardState.IsKeyDown(Keys.P))
                    return CurMapInfo with
                    {
                        StartingInfo = CurMapInfo.StartingInfo with
                        {
                            HouseCosmicBodyId = CurMapInfo.StartingInfo.HouseCosmicBodyId == selectedCosmicBodyId ? null : CurMapInfo.StartingInfo.HouseCosmicBodyId,
                            PowerPlantCosmicBodyId = selectedCosmicBodyId
                        }
                    };
                // Radius change
                if (leftClicked && keyboardState.IsKeyDown(Keys.R))
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
                if (leftClicked && keyboardState.IsKeyDown(Keys.M))
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
                if (leftClicked && keyboardState.IsKeyDown(Keys.L) && hoverUIElement is CosmicBodyId hoverCosmicBodyId && selectedCosmicBodyId != hoverCosmicBodyId)
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
            if (leftClicked)
            {
                selectedUIElement = hoverUIElement;
                return null;
            }
            bool zDown = keyboardState.IsKeyDown(Keys.Z),
                zClicked = zDown && !prevZDown,
                controlDown = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl),
                shiftDown = keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift);
            prevZDown = zDown;
            if (controlDown && shiftDown && zClicked)
            {
                changeHistory.Redo();
                return null;
            }
            if (controlDown && zClicked)
            {
                changeHistory.Undo();
                return null;
            }
            return null;

            //ImmutableDictionary<TKey, TValue> FilterDictByValue<TKey, TValue>(ImmutableDictionary<TKey, TValue> dict, Func<TValue, bool> predicate)
            //    where TKey : notnull
            //    => dict.Where(keyValue => predicate(keyValue.Value)).ToImmutableDictionary
            //    (
            //        keySelector: keyValue => keyValue.Key,
            //        elementSelector: keyValue => keyValue.Value
            //    );
        }

        private IEnumerable<(IWorldUIElementId id, Shape shape, Color color)> GetCurWorldUIElements()
        {
            foreach (var (id, cosmicBody) in CurMapInfo.CosmicBodies)
                yield return
                (
                    id: id,
                    shape: cosmicBody.GetShape(),
                    color: (selectedUIElement is CosmicBodyId selectedCosmicBody && selectedCosmicBody == id)
                        ? ActiveUIManager.colorConfig.selectedWorldUIElementColor
                        : ActiveUIManager.colorConfig.Res0Color
                );
            foreach (var (id, link) in CurMapInfo.Links)
                yield return
                (
                    id: id,
                    shape: link.GetShape(curMapInfo: CurMapInfo),
                    color: (selectedUIElement is LinkId selectedLink && selectedLink == id)
                        ? ActiveUIManager.colorConfig.selectedWorldUIElementColor
                        : ActiveUIManager.colorConfig.linkColor
                );
        }

        public override void Draw()
        {
            C.GraphicsDevice.Clear(ActiveUIManager.colorConfig.cosmosBackgroundColor);
            
            worldCamera.BeginDraw();
            foreach (var (_, shape, color) in GetCurWorldUIElements().Reverse())
                shape.Draw(color: color);
            worldCamera.EndDraw();

            activeUIManager.DrawHUD();
        }

        public ValidMapInfo CurrentMap()
            => CurMapInfo.ToValidMapInfo();
    }
}
