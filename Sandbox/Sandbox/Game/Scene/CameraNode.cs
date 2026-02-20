using System;
using Microsoft.Xna.Framework;
using Sandbox.Game.Scene;

namespace Sandbox.Game;

internal sealed class CameraNode
{
    private readonly float _zoom;

    public CameraNode(float zoom)
    {
        _zoom = zoom;
    }

    public void Update(Engine.Core.EngineFrameContext context, PlayerNode playerNode, MapNode mapNode)
    {
        context.Camera.Zoom = _zoom;

        Vector2 cameraTarget = playerNode.Position +
                               new Vector2(playerNode.CurrentFrameWidth * 0.5f, playerNode.CurrentFrameHeight * 0.5f);

        int clampedViewportWidth = (int)MathF.Ceiling(context.VirtualWidth / _zoom);
        int clampedViewportHeight = (int)MathF.Ceiling(context.VirtualHeight / _zoom);

        cameraTarget = mapNode.ClampCameraTarget(cameraTarget, clampedViewportWidth, clampedViewportHeight);
        context.Camera.LookAt(cameraTarget);
    }
}
