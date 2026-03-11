using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Sandbox.Game.Config;
using Sandbox.Game.Scene;

namespace Sandbox.Game;

internal sealed class CameraNode
{
    private const float EngineMinZoom = 0.25f;
    private const float EngineMaxZoom = 4f;
    private const float MinZoomStep = 0.01f;
    private const float MaxZoomStep = 0.25f;

    private readonly float _minZoom;
    private readonly float _maxZoom;
    private readonly float _zoomSpeed;
    private float _zoom;

    public CameraNode(CameraSettings settings, float startingZoom)
    {
        _minZoom = Math.Clamp(settings.MinZoom, EngineMinZoom, EngineMaxZoom);
        _maxZoom = Math.Clamp(settings.MaxZoom, _minZoom, EngineMaxZoom);
        _zoomSpeed = Math.Clamp(settings.ZoomSpeed, MinZoomStep, MaxZoomStep);
        _zoom = SnapToZoomStep(startingZoom);
    }

    public void Update(EngineFrameContext context, PlayerNode playerNode, MapNode mapNode)
    {
        int zoomInput = 0;
        if (context.Input.ScrollingUp)
            zoomInput++;
        if (context.Input.ScrollingDown)
            zoomInput--;

        if (zoomInput != 0)
            _zoom = SnapToZoomStep(_zoom + zoomInput * _zoomSpeed);

        context.Camera.Zoom = _zoom;

        Vector2 cameraTarget = playerNode.Position +
                               new Vector2(playerNode.CurrentFrameWidth * 0.5f, playerNode.CurrentFrameHeight * 0.5f);

        int clampedViewportWidth = (int)MathF.Ceiling(context.VirtualWidth / _zoom);
        int clampedViewportHeight = (int)MathF.Ceiling(context.VirtualHeight / _zoom);

        cameraTarget = mapNode.ClampCameraTarget(cameraTarget, clampedViewportWidth, clampedViewportHeight);
        context.Camera.LookAt(cameraTarget);
    }

    private float SnapToZoomStep(float zoom)
    {
        float clampedZoom = Math.Clamp(zoom, _minZoom, _maxZoom);
        float stepCount = MathF.Round((clampedZoom - _minZoom) / _zoomSpeed);
        float snappedZoom = _minZoom + stepCount * _zoomSpeed;
        return Math.Clamp(snappedZoom, _minZoom, _maxZoom);
    }
}
