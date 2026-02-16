using Microsoft.Xna.Framework;

namespace Sandbox.Game;

internal sealed class CameraNode
{
    public void Update(Engine.Core.EngineFrameContext context, PlayerNode playerNode, MapNode mapNode)
    {
        Vector2 cameraTarget = playerNode.Position +
                               new Vector2(playerNode.CurrentFrameWidth * 0.5f, playerNode.CurrentFrameHeight * 0.5f);

        cameraTarget = mapNode.ClampCameraTarget(cameraTarget, context.VirtualWidth, context.VirtualHeight);
        context.Camera.LookAt(cameraTarget);
    }
}
