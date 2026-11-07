using Celeste.Mod.Entities;
using FrostTempleHelper;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.JacopoHelper.Entities;

[CustomEntity("JacopoHelper/UntrackedCoreBerry")]
[RegisterStrawberry(false, false)]
public class UntrackedCoreBerry : CoreBerry {
    public UntrackedCoreBerry(EntityData data, Vector2 position, EntityID gid)
        : base(data, position, gid) { }
}