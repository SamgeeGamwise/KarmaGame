using System;
using System.Collections.Generic;
using Engine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Sandbox.Game.Config;
using Sandbox.Game.Scene;

namespace Sandbox.Game;

internal sealed class SandboxScene
{
    private readonly SceneSettings _sceneSettings;
    private readonly TiledMapAuthoringProfile _mapProfile;
    private readonly Dictionary<string, MapNode> _maps = new(StringComparer.Ordinal);
    private readonly PlayerNode _playerNode;
    private readonly CameraNode _cameraNode;
    private readonly DayNightNode _dayNightNode;
    private readonly YSortRenderer _ySortRenderer = new();
    private readonly ScenePortal[] _portals;

    private string _activeMapAssetName;
    private float _portalCooldownSeconds;
    private bool _isPortalDebugOverlayEnabled;
    private Texture2D _debugPixel = null!;

    public SandboxScene(SandboxGameSettings settings, TiledMapAuthoringProfile mapProfile)
    {
        _sceneSettings = settings.Scene;
        _mapProfile = mapProfile;
        _cameraNode = new CameraNode(_sceneSettings.CameraZoom);
        _activeMapAssetName = _sceneSettings.StartingMapAssetName;
        _isPortalDebugOverlayEnabled = _sceneSettings.DrawPortalDebugOverlay;
        _portals = _sceneSettings.Portals
            .ConvertAll(portal => new ScenePortal(
                portal.SourceMapAssetName,
                portal.TriggerObjectName,
                portal.TargetMapAssetName,
                portal.TargetSpawnObjectName))
            .ToArray();
        _playerNode = new PlayerNode(settings.Player);
        _dayNightNode = new DayNightNode(settings.DayNight);
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        EnsureMapRegistered(_activeMapAssetName);
        foreach (ScenePortal portal in _portals)
        {
            EnsureMapRegistered(portal.SourceMapAssetName);
            EnsureMapRegistered(portal.TargetMapAssetName);
        }

        foreach (MapNode mapNode in _maps.Values)
            mapNode.LoadContent(content, graphicsDevice);

        _playerNode.LoadContent(content);
        _dayNightNode.LoadContent(content, graphicsDevice);
        _debugPixel = new Texture2D(graphicsDevice, 1, 1);
        _debugPixel.SetData([Color.White]);

        if (ActiveMap.TryGetPlayerSpawn(out Vector2 spawn))
            _playerNode.SetFeetPosition(spawn);
    }

    public void Update(EngineFrameContext context, Action exitGame)
    {
        ActiveMap.Update(context.GameTime);
        _dayNightNode.Update(context.DeltaSeconds);

        if (context.Input.Down(_sceneSettings.ExitInputActionName))
            exitGame();

        if (!string.IsNullOrWhiteSpace(_sceneSettings.DebugToggleInputActionName) &&
            context.Input.Pressed(_sceneSettings.DebugToggleInputActionName))
        {
            _isPortalDebugOverlayEnabled = !_isPortalDebugOverlayEnabled;
        }

        _playerNode.Update(context, ActiveMap);
        UpdatePortalTransitions(context);
        _cameraNode.Update(context, _playerNode, ActiveMap);
    }

    public void Draw(EngineFrameContext context)
    {
        Matrix view = context.Camera.GetViewMatrix();
        ActiveMap.DrawBackground(context.SpriteBatch, view, context.VirtualWidth, context.VirtualHeight);

        context.SpriteBatch.Begin(transformMatrix: view);
        _ySortRenderer.Clear();
        _ySortRenderer.Add(_playerNode);
        _ySortRenderer.Draw(context.SpriteBatch);
        DrawPortalDebug(context.SpriteBatch);
        context.SpriteBatch.End();

        ActiveMap.DrawForeground(view);
    }

    public void DrawScreen(EngineFrameContext context)
    {
        context.SpriteBatch.Begin();
        _dayNightNode.DrawScreen(context.SpriteBatch, context.VirtualWidth, context.VirtualHeight);
        context.SpriteBatch.End();
    }

    private MapNode ActiveMap => _maps[_activeMapAssetName];

    private void EnsureMapRegistered(string mapAssetName)
    {
        if (_maps.ContainsKey(mapAssetName))
            return;

        _maps.Add(mapAssetName, new MapNode(mapAssetName, _mapProfile));
    }

    private void UpdatePortalTransitions(EngineFrameContext context)
    {
        if (_portalCooldownSeconds > 0f)
        {
            _portalCooldownSeconds = Math.Max(0f, _portalCooldownSeconds - context.DeltaSeconds);
            return;
        }

        // Require explicit interaction so door zones don't auto-trigger while moving.
        if (!context.Input.Pressed(_sceneSettings.ActionInputActionName))
            return;

        foreach (ScenePortal portal in _portals)
        {
            if (!string.Equals(portal.SourceMapAssetName, _activeMapAssetName, StringComparison.Ordinal))
                continue;

            if (!ActiveMap.TryGetObjectRectangle(portal.TriggerObjectName, out Rectangle triggerArea))
                continue;

            if (!triggerArea.Intersects(_playerNode.DoorInteractionBounds))
                continue;

            MovePlayerToMap(portal.TargetMapAssetName, portal.TargetSpawnObjectName);
            _portalCooldownSeconds = _sceneSettings.PortalTransitionCooldownSeconds;
            break;
        }
    }

    private void MovePlayerToMap(string mapAssetName, string spawnObjectName)
    {
        if (!_maps.TryGetValue(mapAssetName, out MapNode? destinationMap))
            return;

        _activeMapAssetName = mapAssetName;

        if (destinationMap.TryGetObjectAnchorPosition(spawnObjectName, out Vector2 spawnPosition))
        {
            _playerNode.SetFeetPosition(spawnPosition);
            return;
        }

        if (destinationMap.TryGetPlayerSpawn(out Vector2 fallbackSpawn))
            _playerNode.SetFeetPosition(fallbackSpawn);
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

    }

    private void DrawRectangleOutline(SpriteBatch spriteBatch, Rectangle rect, Color color)
    {
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Top, rect.Width, 1), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Bottom - 1, rect.Width, 1), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Left, rect.Top, 1, rect.Height), color);
        spriteBatch.Draw(_debugPixel, new Rectangle(rect.Right - 1, rect.Top, 1, rect.Height), color);
    }
}
