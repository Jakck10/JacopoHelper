using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JacopoHelper.Entities;

[CustomEntity("JacopoHelper/Firefly")]
public class Firefly : Entity
{
    private readonly int fireflyPerBlock;
    private readonly float brightness;

    public Firefly(EntityData data, Vector2 offset)
        : base(data.Position + offset)
    {
        Collider = new Hitbox(data.Width, data.Height);
        fireflyPerBlock = data.Int("fireflyPerBlock", 5);
        brightness = data.Float("brightness", 10f);
    }
}