using Microsoft.Xna.Framework.Graphics;

namespace Engine.Core;

public interface IYSortDrawable
{
    float YSort { get; }

    void Draw(SpriteBatch spriteBatch);
}
