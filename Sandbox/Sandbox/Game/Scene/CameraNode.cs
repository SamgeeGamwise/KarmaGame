using System;
using Engine.Core;
using Microsoft.Xna.Framework;
using Sandbox.Game.Config;
using Sandbox.Game.Scene;

namespace Sandbox.Game;

internal sealed class CameraNode(CameraSettings settings, float startingZoom)
{
    private float _zoom = startingZoom;
    private readonly float _zoomSpeed = settings.ZoomSpeed;

    public void Update(EngineFrameContext context, PlayerNode playerNode, MapNode mapNode)
    {
        _zoom += context.Input.ScrollingUp ? _zoomSpeed : 0;
        _zoom += context.Input.ScrollingDown ? -_zoomSpeed : 0;
        context.Camera.Zoom = _zoom;

        Vector2 cameraTarget = playerNode.Position +
                               new Vector2(playerNode.CurrentFrameWidth * 0.5f, playerNode.CurrentFrameHeight * 0.5f);

        int clampedViewportWidth = (int)MathF.Ceiling(context.VirtualWidth / _zoom);
        int clampedViewportHeight = (int)MathF.Ceiling(context.VirtualHeight / _zoom);

        cameraTarget = mapNode.ClampCameraTarget(cameraTarget, clampedViewportWidth, clampedViewportHeight);
        context.Camera.LookAt(cameraTarget);
    }
}
