using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;

namespace Celeste.Mod.JacopoHelper;

public class OuiJournalCheckpoints : OuiJournalPage {
    private readonly CheckpointInfo left, right;

    public OuiJournalCheckpoints(OuiJournal journal, CheckpointInfo left, CheckpointInfo right)
        : base(journal) {
        PageTexture = "page";
        this.left = left;
        this.right = right;
    }

    public override void Redraw(VirtualRenderTarget buffer) {
        base.Redraw(buffer);
        float half = buffer.Width / 2f;
        DrawCheckpoint(left, half * 0.5f, half, buffer.Height);
        DrawCheckpoint(right, half * 1.5f, half, buffer.Height);
    }

    private void DrawCheckpoint(CheckpointInfo cp, float x, float halfWidth, float pageHeight) {
        if (cp == null) return;

        bool hasFrame = MTN.Checkpoints.Has("polaroid");
        MTexture polaroid = hasFrame ? MTN.Checkpoints["polaroid"] : null;
        MTexture img = MTN.Checkpoints.Has(cp.Image) ? MTN.Checkpoints[cp.Image] : null;

        // scala della cornice: occupa ~85% della metà pagina, ma mai più del 55% dell'altezza
        float s = 0.75f;
        float frameHeight = 300f;
        if (hasFrame) {
            s = Math.Min(halfWidth * 0.85f / polaroid.Width, pageHeight * 0.55f / polaroid.Height);
            frameHeight = polaroid.Height * s;
        }

        Vector2 center = new Vector2(x, 80f + frameHeight / 2f) + cp.Offset;

        // cornice (filtro normale)
        Draw.SpriteBatch.Begin();
        if (hasFrame)
            polaroid.DrawCentered(center, Color.White, s, cp.Rotation);
        Draw.SpriteBatch.End();

        // anteprima con PointClamp, così i pixel restano nitidi (come in vanilla)
        if (img != null) {
            float imgScale = s * 720f / img.Width;
            Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone);
            img.DrawCentered(center, Color.White, imgScale, cp.Rotation);
            Draw.SpriteBatch.End();
        }

        Draw.SpriteBatch.Begin();

        float y = 80f + frameHeight + 50f;
        ActiveFont.Draw(cp.Name, new Vector2(x, y), new Vector2(0.5f, 0f),
            Vector2.One * 1.1f, Color.Black * 0.8f);
        y += 120f;

        MTexture normal = GFX.Gui["collectables/strawberry"];
        MTexture untracked = GFX.Gui.Has("JacopoHelper/untrackedberry")
            ? GFX.Gui["JacopoHelper/untrackedberry"] : normal;

        float spacing = 53f;
        float startX = x - (cp.Berries.Count - 1) * spacing / 2f;
        for (int i = 0; i < cp.Berries.Count; i++) {
            BerryInfo b = cp.Berries[i];
            Color c = b.Collected ? Color.White : Color.Black * 0.3f;
            (b.Untracked ? untracked : normal).DrawCentered(new Vector2(startX + i * spacing, y + 25f), c, 0.5f);
        }

        Draw.SpriteBatch.End();
    }
}