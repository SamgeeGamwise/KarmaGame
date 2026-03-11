using System;
using System.Collections.Generic;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sandbox.Game.Config;
using Sandbox.Game.Scene;
using Sandbox.Game.Scene.Buildings;
using Sandbox.Game.Scene.Npc;
using Sandbox.Game.Scene.Progression;
using Sandbox.Game.Scene.UI;

namespace Sandbox.Game;

internal sealed class SandboxScene
{
    private readonly SceneSettings _sceneSettings;
    private readonly InteractionSettings _interactionSettings;
    private readonly MenuSettings _menuSettings;
    private readonly EconomySettings _economySettings;
    private readonly DebugSettings _debugSettings;
    private readonly SleepSettings _sleepSettings;
    private readonly TiledMapAuthoringProfile _mapProfile;
    private readonly Dictionary<string, MapNode> _maps = new(StringComparer.Ordinal);
    private readonly PlayerNode _playerNode;
    private readonly CameraNode _cameraNode;
    private readonly DayNightNode _dayNightNode;
    private readonly NpcRosterNode _npcRosterNode;
    private readonly MenuOverlayNode _menuOverlayNode;
    private readonly DialogueNode _dialogueNode = new();
    private readonly NotificationFeedNode _notificationFeedNode;
    private readonly HudNode _hudNode;
    private readonly IReadOnlyDictionary<string, string> _keysByAction;
    private readonly PlayerProgressState _progressState;
    private readonly BuildingDirectory _buildingDirectory;
    private readonly YSortRenderer _ySortRenderer = new();
    private readonly ScenePortal[] _portals;

    private string _activeMapAssetName;
    private float _portalCooldownSeconds;
    private bool _isPortalDebugOverlayEnabled;
    private Texture2D _debugPixel = null!;

    public SandboxScene(SandboxGameSettings settings, TiledMapAuthoringProfile mapProfile)
    {
        _sceneSettings = settings.Scene;
        _interactionSettings = settings.Interaction;
        _menuSettings = settings.Menu;
        _economySettings = settings.Economy;
        _debugSettings = settings.Debug;
        _sleepSettings = settings.Sleep;
        _mapProfile = mapProfile;
        _cameraNode = new CameraNode(settings.Camera, _sceneSettings.CameraZoom);
        _activeMapAssetName = _sceneSettings.StartingMapAssetName;
        _isPortalDebugOverlayEnabled = settings.Debug.StartWithDebugLinesOn;
        _portals = BuildPortals(_sceneSettings);
        _playerNode = new PlayerNode(settings.Player);
        _dayNightNode = new DayNightNode(settings.DayNight);
        _npcRosterNode = new NpcRosterNode(settings.Npcs, settings.Interaction);
        _keysByAction = BuildInputKeyMap(settings.Input);
        _menuOverlayNode = new MenuOverlayNode(settings.Menu, _keysByAction);
        _notificationFeedNode = new NotificationFeedNode(settings.Interaction.NotificationDurationSeconds);
        _hudNode = new HudNode(
            _keysByAction,
            _menuSettings.ToggleInputActionName,
            _sceneSettings.ActionInputActionName,
            _debugSettings.AddMoneyInputActionName);
        _progressState = PlayerProgressState.Create(settings.Progression, settings.Economy);
        _buildingDirectory = new BuildingDirectory(_sceneSettings.Buildings);
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        EnsureMapRegistered(_activeMapAssetName);
        foreach (ScenePortal portal in _portals)
        {
            EnsureMapRegistered(portal.SourceMapAssetName);
            EnsureMapRegistered(portal.TargetMapAssetName);
        }

        foreach (BuildingSettings building in _buildingDirectory.Buildings)
        {
            EnsureMapRegistered(building.ExteriorMapAssetName);
            EnsureMapRegistered(building.InteriorMapAssetName);
        }

        foreach (MapNode mapNode in _maps.Values)
            mapNode.LoadContent(content, graphicsDevice);

        _playerNode.LoadContent(content);
        _dayNightNode.LoadContent(content, graphicsDevice);
        _npcRosterNode.LoadContent(content, _maps);
        _dialogueNode.LoadContent(content, graphicsDevice);
        _menuOverlayNode.LoadContent(content, graphicsDevice);
        _notificationFeedNode.LoadContent(content, graphicsDevice);
        _hudNode.LoadContent(content, graphicsDevice);

        _debugPixel = new Texture2D(graphicsDevice, 1, 1);
        _debugPixel.SetData([Color.White]);

        if (ActiveMap.TryGetPlayerSpawn(out Vector2 spawn))
            _playerNode.SetFeetPosition(spawn);

        _notificationFeedNode.Push($"Sandbox ready: {_maps.Count} map(s), {_buildingDirectory.Buildings.Count} building placeholder(s).");
    }

