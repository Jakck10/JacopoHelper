// Based on OuiHelper_EnterJournal.cs from CollabUtils2
// https://github.com/EverestAPI/CelesteCollabUtils2
// Copyright (c) 2020 Everest Team, MIT License (see THIRD-PARTY-LICENSES.md)

using System.Collections;
using Monocle;

namespace Celeste.Mod.JacopoHelper;

public class OuiHelper_EnterJournal : Oui {
    public static bool Start;

    public override bool IsStart(Overworld overworld, Overworld.StartMode start) {
        if (Start) {
            Start = false;
            Add(new Coroutine(Enter(null)));
            return true;
        }
        return false;
    }

    public override IEnumerator Enter(Oui from) {
        Audio.Play("event:/ui/world_map/journal/select");
        Overworld.Goto<OuiJournal>();
        yield break;
    }

    public override IEnumerator Leave(Oui next) {
        yield break;
    }
}