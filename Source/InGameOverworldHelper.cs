// Based on InGameOverworldHelper.cs from CollabUtils2
// https://github.com/EverestAPI/CelesteCollabUtils2
// Copyright (c) 2020 Everest Team, MIT License (see THIRD-PARTY-LICENSES.md)

using System;
using System.Collections;
using System.Reflection;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.JacopoHelper;

public static class InGameOverworldHelper {
    public static bool IsOpen => overworldWrapper?.Scene == Engine.Scene;

    private static SceneWrappingEntity<Overworld> overworldWrapper;
    private static Hook onReloadLevelHook;

    internal static void Load() {
        Everest.Events.Level.OnPause += OnPause;
        On.Celeste.Audio.SetMusic += OnSetMusic;
        On.Celeste.Audio.SetAmbience += OnSetAmbience;
        On.Celeste.Player.Die += OnPlayerDie;
        On.Celeste.OuiMainMenu.CreateButtons += OnOuiMainMenuCreateButtons;

        onReloadLevelHook = new Hook(
            typeof(AssetReloadHelper).GetMethod("ReloadLevel", new Type[0]),
            typeof(InGameOverworldHelper).GetMethod(nameof(OnReloadLevel), BindingFlags.NonPublic | BindingFlags.Static));
    }

    internal static void Unload() {
        Everest.Events.Level.OnPause -= OnPause;
        On.Celeste.Audio.SetMusic -= OnSetMusic;
        On.Celeste.Audio.SetAmbience -= OnSetAmbience;
        On.Celeste.Player.Die -= OnPlayerDie;
        On.Celeste.OuiMainMenu.CreateButtons -= OnOuiMainMenuCreateButtons;

        onReloadLevelHook?.Dispose();
        onReloadLevelHook = null;
    }

    private static void OnPause(Level level, int startIndex, bool minimal, bool quickReset) {
        if (overworldWrapper != null)
            Close(level, true, true);
    }

    private static PlayerDeadBody OnPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats) {
        PlayerDeadBody result = orig(self, direction, evenIfInvincible, registerDeathInStats);
        if (result != null && self.Scene is Level level && overworldWrapper != null)
            Close(level, true, true);
        return result;
    }

    private static void OnReloadLevel(Action orig) {
        if (overworldWrapper != null) {
            if (!(Engine.Scene is Level level))
                level = AssetReloadHelper.ReturnToScene as Level;

            Player player = level?.Tracker.GetEntity<Player>();
            if (player != null && player.StateMachine.State == Player.StDummy)
                player.StateMachine.State = Player.StNormal;
        }
        orig();
    }

    private static bool OnSetMusic(On.Celeste.Audio.orig_SetMusic orig, string path, bool startPlaying, bool allowFadeOut) {
        if (path != null && overworldWrapper?.Scene == Engine.Scene)
            return false;
        return orig(path, startPlaying, allowFadeOut);
    }

    private static bool OnSetAmbience(On.Celeste.Audio.orig_SetAmbience orig, string path, bool startPlaying) {
        if (path != null && overworldWrapper?.Scene == Engine.Scene)
            return false;
        return orig(path, startPlaying);
    }

    public static void OpenJournal(Player player, AreaKey area) {
        player.Drop();

        OuiHelper_EnterJournal.Start = true;
        JacopoHelperModule.OpenCustom = true;
        JacopoHelperModule.JournalArea = area;

        Open(player, out bool opened);

        if (!opened) {
            OuiHelper_EnterJournal.Start = false;
            JacopoHelperModule.OpenCustom = false;
        }
    }

    private static void OnOuiMainMenuCreateButtons(On.Celeste.OuiMainMenu.orig_CreateButtons orig, OuiMainMenu self) {
        orig(self);

        if (Engine.Scene is Overworld) {
            OuiHelper_EnterJournal.Start = false;
            JacopoHelperModule.OpenCustom = false;
        }
    }

    private static void Open(Player player, out bool opened) {
        opened = false;

        if (overworldWrapper?.Scene == Engine.Scene || player.StateMachine.State == Player.StDummy)
            return;
        player.StateMachine.State = Player.StDummy;
        opened = true;

        Level level = player.Scene as Level;
        var strawberryDisplay = level.Entities.FindFirst<TotalStrawberriesDisplay>();
        if (strawberryDisplay != null)
            strawberryDisplay.Active = false;

        overworldWrapper = new SceneWrappingEntity<Overworld>(new Overworld(new OverworldLoader((Overworld.StartMode) (-1),
            new HiresSnow() {
                Alpha = 0f,
                ParticleAlpha = 0.25f,
            }
        )));
        overworldWrapper.OnBegin += overworld => {
            overworld.RendererList.Remove(overworld.RendererList.Renderers.Find(r => r is MountainRenderer));
            overworld.RendererList.Remove(overworld.RendererList.Renderers.Find(r => r is ScreenWipe));
            overworld.RendererList.UpdateLists();
        };
        overworldWrapper.OnEnd += overworld => {
            if (overworldWrapper?.WrappedScene == overworld) {
                OuiHelper_EnterJournal.Start = false;
                JacopoHelperModule.OpenCustom = false;
                overworldWrapper = null;
                }
        };

        level.Add(overworldWrapper);
        overworldWrapper.Add(new Coroutine(UpdateRoutine(overworldWrapper)));
    }

    public static void Close(Level level, bool removeScene, bool resetPlayer) {
        if (removeScene) {
            overworldWrapper?.WrappedScene?.Entities.UpdateLists();
            overworldWrapper?.RemoveSelf();

            var strawberryDisplay = level.Entities.FindFirst<TotalStrawberriesDisplay>();
            if (strawberryDisplay != null)
                strawberryDisplay.Active = true;
        }

        if (resetPlayer) {
            Player player = level.Tracker.GetEntity<Player>();
            if (player != null && player.StateMachine.State == Player.StDummy) {
                Engine.Scene.OnEndOfFrame += () => {
                    player.StateMachine.State = Player.StNormal;
                };
            }
        }
    }

    private static IEnumerator DelayedCloseRoutine(Level level) {
        yield return null;
        Close(level, false, true);
    }

    private static IEnumerator UpdateRoutine(SceneWrappingEntity<Overworld> wrapper) {
        Level level = wrapper.Scene as Level;
        Overworld overworld = wrapper.WrappedScene;

        while (overworldWrapper?.Scene == Engine.Scene) {
            if (overworld.Next is OuiChapterSelect) {
                overworld.Next.RemoveSelf();
                overworldWrapper.Add(new Coroutine(DelayedCloseRoutine(level)));
            }

            overworld.Snow.ParticleAlpha = 0.25f;

            if (overworld.Current != null || overworld.Next?.Scene != null) {
                overworld.Snow.Alpha = Calc.Approach(overworld.Snow.Alpha, 1f, Engine.DeltaTime * 2f);
            } else {
                overworld.Snow.Alpha = Calc.Approach(overworld.Snow.Alpha, 0, Engine.DeltaTime * 2f);
                if (overworld.Snow.Alpha <= 0.01f)
                    Close(level, true, true);
            }

            yield return null;
        }

        if (wrapper.Scene != null)
            wrapper.RemoveSelf();
    }
}