    public void Update(EngineFrameContext context, Action exitGame)
    {
        HandleGlobalInputs(context, exitGame);
        _menuOverlayNode.UpdateInput(context);

        bool pauseByMenu = _menuOverlayNode.IsOpen &&
                           _menuSettings.PauseWorldWhileOpen &&
                           _sceneSettings.FreezeWorldWhileMenuOpen;
        bool pauseByDialogue = _dialogueNode.IsActive && _sceneSettings.FreezeWorldWhileDialogueOpen;
        bool pauseWorld = pauseByMenu || pauseByDialogue;

        if (_portalCooldownSeconds > 0f)
            _portalCooldownSeconds = Math.Max(0f, _portalCooldownSeconds - context.DeltaSeconds);

        ActiveMap.Update(context.GameTime);
        _dayNightNode.Update(context.DeltaSeconds, pauseWorld);
        _notificationFeedNode.Update(context.DeltaSeconds);

        if (_dialogueNode.IsActive)
        {
            if (context.Input.Pressed(_sceneSettings.ActionInputActionName))
                _dialogueNode.Advance();

            _cameraNode.Update(context, _playerNode, ActiveMap);
            return;
        }

        if (!pauseWorld)
            _playerNode.Update(context, ActiveMap);

        if (!pauseWorld &&
            !string.IsNullOrWhiteSpace(_debugSettings.AddMoneyInputActionName) &&
            context.Input.Pressed(_debugSettings.AddMoneyInputActionName))
        {
            _progressState.AddMoney(_debugSettings.AddMoneyAmount);
            _notificationFeedNode.Push($"+${_debugSettings.AddMoneyAmount} debug money");
        }

        if (!pauseWorld && context.Input.Pressed(_sceneSettings.ActionInputActionName))
        {
            if (UpdatePortalTransitions(context))
            {
                _cameraNode.Update(context, _playerNode, ActiveMap);
                return;
            }

            if (TryBeginNpcDialogue())
            {
                _cameraNode.Update(context, _playerNode, ActiveMap);
                return;
            }

            if (TrySleep())
            {
                _cameraNode.Update(context, _playerNode, ActiveMap);
                return;
            }
        }

        _cameraNode.Update(context, _playerNode, ActiveMap);
    }

    public void Draw(EngineFrameContext context)
    {
        Matrix view = context.Camera.GetViewMatrix();
        context.SpriteBatch.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        ActiveMap.DrawBackground(context.SpriteBatch, view, context.VirtualWidth, context.VirtualHeight);

        context.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: view);
        _ySortRenderer.Clear();
        foreach (IYSortDrawable overhang in ActiveMap.YSortForegroundDrawables)
            _ySortRenderer.Add(overhang);
        foreach (NpcNode npc in _npcRosterNode.GetNpcsForMap(_activeMapAssetName))
            _ySortRenderer.Add(npc);
        _ySortRenderer.Add(_playerNode);
        _ySortRenderer.Draw(context.SpriteBatch);
        DrawPortalDebug(context.SpriteBatch);
        context.SpriteBatch.End();

