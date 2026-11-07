using System;

namespace Celeste.Mod.JacopoHelper;

public class JacopoHelperModule : EverestModule {
    public static bool OpenCustom;
    public static AreaKey? JournalArea;
    public static JacopoHelperModule Instance { get; private set; }

    public override Type SettingsType => typeof(JacopoHelperModuleSettings);
    public static JacopoHelperModuleSettings Settings => (JacopoHelperModuleSettings) Instance._Settings;

    public override Type SessionType => typeof(JacopoHelperModuleSession);
    public static JacopoHelperModuleSession Session => (JacopoHelperModuleSession) Instance._Session;

    public override Type SaveDataType => typeof(JacopoHelperModuleSaveData);
    public static JacopoHelperModuleSaveData SaveData => (JacopoHelperModuleSaveData) Instance._SaveData;

    public JacopoHelperModule() {
        Instance = this;
#if DEBUG
        // debug builds use verbose logging
        Logger.SetLogLevel(nameof(JacopoHelperModule), LogLevel.Verbose);
#else
        // release builds use info logging to reduce spam in log files
        Logger.SetLogLevel(nameof(JacopoHelperModule), LogLevel.Info);
#endif
    }

    public override void Load() {
        Everest.Events.Journal.OnEnter += OnJournalEnter;
        InGameOverworldHelper.Load();
    }

    public override void Unload() {
        Everest.Events.Journal.OnEnter -= OnJournalEnter;
        InGameOverworldHelper.Unload();
    }

    private static void OnJournalEnter(OuiJournal journal, Oui from) {
        Logger.Info("JacopoHelper", $"OnJournalEnter: OpenCustom={OpenCustom}, JournalArea={JournalArea}, pagine prima={journal.Pages.Count}");
        if (!OpenCustom) return;

        journal.Pages.Clear();
        journal.Pages.Add(new OuiJournalCover(journal));

        var area = JournalArea ?? global::Celeste.SaveData.Instance.LastArea;
        var list = JournalData.Build(area);
        Logger.Info("JacopoHelper", $"Checkpoint trovati: {list.Count}");
        for (int i = 0; i < list.Count; i += 2)
            journal.Pages.Add(new OuiJournalCheckpoints(journal, list[i], i + 1 < list.Count ? list[i + 1] : null));

        Logger.Info("JacopoHelper", $"pagine dopo={journal.Pages.Count}");
        OpenCustom = false;
    }
}