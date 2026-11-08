using System.Collections.Generic;

namespace Celeste.Mod.JacopoHelper;

public class BerryInfo {
    public bool Collected;
    public bool Untracked;
    public bool Ghost;
}

public class CheckpointInfo {
    public string Name;
    public string Image;
    public float Rotation;
    public Microsoft.Xna.Framework.Vector2 Offset;
    public List<BerryInfo> Berries = new();
}