        ActiveMap.DrawForeground(view);
    }

    public void DrawScreen(EngineFrameContext context)
    {
        context.SpriteBatch.Begin();
        Viewport viewport = context.SpriteBatch.GraphicsDevice.Viewport;
        int screenWidth = viewport.Width;
        int screenHeight = viewport.Height;

        _dayNightNode.DrawScreen(context.SpriteBatch, screenWidth, screenHeight);
        _hudNode.DrawScreen(
            context.SpriteBatch,
            screenWidth,
            _progressState.DayNumber,
            _dayNightNode.CurrentClockText,
            _progressState.Money,
            _progressState.Level,
            !_menuOverlayNode.IsOpen);
        _notificationFeedNode.DrawScreen(context.SpriteBatch);
        _dialogueNode.DrawScreen(context.SpriteBatch, screenWidth, screenHeight);
        _menuOverlayNode.DrawScreen(
            context.SpriteBatch,
            screenWidth,
            screenHeight,
            _progressState,
            _buildingDirectory.Buildings,
            _activeMapAssetName);
        context.SpriteBatch.End();
    }

    private MapNode ActiveMap => _maps[_activeMapAssetName];

    private static ScenePortal[] BuildPortals(SceneSettings sceneSettings)
    {
        var result = new List<ScenePortal>();
        var dedupe = new HashSet<string>(StringComparer.Ordinal);

        foreach (PortalSettings portal in sceneSettings.Portals)
            TryAddPortal(portal.SourceMapAssetName, portal.TriggerObjectName, portal.TargetMapAssetName, portal.TargetSpawnObjectName);

        foreach (BuildingSettings building in sceneSettings.Buildings)
        {
            TryAddPortal(
                building.ExteriorMapAssetName,
                building.EnterTriggerObjectName,
                building.InteriorMapAssetName,
                building.InteriorSpawnObjectName);
            TryAddPortal(
                building.InteriorMapAssetName,
                building.ExitTriggerObjectName,
                building.ExteriorMapAssetName,
                building.ExteriorSpawnObjectName);
        }

        return result.ToArray();

        void TryAddPortal(string sourceMap, string triggerObject, string targetMap, string targetSpawn)
        {
            if (string.IsNullOrWhiteSpace(sourceMap) ||
                string.IsNullOrWhiteSpace(triggerObject) ||
                string.IsNullOrWhiteSpace(targetMap) ||
                string.IsNullOrWhiteSpace(targetSpawn))
            {
                return;
            }

            string key = $"{sourceMap}|{triggerObject}|{targetMap}|{targetSpawn}";
            if (!dedupe.Add(key))
                return;

            result.Add(new ScenePortal(sourceMap, triggerObject, targetMap, targetSpawn));
        }
    }

    private void HandleGlobalInputs(EngineFrameContext context, Action exitGame)
    {
        if (!string.IsNullOrWhiteSpace(_debugSettings.ToggleDebugLinesInputActionName) &&
            context.Input.Pressed(_debugSettings.ToggleDebugLinesInputActionName))
        {
            _isPortalDebugOverlayEnabled = !_isPortalDebugOverlayEnabled;
            _notificationFeedNode.Push(_isPortalDebugOverlayEnabled ? "Portal debug enabled" : "Portal debug disabled");
        }

        if (!string.IsNullOrWhiteSpace(_menuSettings.ToggleInputActionName) &&
            context.Input.Pressed(_menuSettings.ToggleInputActionName))
        {
            _menuOverlayNode.Toggle();
        }

        if (!context.Input.Pressed(_sceneSettings.ExitInputActionName))
            return;

        if (_menuOverlayNode.IsOpen)
        {
            _menuOverlayNode.Close();
            return;
        }

        if (_dialogueNode.IsActive)
        {
            _dialogueNode.Close();
            return;
        }

        exitGame();
    }

    private void EnsureMapRegistered(string mapAssetName)
    {
        if (_maps.ContainsKey(mapAssetName))
            return;

        _maps.Add(mapAssetName, new MapNode(mapAssetName, _mapProfile));
    }

    private bool UpdatePortalTransitions(EngineFrameContext context)
    {
        if (_portalCooldownSeconds > 0f)
            return false;

        foreach (ScenePortal portal in _portals)
        {
            if (!string.Equals(portal.SourceMapAssetName, _activeMapAssetName, StringComparison.Ordinal))
                continue;

            if (!ActiveMap.TryGetObjectRectangle(portal.TriggerObjectName, out Rectangle triggerArea))
                continue;

            if (!triggerArea.Intersects(_playerNode.DoorInteractionBounds))
                continue;

            if (!MovePlayerToMap(portal.TargetMapAssetName, portal.TargetSpawnObjectName))
                return false;

            _portalCooldownSeconds = _sceneSettings.PortalTransitionCooldownSeconds;

            if (_buildingDirectory.TryGetBuildingByEnterTrigger(portal.SourceMapAssetName, portal.TriggerObjectName, out BuildingSettings? building))
            {
                _notificationFeedNode.Push($"Entered {building!.DisplayName}");
            }
            else if (_buildingDirectory.TryGetBuildingByInteriorMap(portal.SourceMapAssetName, out BuildingSettings? sourceBuilding))
            {
                _notificationFeedNode.Push($"Exited {sourceBuilding!.DisplayName}");
            }

            return true;
        }

        return false;
    }

    private bool MovePlayerToMap(string mapAssetName, string spawnObjectName)
    {
        if (!_maps.TryGetValue(mapAssetName, out MapNode? destinationMap))
            return false;

        _activeMapAssetName = mapAssetName;

        if (destinationMap.TryGetObjectAnchorPosition(spawnObjectName, out Vector2 spawnPosition))
        {
            _playerNode.SetFeetPosition(spawnPosition);
            return true;
        }

        if (destinationMap.TryGetPlayerSpawn(out Vector2 fallbackSpawn))
        {
            _playerNode.SetFeetPosition(fallbackSpawn);
            return true;
        }

        return true;
    }

    private bool TryBeginNpcDialogue()
    {
        if (!_npcRosterNode.TryFindInteractableNpc(_activeMapAssetName, _playerNode.FeetPosition, out NpcNode? npc) ||
            npc is null)
        {
            return false;
        }

        _dialogueNode.StartDialogue(npc.DisplayName, npc.DialogueLines);
        return true;
    }

    private bool TrySleep()
    {
        if (!_sleepSettings.Enabled)
            return false;

        // Never treat an active door interaction as a sleep interaction.
        if (IsPlayerInsideAnyPortalTrigger())
            return false;

        foreach (SleepSpotSettings spot in _sleepSettings.Spots)
        {
            if (!string.Equals(spot.MapAssetName, _activeMapAssetName, StringComparison.Ordinal))
                continue;

            if (!IsPlayerInsideSleepSpot(spot))
                continue;

            if (!_sleepSettings.AllowSleepAnytime &&
                !_dayNightNode.IsWithinRange(_sleepSettings.EarliestSleepMinutes, _sleepSettings.LatestSleepMinutes))
            {
                _notificationFeedNode.Push("You are not tired enough yet.");
                return true;
            }

            _progressState.AdvanceDay();
            _dayNightNode.SetCurrentMinutes(_sleepSettings.WakeMinutes);
            _notificationFeedNode.Push($"Slept until {_dayNightNode.CurrentClockText}. Day {_progressState.DayNumber}");
            return true;
        }

        return false;
    }

    private bool IsPlayerInsideSleepSpot(SleepSpotSettings spot)
    {
        if (!string.IsNullOrWhiteSpace(spot.TriggerObjectName) &&
            ActiveMap.TryGetObjectRectangle(spot.TriggerObjectName, out Rectangle areaFromMap))
        {
            return areaFromMap.Intersects(_playerNode.DoorInteractionBounds);
        }

        if (spot.FallbackRadius <= 0f)
            return false;

        Vector2 fallbackFeetPosition = new(spot.FallbackX, spot.FallbackY);
        return Vector2.DistanceSquared(_playerNode.FeetPosition, fallbackFeetPosition) <=
               spot.FallbackRadius * spot.FallbackRadius;
    }

    private bool IsPlayerInsideAnyPortalTrigger()
    {
        Rectangle interactionBounds = _playerNode.DoorInteractionBounds;
        foreach (ScenePortal portal in _portals)
        {
            if (!string.Equals(portal.SourceMapAssetName, _activeMapAssetName, StringComparison.Ordinal))
                continue;

            if (!ActiveMap.TryGetObjectRectangle(portal.TriggerObjectName, out Rectangle triggerArea))
                continue;

            if (triggerArea.Intersects(interactionBounds))
                return true;
        }

        return false;
    }

    private static IReadOnlyDictionary<string, string> BuildInputKeyMap(InputSettings inputSettings)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (InputBindingSettings binding in inputSettings.Bindings)
        {
            if (string.IsNullOrWhiteSpace(binding.Action) || string.IsNullOrWhiteSpace(binding.Key))
                continue;

            string keyLabel = FormatKeyLabel(binding.Key);
            if (!result.TryGetValue(binding.Action, out string? existing))
            {
                result[binding.Action] = keyLabel;
                continue;
            }

            if (existing.IndexOf(keyLabel, StringComparison.OrdinalIgnoreCase) >= 0)
                continue;

            result[binding.Action] = $"{existing}/{keyLabel}";
        }

        return result;
    }

    private static string FormatKeyLabel(string rawKey)
    {
        return rawKey.Trim() switch
        {
            "LeftShift" => "Shift",
            "RightShift" => "Shift",
            "LeftControl" => "Ctrl",
            "RightControl" => "Ctrl",
            "LeftAlt" => "Alt",
            "RightAlt" => "Alt",
            "Back" => "Backspace",
            "Return" => "Enter",
            "OemQuestion" => "?",
            "OemComma" => ",",
            "OemPeriod" => ".",
            _ => rawKey.Trim()
        };
    }

    private readonly record struct ScenePortal(
        string SourceMapAssetName,
        string TriggerObjectName,
        string TargetMapAssetName,
        string TargetSpawnObjectName);

    private void DrawPortalDebug(SpriteBatch spriteBatch)
    {
        if (!_isPortalDebugOverlayEnabled)
            return;

        Rectangle interactionBounds = _playerNode.DoorInteractionBounds;
        DrawRectangleOutline(spriteBatch, interactionBounds, new Color(80, 200, 255, 220));

        foreach (ScenePortal portal in _portals)
        {
            if (!string.Equals(portal.SourceMapAssetName, _activeMapAssetName, StringComparison.Ordinal))
                continue;

            if (!ActiveMap.TryGetObjectRectangle(portal.TriggerObjectName, out Rectangle triggerArea))
                continue;

            bool isInside = triggerArea.Intersects(interactionBounds);
            Color fillColor = isInside ? new Color(40, 210, 85, 90) : new Color(220, 65, 65, 80);
            Color outlineColor = isInside ? new Color(20, 140, 40, 180) : new Color(180, 35, 35, 180);
            spriteBatch.Draw(_debugPixel, triggerArea, fillColor);
            DrawRectangleOutline(spriteBatch, triggerArea, outlineColor);
        }

        DrawNpcInteractionDebug(spriteBatch);
        DrawSleepDebug(spriteBatch);
    }

    private void DrawNpcInteractionDebug(SpriteBatch spriteBatch)
    {
        _npcRosterNode.TryFindInteractableNpc(_activeMapAssetName, _playerNode.FeetPosition, out NpcNode? interactableNpc);

        foreach (NpcNode npc in _npcRosterNode.GetNpcsForMap(_activeMapAssetName))
        {
            float interactionRange = npc.InteractionRange > 0f ? npc.InteractionRange : _interactionSettings.NpcInteractionRange;
            Rectangle rangeRect = RectangleFromCenter(npc.FeetPosition, interactionRange);
            bool inRange = npc.IsInInteractionRange(_playerNode.FeetPosition, _interactionSettings.NpcInteractionRange);
            bool isFocused = ReferenceEquals(interactableNpc, npc);

            Color fillColor = isFocused
                ? new Color(255, 228, 70, 90)
                : inRange
                    ? new Color(90, 205, 255, 70)
                    : new Color(140, 140, 140, 45);
            Color outlineColor = isFocused
                ? new Color(255, 228, 70, 190)
                : inRange
                    ? new Color(90, 205, 255, 170)
                    : new Color(140, 140, 140, 140);

            spriteBatch.Draw(_debugPixel, rangeRect, fillColor);
            DrawRectangleOutline(spriteBatch, rangeRect, outlineColor);

            Rectangle feetMarker = new(
                (int)MathF.Round(npc.FeetPosition.X) - 1,
                (int)MathF.Round(npc.FeetPosition.Y) - 1,
                3,
                3);
            spriteBatch.Draw(_debugPixel, feetMarker, Color.Yellow);
        }
    }

    private void DrawSleepDebug(SpriteBatch spriteBatch)
    {
        foreach (SleepSpotSettings spot in _sleepSettings.Spots)
        {
            if (!string.Equals(spot.MapAssetName, _activeMapAssetName, StringComparison.Ordinal))
                continue;

            bool isInside = IsPlayerInsideSleepSpot(spot);
            Color fillColor = isInside ? new Color(170, 120, 255, 95) : new Color(170, 120, 255, 45);
            Color outlineColor = isInside ? new Color(212, 184, 255, 200) : new Color(170, 120, 255, 155);

            if (!string.IsNullOrWhiteSpace(spot.TriggerObjectName) &&
                ActiveMap.TryGetObjectRectangle(spot.TriggerObjectName, out Rectangle triggerArea))
            {
                spriteBatch.Draw(_debugPixel, triggerArea, fillColor);
                DrawRectangleOutline(spriteBatch, triggerArea, outlineColor);
                continue;
            }

            Rectangle fallbackRect = RectangleFromCenter(new Vector2(spot.FallbackX, spot.FallbackY), spot.FallbackRadius);
            spriteBatch.Draw(_debugPixel, fallbackRect, fillColor);
            DrawRectangleOutline(spriteBatch, fallbackRect, outlineColor);
        }
    }

    private static Rectangle RectangleFromCenter(Vector2 center, float radius)
    {
        int size = Math.Max(1, (int)MathF.Round(radius * 2f));
        int left = (int)MathF.Round(center.X - radius);
        int top = (int)MathF.Round(center.Y - radius);
        return new Rectangle(left, top, size, size);
    }

    private void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), color);
    }
}
