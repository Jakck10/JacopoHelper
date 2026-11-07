using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.JacopoHelper;

[CustomEntity("JacopoHelper/JournalTrigger")]
public class JournalTrigger : Trigger {
    private readonly TalkComponent talkComponent;

    public JournalTrigger(EntityData data, Vector2 offset)
        : base(data, offset) {
        Add(talkComponent = new TalkComponent(
            new Rectangle(0, 0, data.Width, data.Height),
            data.Nodes.Length != 0 ? (data.Nodes[0] - data.Position) : new Vector2(data.Width / 2f, data.Height / 2f),
            player => InGameOverworldHelper.OpenJournal(player, SceneAs<Level>().Session.Area)
        ) { PlayerMustBeFacing = false });
    }

    public override void Update() {
        base.Update();
        talkComponent.Enabled = !InGameOverworldHelper.IsOpen;
    }
}