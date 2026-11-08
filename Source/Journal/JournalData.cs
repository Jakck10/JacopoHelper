using System.Collections.Generic;
using Monocle;

namespace Celeste.Mod.JacopoHelper;

public static class JournalData {
    public static List<CheckpointInfo> Build(AreaKey area, bool onlyDisc) {
        ModeProperties mp = AreaData.Areas[area.ID].Mode[(int) area.Mode];
        AreaModeStats stats = SaveData.Instance.Areas[area.ID].Modes[(int) area.Mode];
        HashSet<string> found = SaveData.Instance.GetCheckpoints(area);

        var list = new List<CheckpointInfo>();
        var byIndex = new Dictionary<int, CheckpointInfo>();

        for (int i = 0; i <= mp.Checkpoints.Length; i++) {
            string level = i == 0 ? null : mp.Checkpoints[i - 1].Level;
            if (i > 0 && !found.Contains(level) && onlyDisc) continue;

            int num = i == 0 ? mp.StartStrawberries : mp.Checkpoints[i - 1].Strawberries;
            var info = new CheckpointInfo {
                Name = i == 0 ? Dialog.Clean("overworld_start") : AreaData.GetCheckpointName(area, level),
                Image = OuiChapterPanel.GetCheckpointPreviewName(area, level)
            };

            int seed = 23;
            foreach (char c in info.Image) seed = seed * 31 + c;
            var rng = new System.Random(seed);
            info.Rotation = (rng.Next(2) == 0 ? -1 : 1) * (0.05f + (float) rng.NextDouble() * 0.15f);
            info.Offset = new Microsoft.Xna.Framework.Vector2(rng.Next(-16, 17), rng.Next(-16, 17));

            for (int b = 0; b < num; b++) {
                EntityData e = mp.StrawberriesByCheckpoint[i, b];
                bool got = e != null && stats.Strawberries.Contains(new EntityID(e.Level.Name, e.ID));
                info.Berries.Add(new BerryInfo { Collected = got });
            }

            list.Add(info);
            byIndex[i] = info;
        }

        int cp = 0;
        foreach (LevelData level in mp.MapData.Levels) {
            for (int i = 0; i < mp.Checkpoints.Length; i++)
                if (mp.Checkpoints[i].Level == level.Name) cp = i + 1;

            if (!byIndex.TryGetValue(cp, out CheckpointInfo info)) continue;
            foreach (EntityData e in level.Entities) {
                if (e.Name == "JacopoHelper/UntrackedCoreBerry") {
                    info.Berries.Add(new BerryInfo {
                        Untracked = true,
                        Collected = stats.Strawberries.Contains(new EntityID(level.Name, e.ID))
                    });
                }
            }
        }

        return list;
    }
}
