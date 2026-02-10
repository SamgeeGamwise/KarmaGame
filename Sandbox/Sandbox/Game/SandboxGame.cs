using Engine.Core;
using Engine.Input;
using Engine.Scene;
using Microsoft.Xna.Framework.Input;

namespace Sandbox.Game;
public sealed class SandboxGame : EngineGame
{
    public SandboxGame() : base(640, 360, useVirtualResolution: true) { }

    protected override void ConfigureInput(InputMap input)
    {
        input.BindKey("move_left", Keys.A);
        input.BindKey("move_right", Keys.D);
        input.BindKey("move_up", Keys.W);
        input.BindKey("move_down", Keys.S);
        input.BindKey("ui_accept", Keys.Enter);
    }

    protected override Node CreateInitialScene() => new MyRootScene();
}